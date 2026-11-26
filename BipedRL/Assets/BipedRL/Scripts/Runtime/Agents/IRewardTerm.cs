namespace BipedRL.Agents
{
    /// <summary>
    /// Strategy pattern: one term of the reward. The agent adds Weight * Evaluate for every
    /// term, so a positive weight rewards the value and a negative weight penalizes it.
    /// </summary>
    public interface IRewardTerm
    {
        float Weight { get; }

        /// <summary>Raw value of the term, before applying the weight.</summary>
        float Evaluate(StepContext step);
    }
}
