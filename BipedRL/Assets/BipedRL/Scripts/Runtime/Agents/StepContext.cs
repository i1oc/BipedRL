using System.Collections.Generic;
using BipedRL.Robot;

namespace BipedRL.Agents
{
    /// <summary>
    /// Read-only view of one agent decision, given to the reward terms and termination
    /// conditions. It never exposes the <see cref="RobotBody"/> itself, so a strategy can
    /// read the robot but never move it. The agent refills the same instance every decision.
    /// </summary>
    public sealed class StepContext
    {
        private readonly RobotBody robot;

        /// <summary>The robot's sensors at the time of the decision.</summary>
        public RobotState State { get; private set; }

        /// <summary>Actions just chosen by the policy, clamped to [-1, 1].</summary>
        public IReadOnlyList<float> Actions { get; }

        /// <summary>Actions chosen at the previous decision.</summary>
        public IReadOnlyList<float> PreviousActions { get; }

        public IReadOnlyList<float> DefaultJointAngles => robot.DefaultJointAngles;
        public IReadOnlyList<float> JointLowerLimits => robot.JointLowerLimits;
        public IReadOnlyList<float> JointUpperLimits => robot.JointUpperLimits;
        public float DefaultBaseHeight => robot.DefaultBaseHeight;

        public StepContext(RobotBody robot, IReadOnlyList<float> actions, IReadOnlyList<float> previousActions)
        {
            this.robot = robot;
            Actions = actions;
            PreviousActions = previousActions;
        }

        public void Update(RobotState state)
        {
            State = state;
        }
    }
}
