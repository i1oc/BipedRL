using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using UnityEngine;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Reads a URDF document into a <see cref="UrdfRobot"/>.
    /// Only links (with their inertial data) and joints are read; elements the project
    /// does not need, such as visuals, collisions, materials or simulator extensions,
    /// are ignored. Every value is converted to Unity coordinates through <see cref="RosToUnity"/>.
    /// </summary>
    public static class UrdfParser
    {
        // URDF default axis when <axis> is missing (ROS X).
        private static readonly Vector3 DefaultRosAxis = new Vector3(1f, 0f, 0f);

        public static UrdfRobot ParseFile(string path)
        {
            return Parse(File.ReadAllText(path));
        }

        public static UrdfRobot Parse(string urdfXml)
        {
            XElement robot = LoadRobotElement(urdfXml);

            List<UrdfLink> links = robot.Elements("link").Select(ParseLink).ToList();
            List<UrdfJoint> joints = robot.Elements("joint").Select(ParseJoint).ToList();

            return new UrdfRobot(RequiredAttribute(robot, "name"), links, joints);
        }

        private static XElement LoadRobotElement(string urdfXml)
        {
            XDocument document;
            try
            {
                document = XDocument.Parse(urdfXml);
            }
            catch (XmlException exception)
            {
                throw new UrdfFormatException($"The URDF is not valid XML: {exception.Message}", exception);
            }

            if (document.Root == null || document.Root.Name.LocalName != "robot")
            {
                throw new UrdfFormatException("The root element of a URDF must be <robot>.");
            }

            return document.Root;
        }

        private static UrdfLink ParseLink(XElement link)
        {
            XElement inertial = link.Element("inertial");
            return new UrdfLink(
                RequiredAttribute(link, "name"),
                inertial == null ? null : ParseInertial(inertial));
        }

        private static UrdfInertial ParseInertial(XElement inertial)
        {
            XElement inertia = RequiredElement(inertial, "inertia");

            return new UrdfInertial(
                FloatAttribute(RequiredElement(inertial, "mass"), "value"),
                ParseOrigin(inertial),
                RosToUnity.Inertia(
                    FloatAttribute(inertia, "ixx"),
                    FloatAttribute(inertia, "iyy"),
                    FloatAttribute(inertia, "izz"),
                    FloatAttribute(inertia, "ixy"),
                    FloatAttribute(inertia, "ixz"),
                    FloatAttribute(inertia, "iyz")));
        }

        private static UrdfJoint ParseJoint(XElement joint)
        {
            string name = RequiredAttribute(joint, "name");
            UrdfJointType type = ParseJointType(RequiredAttribute(joint, "type"), name);
            XElement axis = joint.Element("axis");
            XElement limit = joint.Element("limit");

            if (limit == null && (type == UrdfJointType.Revolute || type == UrdfJointType.Prismatic))
            {
                throw new UrdfFormatException($"Joint '{name}' is {type} and needs a <limit> element.");
            }

            return new UrdfJoint(
                name,
                type,
                RequiredAttribute(RequiredElement(joint, "parent"), "link"),
                RequiredAttribute(RequiredElement(joint, "child"), "link"),
                ParseOrigin(joint),
                RosToUnity.Axis(axis == null ? DefaultRosAxis : VectorAttribute(axis, "xyz")),
                limit == null ? null : ParseLimit(limit));
        }

        private static UrdfLimit ParseLimit(XElement limit)
        {
            return new UrdfLimit(
                OptionalFloatAttribute(limit, "lower", 0f),
                OptionalFloatAttribute(limit, "upper", 0f),
                FloatAttribute(limit, "effort"),
                FloatAttribute(limit, "velocity"));
        }

        private static UrdfJointType ParseJointType(string type, string jointName)
        {
            switch (type)
            {
                case "revolute": return UrdfJointType.Revolute;
                case "continuous": return UrdfJointType.Continuous;
                case "prismatic": return UrdfJointType.Prismatic;
                case "fixed": return UrdfJointType.Fixed;
                case "floating": return UrdfJointType.Floating;
                case "planar": return UrdfJointType.Planar;
                default:
                    throw new UrdfFormatException($"Joint '{jointName}' has unknown type '{type}'.");
            }
        }

        // A missing <origin>, or a missing xyz or rpy attribute, means zero (URDF specification).
        private static UrdfPose ParseOrigin(XElement parent)
        {
            XElement origin = parent.Element("origin");
            if (origin == null)
            {
                return UrdfPose.Identity;
            }

            Vector3 xyz = origin.Attribute("xyz") == null ? Vector3.zero : VectorAttribute(origin, "xyz");
            Vector3 rpy = origin.Attribute("rpy") == null ? Vector3.zero : VectorAttribute(origin, "rpy");
            return RosToUnity.Pose(xyz, rpy);
        }

        private static XElement RequiredElement(XElement parent, string elementName)
        {
            XElement element = parent.Element(elementName);
            if (element == null)
            {
                throw new UrdfFormatException(
                    $"<{parent.Name.LocalName}> {Describe(parent)}is missing the <{elementName}> element.");
            }

            return element;
        }

        private static string RequiredAttribute(XElement element, string attributeName)
        {
            XAttribute attribute = element.Attribute(attributeName);
            if (attribute == null || string.IsNullOrWhiteSpace(attribute.Value))
            {
                throw new UrdfFormatException(
                    $"<{element.Name.LocalName}> {Describe(element)}is missing the '{attributeName}' attribute.");
            }

            return attribute.Value.Trim();
        }

        private static float FloatAttribute(XElement element, string attributeName)
        {
            return ParseFloat(RequiredAttribute(element, attributeName), element, attributeName);
        }

        private static float OptionalFloatAttribute(XElement element, string attributeName, float defaultValue)
        {
            return element.Attribute(attributeName) == null
                ? defaultValue
                : FloatAttribute(element, attributeName);
        }

        private static Vector3 VectorAttribute(XElement element, string attributeName)
        {
            string[] parts = RequiredAttribute(element, attributeName)
                .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 3)
            {
                throw new UrdfFormatException(
                    $"'{attributeName}' of <{element.Name.LocalName}> must have 3 numbers, but has {parts.Length}.");
            }

            return new Vector3(
                ParseFloat(parts[0], element, attributeName),
                ParseFloat(parts[1], element, attributeName),
                ParseFloat(parts[2], element, attributeName));
        }

        // Invariant culture: URDF always uses '.' as decimal separator, whatever the OS language.
        private static float ParseFloat(string text, XElement element, string attributeName)
        {
            if (!float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float value))
            {
                throw new UrdfFormatException(
                    $"'{attributeName}' of <{element.Name.LocalName}> is not a number: '{text}'.");
            }

            return value;
        }

        private static string Describe(XElement element)
        {
            XAttribute name = element.Attribute("name");
            return name == null ? string.Empty : $"'{name.Value}' ";
        }
    }
}
