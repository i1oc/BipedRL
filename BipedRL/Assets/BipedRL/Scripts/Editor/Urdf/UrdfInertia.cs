namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Symmetric 3x3 inertia tensor (kg·m²), already converted to Unity coordinates.
    /// Only the six independent components are stored.
    /// </summary>
    public readonly struct UrdfInertia
    {
        public float Ixx { get; }
        public float Iyy { get; }
        public float Izz { get; }
        public float Ixy { get; }
        public float Ixz { get; }
        public float Iyz { get; }

        public UrdfInertia(float ixx, float iyy, float izz, float ixy, float ixz, float iyz)
        {
            Ixx = ixx;
            Iyy = iyy;
            Izz = izz;
            Ixy = ixy;
            Ixz = ixz;
            Iyz = iyz;
        }
    }
}
