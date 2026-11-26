using BipedRL.Editor.Urdf;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Where a URDF link ends up: the body it belongs to and its pose inside that body.
    /// The pose is the identity when the link has its own body, and an offset when the link
    /// was merged into a parent body through fixed joints.
    /// </summary>
    public readonly struct LinkPlacement
    {
        public BuiltBody Body { get; }
        public UrdfPose PoseInBody { get; }

        public LinkPlacement(BuiltBody body, UrdfPose poseInBody)
        {
            Body = body;
            PoseInBody = poseInBody;
        }
    }
}
