using System.Collections.Generic;
using BipedRL.Robot;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Default values for the Unitree H1, used only to create the settings assets the first
    /// time. After that, the assets are the source of truth and can be tuned in the inspector.
    /// </summary>
    public static class H1Defaults
    {
        // Pelvis height of the default pose: hip 0.1742 m below the pelvis, thigh and shank
        // 0.4 m each at -0.1 and +0.2 rad from vertical, and the sole 0.07 m below the ankle.
        // That gives about 1.034 m; 1.04 m leaves the feet just above the ground.
        public const float PelvisHeight = 1.04f;

        private static readonly string[] Sides = { "left", "right" };

        /// <summary>
        /// Gains and default angles from Unitree's H1 configuration in unitree_rl_gym
        /// (legged_gym/envs/h1/h1_config.py).
        /// </summary>
        public static IEnumerable<JointSettings> Joints()
        {
            foreach (string side in Sides)
            {
                yield return new JointSettings($"{side}_hip_yaw_joint", 150f, 2f, 0f);
                yield return new JointSettings($"{side}_hip_roll_joint", 150f, 2f, 0f);
                yield return new JointSettings($"{side}_hip_pitch_joint", 150f, 2f, -0.1f);
                yield return new JointSettings($"{side}_knee_joint", 200f, 4f, 0.3f);
                yield return new JointSettings($"{side}_ankle_joint", 40f, 2f, -0.2f);
            }
        }

        /// <summary>
        /// Shapes sized from the bounding boxes of Unitree's H1 meshes, converted to Unity
        /// coordinates. The hip yaw and hip roll motor housings get no shape: they sit inside
        /// the pelvis area and never touch the ground.
        /// </summary>
        public static IEnumerable<LinkShape> Shapes()
        {
            yield return LinkShape.Box("pelvis", new Vector3(0f, -0.0385f, 0f), new Vector3(0.307f, 0.093f, 0.138f));
            yield return LinkShape.Box("torso_link", new Vector3(0f, 0.377f, 0.026f), new Vector3(0.344f, 0.754f, 0.198f));

            foreach (string side in Sides)
            {
                // Thigh and shank capsules leave a gap at the knee and ankle so neighbours never overlap.
                yield return LinkShape.Capsule($"{side}_hip_pitch_link", new Vector3(0f, -0.2f, 0f), 0.05f, 0.34f);
                yield return LinkShape.Capsule($"{side}_knee_link", new Vector3(0f, -0.2f, 0f), 0.045f, 0.34f);
                yield return LinkShape.Box($"{side}_ankle_link", new Vector3(0f, -0.0295f, 0.05f), new Vector3(0.08f, 0.081f, 0.3f));
            }
        }
    }
}
