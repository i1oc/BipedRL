using System;
using System.Collections.Generic;
using System.Linq;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// A robot description read from a URDF file: a tree of links connected by joints.
    /// The constructor checks that the links and joints form a valid tree, so the rest
    /// of the code can rely on it.
    /// </summary>
    public sealed class UrdfRobot
    {
        private readonly Dictionary<string, UrdfLink> linksByName;
        private readonly Dictionary<string, List<UrdfJoint>> childJointsByParent;

        public string Name { get; }
        public IReadOnlyList<UrdfLink> Links { get; }
        public IReadOnlyList<UrdfJoint> Joints { get; }

        /// <summary>The only link that is not the child of any joint.</summary>
        public UrdfLink RootLink { get; }

        /// <summary>Sum of the masses of all links, in kilograms.</summary>
        public float TotalMass => Links.Where(link => link.Inertial != null).Sum(link => link.Inertial.Mass);

        public UrdfRobot(string name, IReadOnlyList<UrdfLink> links, IReadOnlyList<UrdfJoint> joints)
        {
            Name = name;
            Links = links;
            Joints = joints;

            linksByName = IndexLinks(links);
            childJointsByParent = IndexJoints(joints, linksByName);
            RootLink = FindRoot(links, joints);
        }

        public UrdfLink GetLink(string linkName)
        {
            if (!linksByName.TryGetValue(linkName, out UrdfLink link))
            {
                throw new KeyNotFoundException($"The robot has no link named '{linkName}'.");
            }

            return link;
        }

        /// <summary>Joints whose parent is the given link, in file order.</summary>
        public IReadOnlyList<UrdfJoint> GetChildJoints(string linkName)
        {
            return childJointsByParent.TryGetValue(linkName, out List<UrdfJoint> children)
                ? children
                : (IReadOnlyList<UrdfJoint>)Array.Empty<UrdfJoint>();
        }

        private static Dictionary<string, UrdfLink> IndexLinks(IReadOnlyList<UrdfLink> links)
        {
            var index = new Dictionary<string, UrdfLink>();
            foreach (UrdfLink link in links)
            {
                if (index.ContainsKey(link.Name))
                {
                    throw new UrdfFormatException($"Duplicate link name '{link.Name}'.");
                }

                index.Add(link.Name, link);
            }

            return index;
        }

        private static Dictionary<string, List<UrdfJoint>> IndexJoints(
            IReadOnlyList<UrdfJoint> joints, Dictionary<string, UrdfLink> linksByName)
        {
            var childrenByParent = new Dictionary<string, List<UrdfJoint>>();
            var linksWithParent = new HashSet<string>();

            foreach (UrdfJoint joint in joints)
            {
                RequireLink(joint, joint.ParentLinkName, linksByName);
                RequireLink(joint, joint.ChildLinkName, linksByName);

                if (!linksWithParent.Add(joint.ChildLinkName))
                {
                    throw new UrdfFormatException(
                        $"Link '{joint.ChildLinkName}' is the child of more than one joint.");
                }

                if (!childrenByParent.TryGetValue(joint.ParentLinkName, out List<UrdfJoint> children))
                {
                    children = new List<UrdfJoint>();
                    childrenByParent.Add(joint.ParentLinkName, children);
                }

                children.Add(joint);
            }

            return childrenByParent;
        }

        private static void RequireLink(UrdfJoint joint, string linkName, Dictionary<string, UrdfLink> linksByName)
        {
            if (!linksByName.ContainsKey(linkName))
            {
                throw new UrdfFormatException($"Joint '{joint.Name}' refers to unknown link '{linkName}'.");
            }
        }

        private static UrdfLink FindRoot(IReadOnlyList<UrdfLink> links, IReadOnlyList<UrdfJoint> joints)
        {
            var childNames = new HashSet<string>(joints.Select(joint => joint.ChildLinkName));
            List<UrdfLink> roots = links.Where(link => !childNames.Contains(link.Name)).ToList();

            if (roots.Count != 1)
            {
                throw new UrdfFormatException(
                    $"A robot must have exactly one root link, but this one has {roots.Count}.");
            }

            return roots[0];
        }
    }
}
