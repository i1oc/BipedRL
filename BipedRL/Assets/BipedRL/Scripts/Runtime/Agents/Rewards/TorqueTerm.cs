using System.Collections.Generic;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Sum of squared drive torques, in N²·m², which favours energy-efficient poses.
    /// </summary>
    [CreateAssetMenu(fileName = "TorqueTerm", menuName = "BipedRL/Reward Terms/Torque")]
    public sealed class TorqueTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            float sum = 0f;
            IReadOnlyList<float> values = step.State.JointTorques;
            for (int joint = 0; joint < values.Count; joint++)
            {
                sum += values[joint] * values[joint];
            }

            return sum;
        }
    }
}
