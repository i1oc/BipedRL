using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// Always asks for the same velocity. With the default zero target the robot steps in
    /// place, as in level 1.
    /// </summary>
    public sealed class FixedVelocityTarget : MonoBehaviour, IVelocityTargetSource
    {
        [Tooltip("Forward speed, in m/s.")]
        [SerializeField] private float forward;

        [Tooltip("Sideways speed, in m/s; positive is to the right.")]
        [SerializeField] private float sideways;

        [Tooltip("Turning speed, in rad/s; positive turns right.")]
        [SerializeField] private float yawRate;

        public void Restart()
        {
        }

        public VelocityTarget GetTarget(RobotState state, float episodeTime)
        {
            return new VelocityTarget(forward, sideways, yawRate);
        }
    }
}
