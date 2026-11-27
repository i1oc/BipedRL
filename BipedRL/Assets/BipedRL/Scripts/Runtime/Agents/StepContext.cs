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

        /// <summary>The velocity the robot is asked to move at.</summary>
        public VelocityTarget Target { get; private set; }

        /// <summary>The stepping rhythm the robot is asked to follow.</summary>
        public GaitClock Gait { get; }

        /// <summary>Actions just chosen by the policy, clamped to [-1, 1].</summary>
        public IReadOnlyList<float> Actions { get; }

        /// <summary>Actions chosen at the previous decision.</summary>
        public IReadOnlyList<float> PreviousActions { get; }

        public IReadOnlyList<float> DefaultJointAngles => robot.DefaultJointAngles;
        public IReadOnlyList<float> JointLowerLimits => robot.JointLowerLimits;
        public IReadOnlyList<float> JointUpperLimits => robot.JointUpperLimits;
        public float DefaultBaseHeight => robot.DefaultBaseHeight;

        public StepContext(RobotBody robot, GaitClock gait, IReadOnlyList<float> actions,
                           IReadOnlyList<float> previousActions)
        {
            this.robot = robot;
            Gait = gait;
            Actions = actions;
            PreviousActions = previousActions;
        }

        public void Update(RobotState state, VelocityTarget target)
        {
            State = state;
            Target = target;
        }
    }
}
