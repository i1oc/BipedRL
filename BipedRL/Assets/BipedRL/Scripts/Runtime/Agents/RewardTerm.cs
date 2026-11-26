using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Base class for reward terms stored as assets, so each term and its weight can be
    /// tuned in the inspector and shared between episode configurations.
    /// </summary>
    public abstract class RewardTerm : ScriptableObject, IRewardTerm
    {
        [SerializeField] private float weight;

        public float Weight => weight;

        public abstract float Evaluate(StepContext step);

        /// <summary>Used by editor tools that create the asset with a default weight.</summary>
        public void Configure(float newWeight)
        {
            weight = newWeight;
        }
    }
}
