using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Editor menu that parses the H1 URDF and logs a summary, to check the parser
    /// from the editor without building the robot.
    /// </summary>
    public static class UrdfSummaryMenu
    {
        private const string H1UrdfAssetPath = "Assets/ThirdParty/Unitree/H1/h1.urdf";

        [MenuItem("BipedRL/URDF/Log H1 Summary")]
        private static void LogH1Summary()
        {
            string fullPath = Path.Combine(Path.GetDirectoryName(Application.dataPath), H1UrdfAssetPath);

            try
            {
                UrdfRobot robot = UrdfParser.ParseFile(fullPath);
                Debug.Log(BuildSummary(robot));
            }
            catch (UrdfFormatException exception)
            {
                Debug.LogError($"Could not parse {H1UrdfAssetPath}: {exception.Message}");
            }
            catch (IOException exception)
            {
                Debug.LogError($"Could not read {H1UrdfAssetPath}: {exception.Message}");
            }
        }

        private static string BuildSummary(UrdfRobot robot)
        {
            var summary = new StringBuilder();
            summary.AppendLine($"URDF '{robot.Name}': {robot.Links.Count} links, {robot.Joints.Count} joints, " +
                               $"root '{robot.RootLink.Name}', total mass {robot.TotalMass:F2} kg.");

            foreach (var group in robot.Joints.GroupBy(joint => joint.Type))
            {
                summary.AppendLine($"  {group.Key}: {group.Count()}");
            }

            // Fixed joints are skipped: in h1.urdf the torso and arm joints are fixed but keep their old limits.
            foreach (UrdfJoint joint in robot.Joints.Where(joint => joint.Type != UrdfJointType.Fixed && joint.Limit != null))
            {
                summary.AppendLine($"  {joint.Name}: [{joint.Limit.Lower:F2}, {joint.Limit.Upper:F2}] rad, " +
                                   $"{joint.Limit.Effort:F0} N·m, axis {joint.Axis}");
            }

            return summary.ToString();
        }
    }
}
