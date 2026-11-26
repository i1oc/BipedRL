using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Squared horizontal part of the gravity seen by the pelvis: 0 when upright, about sin² of
    /// the tilt angle otherwise.
    /// </summary>
    [CreateAssetMenu(fileName = "TiltTerm", menuName = "BipedRL/Reward Terms/Tilt")]
    public sealed class TiltTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            Vector3 gravity = step.State.ProjectedGravity;
            return gravity.x * gravity.x + gravity.z * gravity.z;
        }
    }
}
