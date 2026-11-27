using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Random targets for training, as in Unitree's H1 training: a new walking velocity and a
    /// heading to face at the start of each episode and every few seconds. The turning speed
    /// follows the heading error, so the robot learns to turn and then to keep its heading.
    /// </summary>
    public sealed class RandomVelocityTargets : MonoBehaviour, IVelocityTargetSource
    {
        [SerializeField] private VelocityTargetRanges ranges;

        private float forward;
        private float sideways;
        private float heading;
        private float nextResampleTime;

        /// <summary>Used by editor tools that create agent prefabs.</summary>
        public void Configure(VelocityTargetRanges targetRanges)
        {
            ranges = targetRanges;
        }

        public void Restart()
        {
            Resample();
            nextResampleTime = ranges.ResampleInterval;
        }

        public VelocityTarget GetTarget(RobotState state, float episodeTime)
        {
            if (episodeTime >= nextResampleTime)
            {
                Resample();
                nextResampleTime += ranges.ResampleInterval;
            }

            float headingError = Mathf.DeltaAngle(state.Heading * Mathf.Rad2Deg, heading * Mathf.Rad2Deg) * Mathf.Deg2Rad;
            float yawRate = Mathf.Clamp(ranges.HeadingGain * headingError, -ranges.MaxYawRate, ranges.MaxYawRate);
            return new VelocityTarget(forward, sideways, yawRate);
        }

        private void Resample()
        {
            forward = Random.Range(ranges.Forward.x, ranges.Forward.y);
            sideways = Random.Range(ranges.Sideways.x, ranges.Sideways.y);
            heading = Random.Range(-Mathf.PI, Mathf.PI);

            if (new Vector2(forward, sideways).magnitude < ranges.MinSpeed)
            {
                forward = 0f;
                sideways = 0f;
            }
        }
    }
}
