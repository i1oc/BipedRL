using System;
using BipedRL.Agents;
using BipedRL.Editor.RobotBuilding;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor.Agents
{
    /// <summary>
    /// Editor menu that creates the milestone 1 episode configuration and one asset per
    /// reward term and termination condition. Assets that already exist are reused and never
    /// overwritten, so tuned values are kept.
    /// </summary>
    public static class StandEpisodeMenu
    {
        private const string Folder = "Assets/BipedRL/Configs/Stand";
        private const string ConfigPath = Folder + "/StandEpisode.asset";

        [MenuItem("BipedRL/Agents/Create Stand Episode Config")]
        private static void CreateStandEpisodeConfig()
        {
            if (AssetDatabase.LoadAssetAtPath<EpisodeConfig>(ConfigPath) != null)
            {
                Debug.Log($"{ConfigPath} already exists; nothing was changed.");
                return;
            }

            RobotSettingsAssets.EnsureFolder(Folder);

            // Weights from Unitree's H1 training configuration (unitree_rl_gym), except the
            // joint velocity weight, which is ours. They are starting values for training.
            RewardTerm[] terms =
            {
                LoadOrCreate<AliveTerm>(term => term.Configure(0.15f)),
                LoadOrCreate<TiltTerm>(term => term.Configure(-1f)),
                LoadOrCreate<BaseHeightTerm>(term => term.Configure(-10f)),
                LoadOrCreate<ActionRateTerm>(term => term.Configure(-0.01f)),
                LoadOrCreate<JointVelocityTerm>(term => term.Configure(-0.001f)),
                LoadOrCreate<TorqueTerm>(term => term.Configure(-0.00001f)),
                LoadOrCreate<JointLimitTerm>(term => term.Configure(-5f))
            };

            // Thresholds use the defaults declared in each condition class.
            TerminationCondition[] conditions =
            {
                LoadOrCreate<LowBaseCondition>(),
                LoadOrCreate<TiltLimitCondition>(),
                LoadOrCreate<BodyContactCondition>()
            };

            var config = ScriptableObject.CreateInstance<EpisodeConfig>();
            config.Configure(terms, conditions);
            AssetDatabase.CreateAsset(config, ConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created {ConfigPath}. Assign it to the Stand Agent of the H1StandAgent prefab.");
        }

        private static T LoadOrCreate<T>(Action<T> setDefaults = null) where T : ScriptableObject
        {
            string path = $"{Folder}/{typeof(T).Name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            setDefaults?.Invoke(asset);
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"Created {path}.");
            return asset;
        }
    }
}
