using System.Collections.Generic;
using System.Linq;
using BipedRL.Editor.Urdf;
using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Turns a <see cref="UrdfRobot"/> into a GameObject hierarchy of ArticulationBody
    /// components. It walks the link tree from the root; for every joint it asks the
    /// <see cref="JointBuilderFactory"/> for the right builder, then adds the child link's
    /// mass and shape to wherever that builder placed it. Finally it adds the
    /// <see cref="RobotBody"/> facade and connects it to the joints and feet.
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
        /// <exception cref="KeyNotFoundException">
        /// A joint or foot in the settings does not exist in the URDF.
        /// </exception>
        public GameObject Build(UrdfRobot robot, string rootName)
        {
            var root = new GameObject(rootName);
            BuiltBody rootBody = CreateRootBody(robot.RootLink, root.transform);
            var parts = new RobotParts(rootBody);

            BuildLink(robot, robot.RootLink, new LinkPlacement(rootBody, UrdfPose.Identity), parts);

            foreach (BuiltBody body in parts.Bodies)
            {
                body.ApplyMass();
            }

            root.AddComponent<RobotBody>().Configure(
                jointSettings,
                rootBody.ArticulationBody,
                jointSettings.Joints.Select(joint => parts.JointBodies[joint.JointName]),
                shapeSettings.FootLinks.Select(link => parts.Feet[link]));
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

        private void BuildLink(UrdfRobot robot, UrdfLink link, LinkPlacement placement, RobotParts parts)
        {
            if (link.Inertial != null)
            {
                placement.Body.AddMass(MassProperties.FromInertial(link.Inertial, placement.PoseInBody));
            }

            if (shapeSettings.TryGetShape(link.Name, out LinkShape shape))
            {
                LinkShapeBuilder.Add(shape, placement);
            }

            if (shapeSettings.IsFoot(link.Name))
            {
                parts.Feet[link.Name] = placement.Body.GameObject.AddComponent<FootContact>();
            }

            foreach (UrdfJoint joint in robot.GetChildJoints(link.Name))
            {
                LinkPlacement childPlacement = jointBuilders.Create(joint.Type).Build(joint, placement);
                if (childPlacement.Body != placement.Body)
                {
                    parts.Bodies.Add(childPlacement.Body);
                    parts.JointBodies[joint.Name] = childPlacement.Body.ArticulationBody;
                }

                BuildLink(robot, robot.GetLink(joint.ChildLinkName), childPlacement, parts);
            }
        }

        /// <summary>Everything created while walking the link tree.</summary>
        private sealed class RobotParts
        {
            public List<BuiltBody> Bodies { get; }
            public Dictionary<string, ArticulationBody> JointBodies { get; } = new Dictionary<string, ArticulationBody>();
            public Dictionary<string, FootContact> Feet { get; } = new Dictionary<string, FootContact>();

            public RobotParts(BuiltBody rootBody)
            {
                Bodies = new List<BuiltBody> { rootBody };
            }
        }
    }
}
