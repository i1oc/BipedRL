using BipedRL.Editor.Urdf;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Creates the primitive that is both collider and visual of a link. It is a child of the
    /// link's body, so the collider is part of that body.
    /// </summary>
    public static class LinkShapeBuilder
    {
        public static void Add(LinkShape shape, LinkPlacement placement)
        {
            GameObject primitive = GameObject.CreatePrimitive(
                shape.ShapeType == LinkShapeType.Box ? PrimitiveType.Cube : PrimitiveType.Capsule);
            primitive.name = $"{shape.LinkName}_shape";

            UrdfPose pose = placement.PoseInBody.Compose(new UrdfPose(shape.Center, Quaternion.identity));
            primitive.transform.SetParent(placement.Body.GameObject.transform, false);
            primitive.transform.localPosition = pose.Position;
            primitive.transform.localRotation = pose.Rotation;
            primitive.transform.localScale = ScaleFor(shape);
        }

        // Unity's cube is 1 m wide; its capsule is 2 m tall with a 0.5 m radius along Y.
        private static Vector3 ScaleFor(LinkShape shape)
        {
            if (shape.ShapeType == LinkShapeType.Box)
            {
                return shape.BoxSize;
            }

            float diameter = 2f * shape.CapsuleRadius;
            return new Vector3(diameter, shape.CapsuleHeight / 2f, diameter);
        }
    }
}
