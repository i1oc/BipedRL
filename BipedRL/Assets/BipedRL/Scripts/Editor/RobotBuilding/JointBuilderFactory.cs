using System;
using BipedRL.Editor.Urdf;
using BipedRL.Robot;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Factory pattern: returns the builder for each URDF joint type, so the robot builder
    /// never branches on joint types. Supporting a new type means adding a builder class
    /// and one line here.
    /// </summary>
    public sealed class JointBuilderFactory
    {
        private readonly IJointBuilder revoluteBuilder;
        private readonly IJointBuilder fixedBuilder;

        public JointBuilderFactory(RobotJointSettings jointSettings)
        {
            revoluteBuilder = new RevoluteJointBuilder(jointSettings);
            fixedBuilder = new FixedJointBuilder();
        }

        public IJointBuilder Create(UrdfJointType jointType)
        {
            switch (jointType)
            {
                case UrdfJointType.Revolute:
                    return revoluteBuilder;
                case UrdfJointType.Fixed:
                    return fixedBuilder;
                default:
                    throw new NotSupportedException(
                        $"URDF joints of type {jointType} are not supported: the H1 legs only use revolute and fixed joints.");
            }
        }
    }
}
