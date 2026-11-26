using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ends the episode when the pelvis drops below a minimum height.
    /// </summary>
    [CreateAssetMenu(fileName = "LowBaseCondition", menuName = "BipedRL/Termination Conditions/Low Base")]
    public sealed class LowBaseCondition : TerminationCondition
    {
        [Tooltip("Minimum pelvis height above the ground, in metres.")]
        [SerializeField] private float minHeight = 0.6f;

        public override bool IsMet(StepContext step)
        {
            return step.State.BaseHeight < minHeight;
        }
    }
}
