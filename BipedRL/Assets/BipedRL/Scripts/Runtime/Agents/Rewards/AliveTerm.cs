using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Constant 1 for every decision the robot has not fallen, so surviving longer pays more.
    /// </summary>
    [CreateAssetMenu(fileName = "AliveTerm", menuName = "BipedRL/Reward Terms/Alive")]
    public sealed class AliveTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            return 1f;
        }
    }
}
