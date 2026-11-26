using BipedRL.Editor.Urdf;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// A fixed joint does not create a body: the child link is merged into the parent body,
    /// offset by the joint origin. Physically this is the same rigid body, with fewer parts
    /// for the solver and no extreme mass ratios between neighbouring bodies.
    /// </summary>
    public sealed class FixedJointBuilder : IJointBuilder
    {
        public LinkPlacement Build(UrdfJoint joint, LinkPlacement parentLink)
        {
            return new LinkPlacement(parentLink.Body, parentLink.PoseInBody.Compose(joint.Origin));
        }
    }
}
