using System.Collections.Generic;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Sum of squared joint velocities, in rad²/s².
    /// </summary>
    [CreateAssetMenu(fileName = "JointVelocityTerm", menuName = "BipedRL/Reward Terms/Joint Velocity")]
    public sealed class JointVelocityTerm : RewardTerm
    {
        public override float Evaluate(StepContext step)
        {
            float sum = 0f;
            IReadOnlyList<float> values = step.State.JointVelocities;
            for (int joint = 0; joint < values.Count; joint++)
            {
                sum += values[joint] * values[joint];
            }

            return sum;
        }
    }
}
