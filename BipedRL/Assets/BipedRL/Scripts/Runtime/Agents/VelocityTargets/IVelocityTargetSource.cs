using BipedRL.Robot;

namespace BipedRL.Agents
{
    /// <summary>
    /// Strategy pattern: decides the velocity a <see cref="LocomotionAgent"/> is asked to move
    /// at. Training uses random targets, level 1 a zero target and level 2 the way to the goal.
    /// </summary>
    public interface IVelocityTargetSource
    {
        /// <summary>Called when an episode starts.</summary>
        void Restart();

        /// <summary>
        /// The target for the current decision. <paramref name="episodeTime"/> is the time since
        /// the episode started, in seconds.
        /// </summary>
        VelocityTarget GetTarget(RobotState state, float episodeTime);
    }
}
