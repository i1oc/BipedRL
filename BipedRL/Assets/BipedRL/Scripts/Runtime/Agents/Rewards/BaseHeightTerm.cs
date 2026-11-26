using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Squared distance between the pelvis height and its height in the default pose, in m².
    /// </summary>
    [CreateAssetMenu(fileName = "BaseHeightTerm", menuName = "BipedRL/Reward Terms/Base Height")]
    public sealed class BaseHeightTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            float error = step.State.BaseHeight - step.DefaultBaseHeight;
            return error * error;
        }
    }
}
