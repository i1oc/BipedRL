using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Drive gains and default pose of a robot's actuated joints. The robot importer uses
    /// it to configure the joint drives, and the robot uses it at runtime to reset to the
    /// default pose.
    /// </summary>
    [CreateAssetMenu(fileName = "RobotJointSettings", menuName = "BipedRL/Robot Joint Settings")]
    public sealed class RobotJointSettings : ScriptableObject
    {
        [Tooltip("Height of the pelvis above the ground in the default pose, in metres.")]
        [SerializeField] private float defaultPelvisHeight;

        [SerializeField] private List<JointSettings> joints = new List<JointSettings>();

        public float DefaultPelvisHeight => defaultPelvisHeight;
        public IReadOnlyList<JointSettings> Joints => joints;

        public JointSettings GetJoint(string jointName)
        {
            JointSettings settings = joints.FirstOrDefault(joint => joint.JointName == jointName);
            if (settings == null)
            {
                throw new KeyNotFoundException($"{name} has no settings for joint '{jointName}'.");
            }

            return settings;
        }

        /// <summary>
        /// Replaces every value. Used by editor tools that create the asset with default values.
        /// </summary>
        public void Configure(float pelvisHeight, IEnumerable<JointSettings> jointSettings)
        {
            defaultPelvisHeight = pelvisHeight;
            joints = jointSettings.ToList();
        }
    }
}
