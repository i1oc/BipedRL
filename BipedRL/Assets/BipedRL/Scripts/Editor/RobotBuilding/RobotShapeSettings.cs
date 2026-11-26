using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Primitive shapes of a robot's links. Links without an entry get no collider or visual,
    /// although their mass is still simulated.
    /// </summary>
    [CreateAssetMenu(fileName = "RobotShapeSettings", menuName = "BipedRL/Robot Shape Settings")]
    public sealed class RobotShapeSettings : ScriptableObject
    {
        [SerializeField] private List<LinkShape> shapes = new List<LinkShape>();

        public IReadOnlyList<LinkShape> Shapes => shapes;

        public bool TryGetShape(string linkName, out LinkShape shape)
        {
            shape = shapes.FirstOrDefault(entry => entry.LinkName == linkName);
            return shape != null;
        }

        /// <summary>
        /// Replaces every shape. Used by editor tools that create the asset with default values.
        /// </summary>
        public void Configure(IEnumerable<LinkShape> linkShapes)
        {
            shapes = linkShapes.ToList();
        }
    }
}
