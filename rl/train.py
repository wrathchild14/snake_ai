import math
import random
import time
from collections import namedtuple, deque

import torch
import torch.nn as nn
import torch.optim as optim
import torch.nn.functional as F

from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple

import numpy as np
from tqdm import tqdm
import matplotlib.pyplot as plt


def optimize_model():
    if len(memory) < BATCH_SIZE:
        return
    transitions = memory.sample(BATCH_SIZE)
    # Transpose the batch (see https://stackoverflow.com/a/19343/3343043 for
    # detailed explanation). This converts batch-array of Transitions
    # to Transition of batch-arrays.
    batch = Transition(*zip(*transitions))

    # Compute a mask of non-final states and concatenate the batch elements
    # (a final state would've been the one after which simulation ended)
    non_final_mask = torch.tensor(tuple(map(lambda s: s is not None,
                                            batch.next_state)), device=device, dtype=torch.bool)
    non_final_next_states = torch.cat([s for s in batch.next_state
                                       if s is not None])
    state_batch = torch.cat(batch.state)
    action_batch = torch.cat(batch.action)
    reward_batch = torch.cat(batch.reward)

    # Compute Q(s_t, a) - the model computes Q(s_t), then we select the
    # columns of actions taken. These are the actions which would've been taken
    # for each batch state according to policy_net
    # print(state_batch.size())
    # print(action_batch.size())
    state_batch = state_batch.squeeze(1)
    state_action_values = policy_net(state_batch).gather(1, action_batch)
    # state_action_values = policy_net(state_batch).gather(1, action_batch.unsqueeze(-1))

    # Compute V(s_{t+1}) for all next states.
    # Expected values of actions for non_final_next_states are computed based
    # on the "older" target_net; selecting their best reward with max(1).values
    # This is merged based on the mask, such that we'll have either the expected
    # state value or 0 in case the state was final.
    non_final_next_states = non_final_next_states.squeeze(1)

    next_state_values = torch.zeros(BATCH_SIZE, device=device)
    with torch.no_grad():
        next_state_values[non_final_mask] = target_net(non_final_next_states).max(1).values
    # Compute the expected Q values
    # expected_state_action_values = (next_state_values * GAMMA) + reward_batch
    expected_state_action_values = ((next_state_values * GAMMA) + reward_batch).unsqueeze(1)

    # Compute Huber loss
    criterion = nn.SmoothL1Loss()
    loss = criterion(state_action_values, expected_state_action_values)

    # Optimize the model
    optimizer.zero_grad()
    loss.backward()
    # In-place gradient clipping
    torch.nn.utils.clip_grad_value_(policy_net.parameters(), 100)
    optimizer.step()


class ReplayMemory(object):

    def __init__(self, capacity):
        self.memory = deque([], maxlen=capacity)

    def push(self, *args):
        """Save a transition"""
        self.memory.append(Transition(*args))

    def sample(self, batch_size):
        return random.sample(self.memory, batch_size)

    def __len__(self):
        return len(self.memory)


class DQN(nn.Module):
    def __init__(self, num_observations, num_actions):
        super(DQN, self).__init__()
        self.layer1 = nn.Linear(num_observations, 64)
        self.layer2 = nn.Linear(64, 128)
        self.layer3 = nn.Linear(128, 64)
        self.layer4 = nn.Linear(64, num_actions)

    # Called with either one element to determine next action, or a batch
    # during optimization. Returns tensor([[left0exp,right0exp]...]).
    def forward(self, x):
        x = F.relu(self.layer1(x))
        x = F.relu(self.layer2(x))
        x = F.relu(self.layer3(x))
        return self.layer4(x)


def select_action(state_in):
    global steps_done
    sample = random.random()
    eps_threshold = EPS_END + (EPS_START - EPS_END) * math.exp(-1. * steps_done / EPS_DECAY)
    steps_done += 1
    if sample > eps_threshold:
        with torch.no_grad():
            # t.max(1) will return the largest column value of each row.
            # second column on max result is index of where max element was
            # found, so we pick action with the larger expected reward.
            action_out = policy_net(state_in).max(2).indices.view(1, 1)
            return action_out
    else:
        return torch.tensor(spec.action_spec.random_action(1).discrete, device=device, dtype=torch.long)


if __name__ == "__main__":
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    timer_start = time.perf_counter()

    Transition = namedtuple('Transition',
                            ('state', 'action', 'next_state', 'reward'))

    # BATCH_SIZE is the number of transitions sampled from the replay buffer
    # GAMMA is the discount factor as mentioned in the previous section
    # EPS_START is the starting value of epsilon
    # EPS_END is the final value of epsilon
    # EPS_DECAY controls the rate of exponential decay of epsilon, higher means a slower decay
    # TAU is the update rate of the target network
    # LR is the learning rate of the ``AdamW`` optimizer
    BATCH_SIZE = 128
    GAMMA = 0.99
    EPS_START = 0.9
    EPS_END = 0.05
    EPS_DECAY = 10_000
    TAU = 0.005
    LR = 1e-5

    SAVE_WEIGHTS = True
    LOAD_WEIGHTS = False
    steps_done = 0
    STEPS = 500

    env = UnityEnvironment(file_name="unity_builds/snake", seed=1, side_channels=[], no_graphics=True)
    env.reset()

    behaviour_name = list(env.behavior_specs)[0]
    spec = env.behavior_specs[behaviour_name]

    n_actions = spec.action_spec.discrete_branches[0]
    state, _ = env.get_steps(behaviour_name)
    state = state.obs[0]
    # n_observations = len(state)
    n_observations = spec.observation_specs[0].shape[0]

    if LOAD_WEIGHTS:
        policy_net = DQN(n_observations, n_actions).to(device)
        policy_net.load_state_dict(torch.load('weights/policy_net.pth'))
    else:
        policy_net = DQN(n_observations, n_actions).to(device)
    target_net = DQN(n_observations, n_actions).to(device)
    target_net.load_state_dict(policy_net.state_dict())

    optimizer = optim.AdamW(policy_net.parameters(), lr=LR, amsgrad=True)
    memory = ReplayMemory(1000)
    print(f"Initialized DQN with {n_observations} observations and {n_actions} actions")
    rewards = []

    if torch.cuda.is_available():
        num_episodes = 30
    else:
        num_episodes = 50

    pbar = tqdm(range(num_episodes))
    for i_episode in pbar:
        if i_episode % 100 == 0 and i_episode != 0:
            print(
                f"Episode {i_episode}, avg reward: {np.mean(rewards[-100:]):.2f}, "
                f"epsilon: {EPS_END + (EPS_START - EPS_END) * math.exp(-1. * steps_done / EPS_DECAY):.2f}")
        step_rewards = []
        # Initialize the environment and get its state
        env.reset()
        decision_steps, terminal_steps = env.get_steps(behaviour_name)
        state = decision_steps.obs[0]
        state = torch.tensor(state, dtype=torch.float32, device=device).unsqueeze(0)
        # for t in range(STEPS):
        t = 0
        # while True:
        for t in range(STEPS):
            # t += 1
            action = select_action(state)
            # action = action.unsqueeze(0)  # Ensure that action has shape [batch_size]
            action_tuple = ActionTuple()
            # eww.
            # action_array = action.cpu().numpy()
            # if len(action_array) > 1:
            #     action_for_env = action_array[0]
            # else:
            #     action_for_env = action_array
            # action_tuple.add_discrete(action_for_env.reshape(-1, 1))
            action_tuple.add_discrete(action.cpu().numpy())
            env.set_actions(behaviour_name, action_tuple)
            env.step()

            assert action.shape[0] == state.shape[0]

            decision_steps, terminal_steps = env.get_steps(behaviour_name)
            observation = decision_steps.obs[0]
            reward = np.zeros(state.shape[0])
            if len(decision_steps.reward) > 0:
                reward += decision_steps.reward
            if len(terminal_steps.reward) > 0:
                reward += terminal_steps.reward
            terminated = len(decision_steps) == 0

            reward = np.repeat(reward, state.shape[0])
            assert len(reward) == state.shape[0] == action.shape[0]

            # if t % 50 == 0:
            #     print(f"step: {t}, reward: {reward}, state: {state}, action: {action}, terminated: {terminated}")
            # observation, reward, terminated, truncated, _ = env.step(action.item())
            reward = torch.tensor(reward, device=device)
            step_rewards.append(reward.item())

            done = terminated  # or truncated

            if terminated:
                next_state = None
            else:
                next_state = torch.tensor(observation, dtype=torch.float32, device=device).unsqueeze(0)

            # Store the transition in memory
            memory.push(state, action, next_state, reward)

            # Move to the next state
            state = next_state

            # Perform one step of the optimization (on the policy network)
            optimize_model()

            # Soft update of the target network's weights
            # θ′ ← τ θ + (1 −τ )θ′
            target_net_state_dict = target_net.state_dict()
            policy_net_state_dict = policy_net.state_dict()
            for key in policy_net_state_dict:
                target_net_state_dict[key] = policy_net_state_dict[key] * TAU + target_net_state_dict[key] * (1 - TAU)
            target_net.load_state_dict(target_net_state_dict)

            if done:
                # episode_durations.append(t + 1)
                break

        ep_rewards = sum(step_rewards)
        pbar.set_description(f"E {i_episode} done after {t + 1} t, with r: {ep_rewards:.2f}")
        rewards.append(ep_rewards)

    if SAVE_WEIGHTS:
        torch.save(policy_net.state_dict(), 'weights/policy_net.pth')

    env.close()

    print(f"Finished training in {(time.perf_counter() - timer_start)/60 :.3} minutes")

    plt.plot(rewards)
    plt.xlabel('Episode')
    plt.ylabel('Total Reward')
    plt.title('Rewards over Episodes')
    plt.show()
