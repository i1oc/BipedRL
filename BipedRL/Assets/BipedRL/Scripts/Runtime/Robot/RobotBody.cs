using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Facade pattern: the only entry point to the robot for agents, rewards and levels.
    /// It hides the ArticulationBody details: degrees versus radians, the DOF index of each
    /// joint in the articulation's reduced coordinates, and how a reset must be done.
    /// Joint values always use radians and the order of <see cref="JointNames"/>.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class RobotBody : MonoBehaviour
    {
        [SerializeField] private RobotJointSettings jointSettings;
        [SerializeField] private ArticulationBody rootBody;

        [Tooltip("Actuated joints, in the same order as the joint settings.")]
        [SerializeField] private ArticulationBody[] joints = Array.Empty<ArticulationBody>();

        [SerializeField] private FootContact[] feet = Array.Empty<FootContact>();

        // Reduced-coordinate buffers covering the whole articulation, reused on every call
        // so nothing is allocated during training.
        private readonly List<float> positionBuffer = new List<float>();
        private readonly List<float> velocityBuffer = new List<float>();
        private readonly List<float> torqueBuffer = new List<float>();
        private readonly List<float> targetBuffer = new List<float>();
        private readonly List<int> dofStartIndices = new List<int>();

        private int[] jointDofIndices;
        private string[] jointNames;
        private float[] defaultAngles;
        private float[] lowerLimits;
        private float[] upperLimits;
        private RobotState state;
        private float groundHeight;

        public int JointCount => joints.Length;
        public int FootCount => feet.Length;
        public IReadOnlyList<string> JointNames => jointNames;

        /// <summary>Angles of the default standing pose, in radians.</summary>
        public IReadOnlyList<float> DefaultJointAngles => defaultAngles;

        /// <summary>Joint limits, in radians.</summary>
        public IReadOnlyList<float> JointLowerLimits => lowerLimits;

        /// <summary>Joint limits, in radians.</summary>
        public IReadOnlyList<float> JointUpperLimits => upperLimits;

        // The articulation only gets its DOF layout once it is in the physics scene, so the
        // indices are looked up on first use instead of in Awake.
        private int[] JointDofIndices => jointDofIndices ??= FindJointDofIndices();

        /// <summary>
        /// Sets the references to the robot's parts. Used by the robot importer when it
        /// builds the prefab.
        /// </summary>
        public void Configure(RobotJointSettings settings, ArticulationBody root,
                              IEnumerable<ArticulationBody> jointBodies, IEnumerable<FootContact> footContacts)
        {
            jointSettings = settings;
            rootBody = root;
            joints = jointBodies.ToArray();
            feet = footContacts.ToArray();
        }

        private void Awake()
        {
            IReadOnlyList<JointSettings> settings = jointSettings.Joints;
            if (settings.Count != joints.Length)
            {
                throw new InvalidOperationException(
                    $"{name} has {joints.Length} joints but {jointSettings.name} has {settings.Count}. Rebuild the robot prefab.");
            }

            jointNames = settings.Select(joint => joint.JointName).ToArray();
            defaultAngles = settings.Select(joint => joint.DefaultAngle).ToArray();
            lowerLimits = joints.Select(joint => joint.xDrive.lowerLimit * Mathf.Deg2Rad).ToArray();
            upperLimits = joints.Select(joint => joint.xDrive.upperLimit * Mathf.Deg2Rad).ToArray();
            state = new RobotState(joints.Length, feet.Length);

            // The prefab root stays on the ground; only the articulation moves.
            groundHeight = transform.position.y;
        }

        /// <summary>Sets the drive target of every joint, in radians.</summary>
        public void SetJointTargets(IReadOnlyList<float> targets)
        {
            if (targets.Count != joints.Length)
            {
                throw new ArgumentException($"Expected {joints.Length} joint targets, got {targets.Count}.", nameof(targets));
            }

            rootBody.GetDriveTargets(targetBuffer);
            WriteJointValues(targets, targetBuffer);
            rootBody.SetDriveTargets(targetBuffer);
        }

        /// <summary>
        /// Puts the robot at rest in its default pose, standing on <paramref name="groundPosition"/>.
        /// </summary>
        public void ResetPose(Vector3 groundPosition, Quaternion rotation)
        {
            groundHeight = groundPosition.y;

            // Writing the transform has no effect on an articulation: the root must be teleported.
            rootBody.TeleportRoot(groundPosition + Vector3.up * jointSettings.DefaultPelvisHeight, rotation);
            rootBody.linearVelocity = Vector3.zero;
            rootBody.angularVelocity = Vector3.zero;

            rootBody.GetJointPositions(positionBuffer);
            WriteJointValues(defaultAngles, positionBuffer);
            rootBody.SetJointPositions(positionBuffer);

            rootBody.GetJointVelocities(velocityBuffer);
            for (int i = 0; i < velocityBuffer.Count; i++)
            {
                velocityBuffer[i] = 0f;
            }

            rootBody.SetJointVelocities(velocityBuffer);

            SetJointTargets(defaultAngles);
        }

        /// <summary>
        /// Reads the robot's sensors. The returned instance is refilled on the next call.
        /// </summary>
        public RobotState ReadState()
        {
            Transform pelvis = rootBody.transform;
            state.SetBase(
                pelvis.InverseTransformDirection(rootBody.angularVelocity),
                pelvis.InverseTransformDirection(Vector3.down),
                pelvis.position.y - groundHeight,
                Vector3.Angle(pelvis.up, Vector3.up) * Mathf.Deg2Rad);

            rootBody.GetJointPositions(positionBuffer);
            rootBody.GetJointVelocities(velocityBuffer);
            rootBody.GetDriveForces(torqueBuffer);
            rootBody.GetDriveTargets(targetBuffer);

            int[] dofIndices = JointDofIndices;
            for (int joint = 0; joint < joints.Length; joint++)
            {
                int dof = dofIndices[joint];
                state.SetJoint(joint, positionBuffer[dof], velocityBuffer[dof], torqueBuffer[dof], targetBuffer[dof]);
            }

            for (int foot = 0; foot < feet.Length; foot++)
            {
                state.SetFootContact(foot, feet[foot].IsTouching);
            }

            return state;
        }

        private void WriteJointValues(IReadOnlyList<float> values, List<float> reducedValues)
        {
            int[] dofIndices = JointDofIndices;
            for (int joint = 0; joint < joints.Length; joint++)
            {
                reducedValues[dofIndices[joint]] = values[joint];
            }
        }

        // Each body's first DOF in the reduced-coordinate lists. Hinges have a single DOF.
        private int[] FindJointDofIndices()
        {
            rootBody.GetDofStartIndices(dofStartIndices);
            return joints.Select(joint => dofStartIndices[joint.index]).ToArray();
        }
    }
}
