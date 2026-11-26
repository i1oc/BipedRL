using System.Text;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Play mode menu that logs the target, angle and drive torque of every hinge in the
    /// scene. It is used to check the joint drives, including whether Unity applies the
    /// stiffness per radian or per degree of error.
    /// </summary>
    public static class JointStateMenu
    {
        private const string MenuPath = "BipedRL/Robot/Log Joint State (Play Mode)";

        [MenuItem(MenuPath)]
        private static void LogJointState()
        {
            var report = new StringBuilder("Joint state (angles in degrees, torque in N·m):\n");

            foreach (ArticulationBody body in Object.FindObjectsByType<ArticulationBody>())
            {
                if (body.jointType != ArticulationJointType.RevoluteJoint)
                {
                    continue;
                }

                ArticulationDrive drive = body.xDrive;
                float angle = body.jointPosition[0] * Mathf.Rad2Deg;
                float errorRadians = (drive.target - angle) * Mathf.Deg2Rad;
                float torque = body.driveForce[0];

                // With no motion, torque = stiffness * error. Comparing torque / error with the
                // configured stiffness tells whether Unity uses N·m per radian or per degree.
                string effectiveStiffness = Mathf.Abs(errorRadians) > 1e-4f
                    ? $"{torque / errorRadians:F1} N·m/rad"
                    : "error too small to measure";

                report.AppendLine($"  {body.name}: target {drive.target:F2}, angle {angle:F2}, torque {torque:F2}, " +
                                  $"configured stiffness {drive.stiffness:F1}, measured {effectiveStiffness}");
            }

            Debug.Log(report.ToString());
        }

        [MenuItem(MenuPath, true)]
        private static bool CanLogJointState()
        {
            return Application.isPlaying;
        }
    }
}
