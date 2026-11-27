namespace BipedRL.Agents
{
    /// <summary>
    /// The velocity the robot is asked to move at, in the pelvis frame. Unitree's training
    /// calls it a command; here it is a target, so it is not mistaken for the Command pattern.
    /// </summary>
    public readonly struct VelocityTarget
    {
        public static readonly VelocityTarget Zero = new VelocityTarget(0f, 0f, 0f);

        /// <summary>Forward speed, in m/s; negative walks backwards.</summary>
        public float Forward { get; }

        /// <summary>Sideways speed, in m/s; positive walks to the right.</summary>
        public float Sideways { get; }

        /// <summary>Turning speed around the vertical axis, in rad/s; positive turns right.</summary>
        public float YawRate { get; }

        public VelocityTarget(float forward, float sideways, float yawRate)
        {
            Forward = forward;
            Sideways = sideways;
            YawRate = yawRate;
        }
    }
}
