using System;
using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// PD gains and default angle of one actuated joint, in SI units.
    /// </summary>
    [Serializable]
    public sealed class JointSettings
    {
        [SerializeField] private string jointName;

        [Tooltip("Proportional gain of the joint drive, in N·m/rad.")]
        [SerializeField] private float stiffness;

        [Tooltip("Derivative gain of the joint drive, in N·m·s/rad.")]
        [SerializeField] private float damping;

        [Tooltip("Angle of the default standing pose, in radians.")]
        [SerializeField] private float defaultAngle;

        public string JointName => jointName;
        public float Stiffness => stiffness;
        public float Damping => damping;
        public float DefaultAngle => defaultAngle;

        // Used by Unity's serializer and by the inspector when adding list entries.
        private JointSettings()
        {
        }

        public JointSettings(string jointName, float stiffness, float damping, float defaultAngle)
        {
            this.jointName = jointName;
            this.stiffness = stiffness;
            this.damping = damping;
            this.defaultAngle = defaultAngle;
        }
    }
}
