using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Collision geometry of a robot: the primitive shape of each link and which links are
    /// feet. Links without a shape get no collider or visual, although their mass is still
    /// simulated.
    /// </summary>
    [CreateAssetMenu(fileName = "RobotShapeSettings", menuName = "BipedRL/Robot Shape Settings")]
    public sealed class RobotShapeSettings : ScriptableObject
    {
        [SerializeField] private List<LinkShape> shapes = new List<LinkShape>();

        [Tooltip("Links that get a foot contact sensor, in the order the robot reports them.")]
        [SerializeField] private List<string> footLinks = new List<string>();

        public IReadOnlyList<LinkShape> Shapes => shapes;
        public IReadOnlyList<string> FootLinks => footLinks;

        public bool TryGetShape(string linkName, out LinkShape shape)
        {
            shape = shapes.FirstOrDefault(entry => entry.LinkName == linkName);
            return shape != null;
        }

        public bool IsFoot(string linkName)
        {
            return footLinks.Contains(linkName);
        }

        /// <summary>
        /// Replaces every value. Used by editor tools that create the asset with default values.
        /// </summary>
        public void Configure(IEnumerable<LinkShape> linkShapes, IEnumerable<string> feet)
        {
            shapes = linkShapes.ToList();
            footLinks = feet.ToList();
        }
    }
}
