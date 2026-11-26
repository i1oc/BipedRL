using System.Collections.Generic;
using BipedRL.Editor.Urdf;
using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Turns a <see cref="UrdfRobot"/> into a GameObject hierarchy of ArticulationBody
    /// components. It walks the link tree from the root; for every joint it asks the
    /// <see cref="JointBuilderFactory"/> for the right builder, then adds the child link's
    /// mass and shape to wherever that builder placed it.
    /// </summary>
    public sealed class ArticulationRobotBuilder
    {
        private readonly RobotJointSettings jointSettings;
        private readonly RobotShapeSettings shapeSettings;
        private readonly JointBuilderFactory jointBuilders;

        public ArticulationRobotBuilder(RobotJointSettings jointSettings, RobotShapeSettings shapeSettings)
        {
            this.jointSettings = jointSettings;
            this.shapeSettings = shapeSettings;
            jointBuilders = new JointBuilderFactory(jointSettings);
        }

        /// <summary>
        /// Builds the robot under a new root GameObject placed at ground level. The root
        /// link's body (the articulation root) floats at the default pelvis height above it.
        /// </summary>
        public GameObject Build(UrdfRobot robot, string rootName)
        {
            var root = new GameObject(rootName);
            BuiltBody rootBody = CreateRootBody(robot.RootLink, root.transform);
            var bodies = new List<BuiltBody> { rootBody };

            BuildLink(robot, robot.RootLink, new LinkPlacement(rootBody, UrdfPose.Identity), bodies);

            foreach (BuiltBody body in bodies)
            {
                body.ApplyMass();
            }

            return root;
        }

        private BuiltBody CreateRootBody(UrdfLink rootLink, Transform parent)
        {
            var gameObject = new GameObject(rootLink.Name);
            gameObject.transform.SetParent(parent, false);
            gameObject.transform.localPosition = Vector3.up * jointSettings.DefaultPelvisHeight;

            var body = gameObject.AddComponent<ArticulationBody>();
            body.immovable = false;
            return new BuiltBody(gameObject, body);
        }

        private void BuildLink(UrdfRobot robot, UrdfLink link, LinkPlacement placement, List<BuiltBody> bodies)
        {
            if (link.Inertial != null)
            {
                placement.Body.AddMass(MassProperties.FromInertial(link.Inertial, placement.PoseInBody));
            }

            if (shapeSettings.TryGetShape(link.Name, out LinkShape shape))
            {
                LinkShapeBuilder.Add(shape, placement);
            }

            foreach (UrdfJoint joint in robot.GetChildJoints(link.Name))
            {
                LinkPlacement childPlacement = jointBuilders.Create(joint.Type).Build(joint, placement);
                if (childPlacement.Body != placement.Body)
                {
                    bodies.Add(childPlacement.Body);
                }

                BuildLink(robot, robot.GetLink(joint.ChildLinkName), childPlacement, bodies);
            }
        }
    }
}
