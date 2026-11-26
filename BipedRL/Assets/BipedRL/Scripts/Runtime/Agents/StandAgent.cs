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
    /// Milestone 1 agent: keeps the robot standing. It observes only what the real robot
    /// can measure and moves the joints by offsetting their drive targets from the default
    /// pose. The robot is reached only through its <see cref="RobotBody"/> facade, and the
    /// reward and episode end come from the strategies listed in an <see cref="EpisodeConfig"/>.
    /// </summary>
    [RequireComponent(typeof(BehaviorParameters), typeof(DecisionRequester))]
    public sealed class StandAgent : Agent
    {
        // Trained policies depend on these values: changing any of them makes every
        // trained model invalid, so they are constants and not tunable settings.
        private const float ActionScale = 0.25f;
        private const float AngularVelocityScale = 0.25f;
        private const float JointVelocityScale = 0.05f;
        private const float PolicyPeriod = 0.02f;

        [SerializeField] private EpisodeConfig episodeConfig;

        private RobotBody robot;
        private float[] actions;
        private float[] previousActions;
        private float[] jointTargets;
        private StepContext step;

        // Called from OnEnable, possibly before the robot's Awake, so it only uses the
        // robot's serialized configuration and never its state.
        public override void Initialize()
        {
            robot = GetComponentInChildren<RobotBody>();
            if (robot == null)
            {
                throw new InvalidOperationException($"{name} needs a RobotBody among its children.");
            }

            actions = new float[robot.JointCount];
            previousActions = new float[robot.JointCount];
            jointTargets = new float[robot.JointCount];
            step = new StepContext(robot, actions, previousActions);
            CheckConfiguration();
        }

        public override void OnEpisodeBegin()
        {
            // The agent root marks the spawn point on the ground; the robot moves, the root does not.
            robot.ResetPose(transform.position, transform.rotation);
            Array.Clear(actions, 0, actions.Length);
            Array.Clear(previousActions, 0, previousActions.Length);
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            RobotState state = robot.ReadState();

            sensor.AddObservation(state.BaseAngularVelocity * AngularVelocityScale);
            sensor.AddObservation(state.ProjectedGravity);

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

            for (int foot = 0; foot < state.FootContacts.Count; foot++)
            {
                sensor.AddObservation(state.FootContacts[foot]);
            }
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

            step.Update(robot.ReadState());
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
                Debug.LogError($"{name}: assign an Episode Config to the Stand Agent.", this);
            }

            int expectedObservations = 6 + 3 * robot.JointCount + robot.FootCount;
            BrainParameters brain = GetComponent<BehaviorParameters>().BrainParameters;
            if (brain.VectorObservationSize != expectedObservations ||
                brain.ActionSpec.NumContinuousActions != robot.JointCount ||
                brain.ActionSpec.NumDiscreteActions != 0)
            {
                Debug.LogError($"{name}: Behavior Parameters need {expectedObservations} vector observations " +
                               $"and {robot.JointCount} continuous actions.", this);
            }

            var requester = GetComponent<DecisionRequester>();
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
