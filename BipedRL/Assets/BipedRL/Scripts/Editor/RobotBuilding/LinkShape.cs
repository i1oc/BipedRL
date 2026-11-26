using System;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Primitive shape of one link, in the link frame (Unity coordinates, metres).
    /// Capsules are aligned with the link's Y axis.
    /// </summary>
    [Serializable]
    public sealed class LinkShape
    {
        [SerializeField] private string linkName;
        [SerializeField] private LinkShapeType shapeType;
        [SerializeField] private Vector3 center;

        [Tooltip("Full size of the box. Only used by boxes.")]
        [SerializeField] private Vector3 boxSize;

        [Tooltip("Only used by capsules.")]
        [SerializeField] private float capsuleRadius;

        [Tooltip("Total height including both caps. Only used by capsules.")]
        [SerializeField] private float capsuleHeight;

        public string LinkName => linkName;
        public LinkShapeType ShapeType => shapeType;
        public Vector3 Center => center;
        public Vector3 BoxSize => boxSize;
        public float CapsuleRadius => capsuleRadius;
        public float CapsuleHeight => capsuleHeight;

        // Used by Unity's serializer and by the inspector when adding list entries.
        private LinkShape()
        {
        }

        private LinkShape(string linkName, LinkShapeType shapeType, Vector3 center)
        {
            this.linkName = linkName;
            this.shapeType = shapeType;
            this.center = center;
        }

        public static LinkShape Box(string linkName, Vector3 center, Vector3 size)
        {
            return new LinkShape(linkName, LinkShapeType.Box, center) { boxSize = size };
        }

        public static LinkShape Capsule(string linkName, Vector3 center, float radius, float height)
        {
            return new LinkShape(linkName, LinkShapeType.Capsule, center)
            {
                capsuleRadius = radius,
                capsuleHeight = height
            };
        }
    }
}
