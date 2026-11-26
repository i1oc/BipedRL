using System.Collections.Generic;
using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Snapshot of what the robot can measure about itself: IMU, joint encoders and foot
    /// sensors. Vectors are in the pelvis frame and joint values follow the order of
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

        /// <summary>Angular velocity of the pelvis, in rad/s.</summary>
        public Vector3 BaseAngularVelocity { get; private set; }

        /// <summary>Unit gravity direction; (0, -1, 0) when the pelvis is upright.</summary>
        public Vector3 ProjectedGravity { get; private set; }

        /// <summary>Height of the pelvis above the ground point of the last reset, in metres.</summary>
        public float BaseHeight { get; private set; }

        /// <summary>Angle between the pelvis up axis and the world up axis, in radians.</summary>
        public float Tilt { get; private set; }

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

        /// <summary>Whether any body other than a foot touches the environment, which means a fall.</summary>
        public bool NonFootContact { get; private set; }

        internal RobotState(int jointCount, int footCount)
        {
            jointPositions = new float[jointCount];
            jointVelocities = new float[jointCount];
            jointTorques = new float[jointCount];
            jointTargets = new float[jointCount];
            footContacts = new bool[footCount];
        }

        internal void SetBase(Vector3 angularVelocity, Vector3 projectedGravity, float height, float tilt)
        {
            BaseAngularVelocity = angularVelocity;
            ProjectedGravity = projectedGravity;
            BaseHeight = height;
            Tilt = tilt;
        }

        internal void SetJoint(int joint, float position, float velocity, float torque, float target)
        {
            jointPositions[joint] = position;
            jointVelocities[joint] = velocity;
            jointTorques[joint] = torque;
            jointTargets[joint] = target;
        }

        internal void SetFootContact(int foot, bool touching)
        {
            footContacts[foot] = touching;
        }

        internal void SetNonFootContact(bool touching)
        {
            NonFootContact = touching;
        }
    }
}
