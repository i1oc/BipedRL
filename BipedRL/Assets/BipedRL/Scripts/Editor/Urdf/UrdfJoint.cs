using UnityEngine;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Connection between a parent link and a child link.
    /// </summary>
    public sealed class UrdfJoint
    {
        public string Name { get; }
        public UrdfJointType Type { get; }
        public string ParentLinkName { get; }
        public string ChildLinkName { get; }

        /// <summary>Pose of the child link frame relative to the parent link frame.</summary>
        public UrdfPose Origin { get; }

        /// <summary>
        /// Rotation (or translation) axis in the joint frame, in Unity coordinates.
        /// A positive joint value turns the child around this axis by Unity's rule,
        /// which matches the positive direction defined in the URDF.
        /// </summary>
        public Vector3 Axis { get; }

        /// <summary>Joint limits, or null when the URDF gives none (fixed or floating joints).</summary>
        public UrdfLimit Limit { get; }

        public UrdfJoint(string name, UrdfJointType type, string parentLinkName, string childLinkName,
            UrdfPose origin, Vector3 axis, UrdfLimit limit)
        {
            Name = name;
            Type = type;
            ParentLinkName = parentLinkName;
            ChildLinkName = childLinkName;
            Origin = origin;
            Axis = axis;
            Limit = limit;
        }
    }
}
