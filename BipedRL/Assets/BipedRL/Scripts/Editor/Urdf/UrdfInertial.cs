namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Mass properties of a link: mass, centre of mass frame and inertia tensor.
    /// The inertia tensor is expressed in the <see cref="Origin"/> frame.
    /// </summary>
    public sealed class UrdfInertial
    {
        public float Mass { get; }
        public UrdfPose Origin { get; }
        public UrdfInertia Inertia { get; }

        public UrdfInertial(float mass, UrdfPose origin, UrdfInertia inertia)
        {
            Mass = mass;
            Origin = origin;
            Inertia = inertia;
        }
    }
}
