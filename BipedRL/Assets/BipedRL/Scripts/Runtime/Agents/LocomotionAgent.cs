using System;
using BipedRL.Robot;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// The policy of Unitree's H1 training (unitree_rl_gym): it walks at the velocity asked by
    /// an <see cref="IVelocityTargetSource"/> while stepping to a <see cref="GaitClock"/>. With a
    /// zero target the robot steps in place, which is how it stands and recovers from pushes.
    /// It observes only what the real robot can measure, plus the target and the clock, and
    /// moves the joints by offsetting their drive targets from the default pose. The robot is
    /// reached only through its <see cref="RobotBody"/> facade, and the reward and episode end
    /// come from the strategies listed in an <see cref="EpisodeConfig"/>.
    /// </summary>
    // No RequireComponent for the DecisionRequester: it requires an Agent itself, so adding
    // this agent would first add a plain Agent. The configuration check reports it instead.
    [RequireComponent(typeof(BehaviorParameters))]
    public sealed class LocomotionAgent : Agent
    {
        // Trained policies depend on these values: changing any of them makes every
        // trained model invalid, so they are constants and not tunable settings.
        // ML-Agents clips the network output to [-3, 3] and divides it by 3, so 0.75 here is
        // Unitree's 0.25 rad per unit of network output.
        private const float ActionScale = 0.75f;
        private const float LinearVelocityScale = 2f;
        private const float AngularVelocityScale = 0.25f;
        private const float JointVelocityScale = 0.05f;
        private const float PolicyPeriod = 0.02f;

        [SerializeField] private EpisodeConfig episodeConfig;

        private RobotBody robot;
        private IVelocityTargetSource targetSource;
        private readonly GaitClock gait = new GaitClock();
        private VelocityTarget target;
        private float[] actions;
        private float[] previousActions;
        private float[] jointTargets;
        private StepContext step;

        /// <summary>
        /// Size of the observation vector: base angular velocity and gravity (6), velocity
        /// target (3), joint positions, velocities and previous actions, and the clock (2).
        /// </summary>
        public static int ObservationSize(int jointCount)
        {
            return 9 + 3 * jointCount + 2;
        }

        /// <summary>Used by editor tools that create agent prefabs.</summary>
        public void Configure(EpisodeConfig config)
        {
            episodeConfig = config;
        }

        // Called from OnEnable, possibly before the robot's Awake, so it only uses the
        // robot's serialized configuration and never its state.
        public override void Initialize()
        {
            robot = GetComponentInChildren<RobotBody>();
            if (robot == null)
            {
                throw new InvalidOperationException($"{name} needs a RobotBody among its children.");
            }

            targetSource = GetComponent<IVelocityTargetSource>();
            if (targetSource == null)
            {
                throw new InvalidOperationException($"{name} needs a velocity target component, such as RandomVelocityTargets.");
            }

            actions = new float[robot.JointCount];
            previousActions = new float[robot.JointCount];
            jointTargets = new float[robot.JointCount];
            step = new StepContext(robot, gait, actions, previousActions);
            CheckConfiguration();
        }

        public override void OnEpisodeBegin()
        {
            // The agent root marks the spawn point on the ground; the robot moves, the root does not.
            robot.ResetPose(transform.position, transform.rotation);
            Array.Clear(actions, 0, actions.Length);
            Array.Clear(previousActions, 0, previousActions.Length);
            targetSource.Restart();
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            RobotState state = robot.ReadState();
            float episodeTime = StepCount * Time.fixedDeltaTime;
            gait.Update(episodeTime);
            target = targetSource.GetTarget(state, episodeTime);

            sensor.AddObservation(state.BaseAngularVelocity * AngularVelocityScale);
            sensor.AddObservation(state.ProjectedGravity);
            sensor.AddObservation(target.Forward * LinearVelocityScale);
            sensor.AddObservation(target.Sideways * LinearVelocityScale);
            sensor.AddObservation(target.YawRate * AngularVelocityScale);

            for (int joint = 0; joint < robot.JointCount; joint++)
            {
                sensor.AddObservation(state.JointPositions[joint] - robot.DefaultJointAngles[joint]);
            }

            for (int joint = 0; joint < robot.JointCount; joint++)
            {
                sensor.AddObservation(state.JointVelocities[joint] * JointVelocityScale);
            }

            // The new action is not chosen yet, so these are the ones from the last decision.
            sensor.AddObservation(actions);

            float angle = 2f * Mathf.PI * gait.Phase;
            sensor.AddObservation(Mathf.Sin(angle));
            sensor.AddObservation(Mathf.Cos(angle));
        }

        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            ActionSegment<float> continuousActions = actionBuffers.ContinuousActions;
            for (int joint = 0; joint < robot.JointCount; joint++)
            {
                previousActions[joint] = actions[joint];
                actions[joint] = Mathf.Clamp(continuousActions[joint], -1f, 1f);
                jointTargets[joint] = robot.DefaultJointAngles[joint] + ActionScale * actions[joint];
            }

            robot.SetJointTargets(jointTargets);

            step.Update(robot.ReadState(), target);
            AddReward(episodeConfig.ComputeReward(step));
            if (episodeConfig.IsEpisodeOver(step))
            {
                EndEpisode();
            }
        }

        /// <summary>Zero actions: every joint holds its default angle.</summary>
        public override void Heuristic(in ActionBuffers actionsOut)
        {
            actionsOut.ContinuousActions.Clear();
        }

        // These values are set in the inspector, so a wrong one is reported here instead of
        // silently training a policy with the wrong inputs or rewards.
        private void CheckConfiguration()
        {
            if (episodeConfig == null)
            {
                Debug.LogError($"{name}: assign an Episode Config to the Locomotion Agent.", this);
            }

            int expectedObservations = ObservationSize(robot.JointCount);
            BrainParameters brain = GetComponent<BehaviorParameters>().BrainParameters;
            if (brain.VectorObservationSize != expectedObservations ||
                brain.ActionSpec.NumContinuousActions != robot.JointCount ||
                brain.ActionSpec.NumDiscreteActions != 0)
            {
                Debug.LogError($"{name}: Behavior Parameters need {expectedObservations} vector observations " +
                               $"and {robot.JointCount} continuous actions.", this);
            }

            var requester = GetComponent<DecisionRequester>();
            if (requester == null)
            {
                Debug.LogError($"{name}: add a Decision Requester so the agent asks for decisions.", this);
                return;
            }

            if (!Mathf.Approximately(requester.DecisionPeriod * Time.fixedDeltaTime, PolicyPeriod))
            {
                Debug.LogError($"{name}: the policy must decide every {PolicyPeriod} s, but Decision Period " +
                               $"{requester.DecisionPeriod} x Fixed Timestep {Time.fixedDeltaTime} gives a different period.", this);
            }

            // Repeating the action between decisions would evaluate the rewards twice per
            // decision; the drives already hold their targets on their own.
            if (requester.TakeActionsBetweenDecisions)
            {
                Debug.LogError($"{name}: untick Take Actions Between Decisions in the Decision Requester.", this);
            }
        }
    }
}
