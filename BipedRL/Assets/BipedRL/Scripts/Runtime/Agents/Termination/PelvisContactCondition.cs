using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Ends the episode when the pelvis touches the environment. As in Unitree's training,
    /// hips and knees touching the ground are penalised by a reward term instead.
    /// </summary>
    [CreateAssetMenu(fileName = "PelvisContactCondition", menuName = "BipedRL/Termination Conditions/Pelvis Contact")]
    public sealed class PelvisContactCondition : TerminationCondition
    {
        public override bool IsMet(StepContext step)
        {
            return step.State.PelvisContact;
        }
    }
}
