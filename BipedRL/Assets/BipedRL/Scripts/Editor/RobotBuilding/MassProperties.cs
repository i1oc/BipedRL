using BipedRL.Editor.Urdf;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Mass, centre of mass and inertia tensor of a rigid body, in the body frame.
    /// Used to merge links joined by fixed joints into one body.
    /// </summary>
    public readonly struct MassProperties
    {
        // ArticulationBody needs strictly positive inertia; tiny links get at least this.
        private const float MinimumMoment = 1e-5f;

        public static readonly MassProperties Empty = new MassProperties(0f, Vector3.zero, SymmetricMatrix3.Zero);

        public float Mass { get; }
        public Vector3 CenterOfMass { get; }

        /// <summary>Inertia about the centre of mass, with axes parallel to the body frame.</summary>
        public SymmetricMatrix3 Inertia { get; }

        public MassProperties(float mass, Vector3 centerOfMass, SymmetricMatrix3 inertia)
        {
            Mass = mass;
            CenterOfMass = centerOfMass;
            Inertia = inertia;
        }

        /// <summary>
        /// Mass properties of a URDF link placed at <paramref name="linkPoseInBody"/>.
        /// </summary>
        public static MassProperties FromInertial(UrdfInertial inertial, UrdfPose linkPoseInBody)
        {
            UrdfPose inertialPoseInBody = linkPoseInBody.Compose(inertial.Origin);
            return new MassProperties(
                inertial.Mass,
                inertialPoseInBody.Position,
                SymmetricMatrix3.FromInertia(inertial.Inertia).Rotated(inertialPoseInBody.Rotation));
        }

        /// <summary>
        /// Properties of two parts rigidly joined: masses add, the centre of mass is the
        /// weighted average, and each inertia is moved to the new centre of mass with the
        /// parallel axis theorem.
        /// </summary>
        public MassProperties Combine(MassProperties other)
        {
            float totalMass = Mass + other.Mass;
            if (totalMass <= 0f)
            {
                return Empty;
            }

            Vector3 center = (Mass * CenterOfMass + other.Mass * other.CenterOfMass) / totalMass;
            SymmetricMatrix3 inertia =
                Inertia + SymmetricMatrix3.PointMass(Mass, CenterOfMass - center) +
                other.Inertia + SymmetricMatrix3.PointMass(other.Mass, other.CenterOfMass - center);

            return new MassProperties(totalMass, center, inertia);
        }

        public void ApplyTo(ArticulationBody body)
        {
            Inertia.ToPrincipalAxes(out Vector3 moments, out Quaternion axes);

            body.automaticCenterOfMass = false;
            body.automaticInertiaTensor = false;
            body.mass = Mass;
            body.centerOfMass = CenterOfMass;
            body.inertiaTensor = new Vector3(
                Mathf.Max(moments.x, MinimumMoment),
                Mathf.Max(moments.y, MinimumMoment),
                Mathf.Max(moments.z, MinimumMoment));
            body.inertiaTensorRotation = axes;
        }
    }
}
