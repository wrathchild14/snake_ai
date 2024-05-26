from mlagents_envs.environment import UnityEnvironment

env = UnityEnvironment(file_name="unity_builds/snake", seed=1, side_channels=[])
env.reset()

behaviour_name = list(env.behavior_specs)[0]
spec = env.behavior_specs[behaviour_name]

done = False

while not done:
    decision_steps, terminal_steps = env.get_steps(behaviour_name)
    print("Decision step rewards:", decision_steps.reward)
    print("Terminal step rewards:", terminal_steps.reward)

    print(decision_steps.obs[0])

    if len(decision_steps) == 0:
        break

    tracked_agent = decision_steps.agent_id[0]
    action = spec.action_spec.random_action(len(decision_steps))
    print(action.discrete)
    env.set_actions(behaviour_name, action)
    env.step()

    if tracked_agent in terminal_steps:
        done = True

print("Game over")
env.close()