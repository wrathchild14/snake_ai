import numpy as np
import torch
from torch import nn
from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.base_env import ActionTuple

class DQN(nn.Module):
    def __init__(self, obs_size, n_actions):
        super(DQN, self).__init__()
        self.net = nn.Sequential(
            nn.Linear(obs_size, 64),
            nn.ReLU(),
            nn.Linear(64, n_actions)
        )

    def forward(self, x):
        return self.net(x.float())

env = UnityEnvironment(file_name="unity_builds/snake", seed=1, side_channels=[])
env.reset()

behaviour_name = list(env.behavior_specs)[0]
spec = env.behavior_specs[behaviour_name]

obs_size = spec.observation_specs[0].shape[0]
n_actions = spec.action_spec.discrete_branches[0]

net = DQN(obs_size, n_actions)
optimizer = torch.optim.Adam(net.parameters(), lr=0.01)

epsilon = 0.2
num_episodes = 5  

for episode in range(num_episodes):
    env.reset()
    total_reward = 0

    for i in range(100):
        decision_steps, terminal_steps = env.get_steps(behaviour_name)

        if len(decision_steps) == 0:
            break

        state_v = torch.tensor(np.array(decision_steps.obs[0]))
        q_vals_v = net(state_v)

        if np.random.random() < epsilon:
            action = np.random.choice(n_actions, size=(len(decision_steps), 1))
        else:
            _, action_v = torch.max(q_vals_v, dim=1)
            action = action_v.data.numpy().reshape(-1, 1)
        action_tuple = ActionTuple()
        action_tuple.add_discrete(action)

        env.set_actions(behaviour_name, action_tuple)
        env.step()

        for agent_id in decision_steps:
            reward = decision_steps[agent_id].reward
            total_reward += reward

        if decision_steps.agent_id[0] in terminal_steps:
            print(f"Episode {episode + 1}: total reward {total_reward}")
            break
print(f"Game over, total reward {total_reward}")
env.close()