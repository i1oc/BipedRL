using BipedRL.Editor.Urdf;
using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// A revolute joint creates a new body connected to its parent by a limited hinge with a
    /// PD drive. The drive target is the default angle, so the robot holds its default pose
    /// without any script.
    /// </summary>
    public sealed class RevoluteJointBuilder : IJointBuilder
    {
        private readonly RobotJointSettings settings;

        public RevoluteJointBuilder(RobotJointSettings settings)
        {
            this.settings = settings;
        }

        public LinkPlacement Build(UrdfJoint joint, LinkPlacement parentLink)
        {
            UrdfPose jointPoseInParentBody = parentLink.PoseInBody.Compose(joint.Origin);

            var gameObject = new GameObject(joint.ChildLinkName);
            gameObject.transform.SetParent(parentLink.Body.GameObject.transform, false);
            gameObject.transform.localPosition = jointPoseInParentBody.Position;
            gameObject.transform.localRotation = jointPoseInParentBody.Rotation;

            var body = gameObject.AddComponent<ArticulationBody>();
            body.jointType = ArticulationJointType.RevoluteJoint;
            body.anchorPosition = Vector3.zero;
            // ArticulationBody hinges turn about the anchor's X axis, so X is aligned with the URDF axis.
            body.anchorRotation = Quaternion.FromToRotation(Vector3.right, joint.Axis);
            body.twistLock = ArticulationDofLock.LimitedMotion;
            body.xDrive = CreateDrive(joint, settings.GetJoint(joint.Name));

            return new LinkPlacement(new BuiltBody(gameObject, body), UrdfPose.Identity);
        }

        private static ArticulationDrive CreateDrive(UrdfJoint joint, JointSettings jointSettings)
        {
            // Limits and target are in degrees for angular drives, but the gains are applied per
            // radian of error (checked with the joint state log), so they stay in SI units.
            return new ArticulationDrive
            {
                driveType = ArticulationDriveType.Force,
                lowerLimit = joint.Limit.Lower * Mathf.Rad2Deg,
                upperLimit = joint.Limit.Upper * Mathf.Rad2Deg,
                stiffness = jointSettings.Stiffness,
                damping = jointSettings.Damping,
                forceLimit = joint.Limit.Effort,
                target = jointSettings.DefaultAngle * Mathf.Rad2Deg,
                targetVelocity = 0f
            };
        }
    }
}
