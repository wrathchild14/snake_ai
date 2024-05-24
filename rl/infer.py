import random
import torch

from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple

import numpy as np

from train import DQN, DuelingDQN


if __name__ == "__main__":
    device = torch.device("cuda" if torch.cuda.is_available() else "cpu")

    env = UnityEnvironment(file_name="unity_builds/snake", seed=1, side_channels=[], no_graphics=False)
    env.reset()

    behaviour_name = list(env.behavior_specs)[0]
    spec = env.behavior_specs[behaviour_name]

    n_actions = spec.action_spec.discrete_branches[0]
    state, _ = env.get_steps(behaviour_name)
    state = state.obs[0]
    # n_observations = len(state)
    n_observations = spec.observation_specs[0].shape[0]

    # policy_net = DQN(n_observations, n_actions).to(device)
    policy_net = DuelingDQN(n_observations, n_actions).to(device)
    # policy_net.load_state_dict(torch.load('weights/large_observations/4k/policy_net.pth'))
    policy_net.load_state_dict(torch.load('weights/policy_net.pth'))
    for t in range(5):
        env.reset()
        decision_steps, terminal_steps = env.get_steps(behaviour_name)
        state = decision_steps.obs[0]
        state = torch.tensor(state, dtype=torch.float32, device=device).unsqueeze(0)
        while True:
            with torch.no_grad():
                action = policy_net(state).max(2).indices.view(1, 1)
        
            action_tuple = ActionTuple()
            action_tuple.add_discrete(action.cpu().numpy())
            env.set_actions(behaviour_name, action_tuple)
            env.step()
            
            assert action.shape[0] == state.shape[0]
            
            decision_steps, terminal_steps = env.get_steps(behaviour_name)
            observation = decision_steps.obs[0]
            reward = np.zeros(state.shape[0])
            # print(decision_steps.reward, terminal_steps.reward)
            if len(decision_steps.reward) > 0:
                reward += decision_steps.reward
            if len(terminal_steps.reward) > 0:
                reward += terminal_steps.reward
            done = len(decision_steps) == 0
            terminated = len(terminal_steps) > 0
            reward = np.repeat(reward, state.shape[0])
            assert len(reward) == state.shape[0] == action.shape[0]
            if done or terminated:
                break
            state = torch.tensor(observation, dtype=torch.float32, device=device).unsqueeze(0)
    env.close()
