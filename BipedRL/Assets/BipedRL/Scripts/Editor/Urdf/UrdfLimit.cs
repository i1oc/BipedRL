namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Limits of a joint, in URDF units: radians (or metres for prismatic joints),
    /// newton-metres (or newtons) and radians per second (or metres per second).
    /// </summary>
    public sealed class UrdfLimit
    {
        public float Lower { get; }
        public float Upper { get; }
        public float Effort { get; }
        public float Velocity { get; }

        public UrdfLimit(float lower, float upper, float effort, float velocity)
        {
            Lower = lower;
            Upper = upper;
            Effort = effort;
            Velocity = velocity;
        }
    }
}
