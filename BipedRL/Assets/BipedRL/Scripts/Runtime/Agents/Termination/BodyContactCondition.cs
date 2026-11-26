using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ends the episode when any body other than a foot touches the environment.
    /// </summary>
    [CreateAssetMenu(fileName = "BodyContactCondition", menuName = "BipedRL/Termination Conditions/Body Contact")]
    public sealed class BodyContactCondition : TerminationCondition
    {
        public override bool IsMet(StepContext step)
        {
            return step.State.NonFootContact;
        }
    }
}
