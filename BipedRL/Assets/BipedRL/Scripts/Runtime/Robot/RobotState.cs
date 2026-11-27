using System.Collections.Generic;
using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Snapshot of the robot: what its IMU, joint encoders and foot sensors measure, plus the
    /// base velocity, feet motion and body contacts that only the training rewards use.
    /// Base vectors are in the pelvis frame and joint values follow the order of
    /// <see cref="RobotBody.JointNames"/>. <see cref="RobotBody"/> refills the same instance
    /// on every read, so no memory is allocated during training.
    /// </summary>
    public sealed class RobotState
    {
        private readonly float[] jointPositions;
        private readonly float[] jointVelocities;
        private readonly float[] jointTorques;
        private readonly float[] jointTargets;
        private readonly bool[] footContacts;
        private readonly float[] footHeights;
        private readonly Vector3[] footVelocities;

        /// <summary>Linear velocity of the pelvis in its own frame, in m/s.</summary>
        public Vector3 BaseLinearVelocity { get; private set; }

        /// <summary>Angular velocity of the pelvis, in rad/s.</summary>
        public Vector3 BaseAngularVelocity { get; private set; }

        /// <summary>Unit gravity direction; (0, -1, 0) when the pelvis is upright.</summary>
        public Vector3 ProjectedGravity { get; private set; }

        /// <summary>Height of the pelvis above the ground point of the last reset, in metres.</summary>
        public float BaseHeight { get; private set; }

        /// <summary>
        /// Direction the pelvis faces on the ground, in radians from the world forward axis;
        /// it grows when the robot turns right.
        /// </summary>
        public float Heading { get; private set; }

        /// <summary>Sideways lean of the pelvis, in radians.</summary>
        public float Roll { get; private set; }

        /// <summary>Forward or backward lean of the pelvis, in radians.</summary>
        public float Pitch { get; private set; }

        /// <summary>Joint angles, in radians.</summary>
        public IReadOnlyList<float> JointPositions => jointPositions;

        /// <summary>Joint angular velocities, in rad/s.</summary>
        public IReadOnlyList<float> JointVelocities => jointVelocities;

        /// <summary>Drive torques, in N·m.</summary>
        public IReadOnlyList<float> JointTorques => jointTorques;

        /// <summary>Drive targets, in radians.</summary>
        public IReadOnlyList<float> JointTargets => jointTargets;

        /// <summary>Whether each foot touches the environment, in the order of the robot's feet.</summary>
        public IReadOnlyList<bool> FootContacts => footContacts;

        /// <summary>Height of each foot's ankle joint above the ground point of the last reset, in metres.</summary>
        public IReadOnlyList<float> FootHeights => footHeights;

        /// <summary>Velocity of each foot in world coordinates, in m/s.</summary>
        public IReadOnlyList<Vector3> FootVelocities => footVelocities;

        /// <summary>Whether the pelvis touches the environment, which means a fall.</summary>
        public bool PelvisContact { get; private set; }

        /// <summary>How many hip and knee bodies touch the environment.</summary>
        public int LegContactCount { get; private set; }

        internal RobotState(int jointCount, int footCount)
        {
            jointPositions = new float[jointCount];
            jointVelocities = new float[jointCount];
            jointTorques = new float[jointCount];
            jointTargets = new float[jointCount];
            footContacts = new bool[footCount];
            footHeights = new float[footCount];
            footVelocities = new Vector3[footCount];
        }

        internal void SetBase(Vector3 linearVelocity, Vector3 angularVelocity, Vector3 projectedGravity,
                              float height, float heading)
        {
            BaseLinearVelocity = linearVelocity;
            BaseAngularVelocity = angularVelocity;
            ProjectedGravity = projectedGravity;
            BaseHeight = height;
            Heading = heading;

            // Unitree's roll and pitch (Z-Y-X Euler angles) from the gravity in the pelvis
            // frame, where x is right, y is up and z is forward.
            Pitch = Mathf.Asin(Mathf.Clamp(projectedGravity.z, -1f, 1f));
            Roll = Mathf.Atan2(projectedGravity.x, -projectedGravity.y);
        }

        internal void SetJoint(int joint, float position, float velocity, float torque, float target)
        {
            jointPositions[joint] = position;
            jointVelocities[joint] = velocity;
            jointTorques[joint] = torque;
            jointTargets[joint] = target;
        }

        internal void SetFoot(int foot, bool touching, float height, Vector3 velocity)
        {
            footContacts[foot] = touching;
            footHeights[foot] = height;
            footVelocities[foot] = velocity;
        }

        internal void SetBodyContacts(bool pelvis, int legBodies)
        {
            PelvisContact = pelvis;
            LegContactCount = legBodies;
        }
    }
}
