using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Sum of squared changes between consecutive actions, which discourages jerky motion.
    /// </summary>
    [CreateAssetMenu(fileName = "ActionRateTerm", menuName = "BipedRL/Reward Terms/Action Rate")]
    public sealed class ActionRateTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            float sum = 0f;
            for (int joint = 0; joint < step.Actions.Count; joint++)
            {
                float change = step.Actions[joint] - step.PreviousActions[joint];
                sum += change * change;
            }

            return sum;
        }
    }
}
