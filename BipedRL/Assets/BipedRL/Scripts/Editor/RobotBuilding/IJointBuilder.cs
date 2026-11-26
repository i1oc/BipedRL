using BipedRL.Editor.Urdf;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Builds one kind of URDF joint. Each implementation decides how the child link is
    /// placed: in a new body with a movable joint, or merged into the parent body.
    /// </summary>
    public interface IJointBuilder
    {
        /// <summary>Returns where the joint's child link goes.</summary>
        LinkPlacement Build(UrdfJoint joint, LinkPlacement parentLink);
    }
}
