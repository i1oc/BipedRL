using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ends the episode when the pelvis leans too far sideways (roll) or forwards or
    /// backwards (pitch). The default limits are Unitree's.
    /// </summary>
    [CreateAssetMenu(fileName = "LeanLimitCondition", menuName = "BipedRL/Termination Conditions/Lean Limit")]
    public sealed class LeanLimitCondition : TerminationCondition
    {
        [Tooltip("Largest sideways lean, in radians.")]
        [Range(0f, Mathf.PI)]
        [SerializeField] private float maxRoll = 0.8f;

        [Tooltip("Largest forward or backward lean, in radians.")]
        [Range(0f, Mathf.PI / 2f)]
        [SerializeField] private float maxPitch = 1f;

        public override bool IsMet(StepContext step)
        {
            return Mathf.Abs(step.State.Roll) > maxRoll || Mathf.Abs(step.State.Pitch) > maxPitch;
        }
    }
}
