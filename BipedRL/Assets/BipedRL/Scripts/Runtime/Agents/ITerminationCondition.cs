namespace BipedRL.Agents
{
    /// <summary>
    /// Strategy pattern: a reason to end the episode. The episode ends as soon as any
    /// condition is met.
    /// </summary>
    public interface ITerminationCondition
    {
        bool IsMet(StepContext step);
    }
}
