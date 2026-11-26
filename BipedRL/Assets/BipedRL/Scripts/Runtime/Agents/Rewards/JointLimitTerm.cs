using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// How far, in radians, the joints go outside a central part of their range. Staying away
    /// from the hard limits avoids hitting them, as the real motors must.
    /// </summary>
    [CreateAssetMenu(fileName = "JointLimitTerm", menuName = "BipedRL/Reward Terms/Joint Limits")]
    public sealed class JointLimitTerm : RewardTerm
    {
        [Tooltip("Fraction of each joint range, centred on its middle, that is free of penalty.")]
        [Range(0f, 1f)]
        [SerializeField] private float softRangeFraction = 0.9f;

        public override float Evaluate(StepContext step)
        {
            float excess = 0f;
            for (int joint = 0; joint < step.DefaultJointAngles.Count; joint++)
            {
                float lower = step.JointLowerLimits[joint];
                float upper = step.JointUpperLimits[joint];
                float middle = (lower + upper) / 2f;
                float softHalfRange = (upper - lower) / 2f * softRangeFraction;
                float angle = step.State.JointPositions[joint];

                excess += Mathf.Max(0f, middle - softHalfRange - angle) + Mathf.Max(0f, angle - middle - softHalfRange);
            }

            return excess;
        }
    }
}
