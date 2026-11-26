using UnityEngine;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Position and rotation of a frame relative to its parent frame,
    /// already converted to Unity coordinates.
    /// </summary>
    public readonly struct UrdfPose
    {
        public static readonly UrdfPose Identity = new UrdfPose(Vector3.zero, Quaternion.identity);

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }

        public UrdfPose(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        /// <summary>
        /// Pose of a frame given relative to this one, expressed in this pose's parent frame.
        /// </summary>
        public UrdfPose Compose(UrdfPose child)
        {
            return new UrdfPose(Position + Rotation * child.Position, Rotation * child.Rotation);
        }
    }
}
