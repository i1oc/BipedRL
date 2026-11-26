using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// The rules of an episode: which reward terms are added and which conditions end it.
    /// Each level or training stage can use its own configuration without code changes.
    /// </summary>
    [CreateAssetMenu(fileName = "EpisodeConfig", menuName = "BipedRL/Episode Config")]
    public sealed class EpisodeConfig : ScriptableObject
    {
        [SerializeField] private List<RewardTerm> rewardTerms = new List<RewardTerm>();
        [SerializeField] private List<TerminationCondition> terminationConditions = new List<TerminationCondition>();

        public IReadOnlyList<IRewardTerm> RewardTerms => rewardTerms;
        public IReadOnlyList<ITerminationCondition> TerminationConditions => terminationConditions;

        // Indexed loops: a foreach over the interface lists would allocate an enumerator on
        // every decision of every robot during training.

        /// <summary>Weighted sum of every reward term.</summary>
        public float ComputeReward(StepContext step)
        {
            float reward = 0f;
            for (int i = 0; i < rewardTerms.Count; i++)
            {
                IRewardTerm term = rewardTerms[i];
                reward += term.Weight * term.Evaluate(step);
            }

            return reward;
        }

        public bool IsEpisodeOver(StepContext step)
        {
            for (int i = 0; i < terminationConditions.Count; i++)
            {
                ITerminationCondition condition = terminationConditions[i];
                if (condition.IsMet(step))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Replaces every value. Used by editor tools that create the asset with default values.
        /// </summary>
        public void Configure(IEnumerable<RewardTerm> terms, IEnumerable<TerminationCondition> conditions)
        {
            rewardTerms = terms.ToList();
            terminationConditions = conditions.ToList();
        }
    }
}
