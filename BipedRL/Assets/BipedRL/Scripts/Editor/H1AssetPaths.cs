using System.IO;
using UnityEngine;

namespace BipedRL.Editor
{
    /// <summary>
    /// Asset paths of the H1 robot, kept in one place.
    /// </summary>
    public static class H1AssetPaths
    {
        public const string Urdf = "Assets/ThirdParty/Unitree/H1/h1.urdf";
        public const string ConfigsFolder = "Assets/BipedRL/Configs";
        public const string JointSettings = ConfigsFolder + "/H1JointSettings.asset";
        public const string ShapeSettings = ConfigsFolder + "/H1ShapeSettings.asset";
        public const string PrefabsFolder = "Assets/BipedRL/Prefabs";
        public const string Prefab = PrefabsFolder + "/H1.prefab";

        /// <summary>Turns an "Assets/..." path into an absolute file system path.</summary>
        public static string ToFullPath(string assetPath)
        {
            return Path.Combine(Path.GetDirectoryName(Application.dataPath), assetPath);
        }
    }
}
