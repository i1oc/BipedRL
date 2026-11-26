using System;
using System.IO;
using BipedRL.Editor.Urdf;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Editor menu that builds the H1 prefab from its URDF and settings assets.
    /// </summary>
    public static class RobotBuildMenu
    {
        [MenuItem("BipedRL/Robot/Build H1 Prefab")]
        private static void BuildH1Prefab()
        {
            GameObject robot = null;
            try
            {
                UrdfRobot urdf = UrdfParser.ParseFile(H1AssetPaths.ToFullPath(H1AssetPaths.Urdf));
                var builder = new ArticulationRobotBuilder(
                    RobotSettingsAssets.LoadOrCreateJointSettings(),
                    RobotSettingsAssets.LoadOrCreateShapeSettings());

                robot = builder.Build(urdf, "H1");
                RobotSettingsAssets.EnsureFolder(H1AssetPaths.PrefabsFolder);
                PrefabUtility.SaveAsPrefabAsset(robot, H1AssetPaths.Prefab);
                Debug.Log($"Built {H1AssetPaths.Prefab} from {H1AssetPaths.Urdf}.");
            }
            catch (Exception exception) when (exception is UrdfFormatException || exception is IOException ||
                                              exception is NotSupportedException)
            {
                Debug.LogError($"Could not build the H1 prefab: {exception.Message}");
            }
            finally
            {
                // The hierarchy is only needed to save the prefab; it must not stay in the open scene.
                if (robot != null)
                {
                    Object.DestroyImmediate(robot);
                }
            }
        }
    }
}
