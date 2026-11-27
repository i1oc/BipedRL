using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ranges of the random velocity targets used for training. The defaults are the command
    /// ranges of Unitree's H1 training (unitree_rl_gym).
    /// </summary>
    [CreateAssetMenu(fileName = "VelocityTargetRanges", menuName = "BipedRL/Velocity Target Ranges")]
    public sealed class VelocityTargetRanges : ScriptableObject
    {
        [Tooltip("Lowest and highest forward speed, in m/s.")]
        [SerializeField] private Vector2 forward = new Vector2(-1f, 1f);

        [Tooltip("Lowest and highest sideways speed, in m/s.")]
        [SerializeField] private Vector2 sideways = new Vector2(-1f, 1f);

        [Tooltip("Time between two new targets, in seconds.")]
        [Min(0.1f)]
        [SerializeField] private float resampleInterval = 10f;

        // Unitree sets small walking targets to zero, so standing still is also trained.
        [Tooltip("Walking targets slower than this become zero, in m/s.")]
        [Min(0f)]
        [SerializeField] private float minSpeed = 0.2f;

        [Tooltip("Turning speed per radian of heading error, in 1/s.")]
        [Min(0f)]
        [SerializeField] private float headingGain = 0.5f;

        [Tooltip("Highest turning speed, in rad/s.")]
        [Min(0f)]
        [SerializeField] private float maxYawRate = 1f;

        public Vector2 Forward => forward;
        public Vector2 Sideways => sideways;
        public float ResampleInterval => resampleInterval;
        public float MinSpeed => minSpeed;
        public float HeadingGain => headingGain;
        public float MaxYawRate => maxYawRate;
    }
}
