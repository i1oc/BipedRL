using UnityEngine;

namespace BipedRL.Pushes
{
    /// <summary>
    /// How often and how hard the random perturbator pushes the robot.
    /// </summary>
    [CreateAssetMenu(fileName = "PushSettings", menuName = "BipedRL/Push Settings")]
    public sealed class PushSettings : ScriptableObject
    {
        [Tooltip("Shortest time between two pushes, in seconds.")]
        [Min(0.1f)]
        [SerializeField] private float minInterval = 3f;

        [Tooltip("Longest time between two pushes, in seconds.")]
        [Min(0.1f)]
        [SerializeField] private float maxInterval = 5f;

        // 25 N·s changes the speed of the 51 kg robot by about 0.5 m/s.
        [Tooltip("Push strength in N·s when no trainer sets the push_strength parameter, as in the game.")]
        [Min(0f)]
        [SerializeField] private float defaultStrength = 25f;

        public float MinInterval => minInterval;
        public float MaxInterval => maxInterval;
        public float DefaultStrength => defaultStrength;

        private void OnValidate()
        {
            maxInterval = Mathf.Max(maxInterval, minInterval);
        }
    }
}
