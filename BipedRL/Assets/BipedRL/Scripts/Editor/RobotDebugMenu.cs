using System.Text;
using BipedRL.Robot;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor
{
    /// <summary>
    /// Play mode menus to check the robots in the scene through their <see cref="RobotBody"/>
    /// facade, the same way agents and levels use them.
    /// </summary>
    public static class RobotDebugMenu
    {
        private const string LogStatePath = "BipedRL/Robot/Log Robot State (Play Mode)";
        private const string ResetPath = "BipedRL/Robot/Reset Robots (Play Mode)";

        [MenuItem(LogStatePath)]
        private static void LogRobotState()
        {
            foreach (RobotBody robot in Object.FindObjectsByType<RobotBody>())
            {
                Debug.Log(Describe(robot));
            }
        }

        // Each robot goes back to the spot where it was placed: the prefab root never moves.
        [MenuItem(ResetPath)]
        private static void ResetRobots()
        {
            foreach (RobotBody robot in Object.FindObjectsByType<RobotBody>())
            {
                robot.ResetPose(robot.transform.position, robot.transform.rotation);
            }
        }

        [MenuItem(LogStatePath, true)]
        [MenuItem(ResetPath, true)]
        private static bool IsPlaying()
        {
            return Application.isPlaying;
        }

        private static string Describe(RobotBody robot)
        {
            RobotState state = robot.ReadState();
            var report = new StringBuilder($"{robot.name} state (angles in degrees, torque in N·m):\n");
            report.AppendLine($"  base height {state.BaseHeight:F3} m, tilt {state.Tilt * Mathf.Rad2Deg:F1}, " +
                              $"projected gravity {state.ProjectedGravity:F2}, angular velocity {state.BaseAngularVelocity:F2} rad/s");
            report.AppendLine($"  foot contacts: {string.Join(", ", state.FootContacts)}, " +
                              $"other body contact: {state.NonFootContact}");

            for (int joint = 0; joint < robot.JointCount; joint++)
            {
                report.AppendLine($"  {robot.JointNames[joint]}: target {state.JointTargets[joint] * Mathf.Rad2Deg:F2}, " +
                                  $"angle {state.JointPositions[joint] * Mathf.Rad2Deg:F2}, " +
                                  $"velocity {state.JointVelocities[joint] * Mathf.Rad2Deg:F1}/s, " +
                                  $"torque {state.JointTorques[joint]:F2}");
            }

            return report.ToString();
        }
    }
}
