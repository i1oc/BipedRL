using UnityEngine;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Converts URDF values from ROS coordinates to Unity coordinates.
    /// This is the only place in the project where that conversion happens.
    /// </summary>
    /// <remarks>
    /// ROS is right-handed with X forward, Y left and Z up.
    /// Unity is left-handed with X right, Y up and Z forward.
    /// A ROS vector (x, y, z) is therefore the Unity vector (-y, z, x).
    /// Because the handedness changes, a rotation of angle θ about a ROS axis a is the
    /// same physical rotation as an angle θ about the Unity axis -(-a.y, a.z, a.x).
    /// </remarks>
    public static class RosToUnity
    {
        public static Vector3 Position(Vector3 ros)
        {
            return new Vector3(-ros.y, ros.z, ros.x);
        }

        /// <summary>
        /// Converts a joint axis. The axis is negated so that a positive joint angle keeps
        /// the meaning it has in the URDF. That way URDF limits and Unitree's default
        /// angles can be used as they are, without changing any sign.
        /// </summary>
        public static Vector3 Axis(Vector3 ros)
        {
            return -Position(ros);
        }

        /// <summary>
        /// Converts URDF roll-pitch-yaw angles (radians, fixed axes X, Y, Z) to a Unity rotation.
        /// </summary>
        public static Quaternion Rotation(Vector3 rollPitchYaw)
        {
            Quaternion ros = RollPitchYawToRosQuaternion(rollPitchYaw.x, rollPitchYaw.y, rollPitchYaw.z);
            return new Quaternion(ros.y, -ros.z, -ros.x, ros.w);
        }

        public static UrdfPose Pose(Vector3 rosPosition, Vector3 rollPitchYaw)
        {
            return new UrdfPose(Position(rosPosition), Rotation(rollPitchYaw));
        }

        /// <summary>
        /// Converts an inertia tensor. With the axis mapping above, Unity X, Y, Z are
        /// ROS -Y, Z, X, so the diagonal is permuted and the products of inertia change
        /// sign where exactly one negated axis is involved.
        /// </summary>
        public static UrdfInertia Inertia(float ixx, float iyy, float izz, float ixy, float ixz, float iyz)
        {
            return new UrdfInertia(
                ixx: iyy,
                iyy: izz,
                izz: ixx,
                ixy: -iyz,
                ixz: -ixy,
                iyz: ixz);
        }

        // q = q_yaw(Z) * q_pitch(Y) * q_roll(X): rotations about fixed axes, as URDF defines rpy.
        private static Quaternion RollPitchYawToRosQuaternion(float roll, float pitch, float yaw)
        {
            float cr = Mathf.Cos(roll * 0.5f);
            float sr = Mathf.Sin(roll * 0.5f);
            float cp = Mathf.Cos(pitch * 0.5f);
            float sp = Mathf.Sin(pitch * 0.5f);
            float cy = Mathf.Cos(yaw * 0.5f);
            float sy = Mathf.Sin(yaw * 0.5f);

            return new Quaternion(
                sr * cp * cy - cr * sp * sy,
                cr * sp * cy + sr * cp * sy,
                cr * cp * sy - sr * sp * cy,
                cr * cp * cy + sr * sp * sy);
        }
    }
}
