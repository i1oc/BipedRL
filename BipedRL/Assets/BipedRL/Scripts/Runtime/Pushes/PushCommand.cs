using BipedRL.Commands;
using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Pushes
{
    /// <summary>
    /// A push on a robot's pelvis. The same command is used by the random perturbator and by
    /// the player, and listeners of a push can read its impulse.
    /// </summary>
    public sealed class PushCommand : ICommand
    {
        private readonly RobotBody robot;

        /// <summary>Impulse in world coordinates, in N·s.</summary>
        public Vector3 Impulse { get; }

        public PushCommand(RobotBody robot, Vector3 impulse)
        {
            this.robot = robot;
            Impulse = impulse;
        }

        public void Execute()
        {
            robot.ApplyPush(Impulse);
        }
    }
}
