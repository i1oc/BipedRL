using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Base class for termination conditions stored as assets, so their thresholds can be
    /// tuned in the inspector.
    /// </summary>
    public abstract class TerminationCondition : ScriptableObject, ITerminationCondition
    {
        public abstract bool IsMet(StepContext step);
    }
}
