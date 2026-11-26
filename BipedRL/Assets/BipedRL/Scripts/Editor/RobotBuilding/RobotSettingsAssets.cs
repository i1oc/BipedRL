using System.IO;
using BipedRL.Robot;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor.RobotBuilding
{
    /// <summary>
    /// Loads the H1 settings assets, creating them with <see cref="H1Defaults"/> when they
    /// do not exist yet. Existing assets are never overwritten, so tuned values are kept.
    /// </summary>
    public static class RobotSettingsAssets
    {
        public static RobotJointSettings LoadOrCreateJointSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RobotJointSettings>(H1AssetPaths.JointSettings);
            if (settings != null)
            {
                return settings;
            }

            settings = ScriptableObject.CreateInstance<RobotJointSettings>();
            settings.Configure(H1Defaults.PelvisHeight, H1Defaults.Joints());
            CreateAsset(settings, H1AssetPaths.JointSettings);
            return settings;
        }

        public static RobotShapeSettings LoadOrCreateShapeSettings()
        {
            var settings = AssetDatabase.LoadAssetAtPath<RobotShapeSettings>(H1AssetPaths.ShapeSettings);
            if (settings != null)
            {
                return settings;
            }

            settings = ScriptableObject.CreateInstance<RobotShapeSettings>();
            settings.Configure(H1Defaults.Shapes());
            CreateAsset(settings, H1AssetPaths.ShapeSettings);
            return settings;
        }

        private static void CreateAsset(Object asset, string assetPath)
        {
            EnsureFolder(Path.GetDirectoryName(assetPath).Replace('\\', '/'));
            AssetDatabase.CreateAsset(asset, assetPath);
            Debug.Log($"Created {assetPath} with the default H1 values.");
        }

        public static void EnsureFolder(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folderPath));
        }
    }
}
