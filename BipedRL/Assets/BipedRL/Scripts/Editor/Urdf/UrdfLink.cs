namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// A rigid body of the robot. Visual and collision geometry are not read:
    /// the robot is built with primitive shapes instead.
    /// </summary>
    public sealed class UrdfLink
    {
        public string Name { get; }

        /// <summary>Mass properties, or null when the URDF gives none (massless link).</summary>
        public UrdfInertial Inertial { get; }

        public UrdfLink(string name, UrdfInertial inertial)
        {
            Name = name;
            Inertial = inertial;
        }
    }
}
