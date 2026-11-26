using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ends the episode when the pelvis tilts more than a maximum angle.
    /// </summary>
    [CreateAssetMenu(fileName = "TiltLimitCondition", menuName = "BipedRL/Termination Conditions/Tilt Limit")]
    public sealed class TiltLimitCondition : TerminationCondition
    {
        [Tooltip("Maximum tilt of the pelvis from upright, in degrees.")]
        [Range(0f, 180f)]
        [SerializeField] private float maxTiltDegrees = 45f;

        public override bool IsMet(StepContext step)
        {
            return step.State.Tilt > maxTiltDegrees * Mathf.Deg2Rad;
        }
    }
}
