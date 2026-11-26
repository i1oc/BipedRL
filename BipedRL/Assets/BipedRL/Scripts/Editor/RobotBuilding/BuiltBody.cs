using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// A rigid body created by the builder. It accumulates the mass of every URDF link
    /// merged into it and applies the total once the whole robot is built.
    /// </summary>
    public sealed class BuiltBody
    {
        public GameObject GameObject { get; }
        public ArticulationBody ArticulationBody { get; }
        public MassProperties Mass { get; private set; } = MassProperties.Empty;

        public BuiltBody(GameObject gameObject, ArticulationBody articulationBody)
        {
            GameObject = gameObject;
            ArticulationBody = articulationBody;
        }

        public void AddMass(MassProperties mass)
        {
            Mass = Mass.Combine(mass);
        }

        public void ApplyMass()
        {
            Mass.ApplyTo(ArticulationBody);
        }
    }
}
