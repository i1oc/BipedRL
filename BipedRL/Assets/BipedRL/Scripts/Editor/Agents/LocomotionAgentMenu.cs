using System;
using BipedRL.Agents;
using BipedRL.Editor.RobotBuilding;
using BipedRL.Robot;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEditor;
using UnityEngine;

namespace BipedRL.Editor.Agents
{
    /// <summary>
    /// Editor menu that creates the locomotion agent prefab around the H1 prefab, with its
    /// episode configuration and the assets it uses. Assets that already exist are reused and
    /// never overwritten, so tuned values are kept.
    /// </summary>
    public static class LocomotionAgentMenu
    {
        private const string BehaviorName = "Locomotion";

        private const string Folder = H1AssetPaths.ConfigsFolder + "/Locomotion";
        private const string EpisodeConfigPath = Folder + "/LocomotionEpisode.asset";

        // Unitree's episodes last 20 s and its policy runs at 50 Hz.
        private const float EpisodeSeconds = 20f;
        private const float PolicyPeriod = 0.02f;

        [MenuItem("BipedRL/Agents/Create Locomotion Agent")]
        private static void CreateLocomotionAgent()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(H1AssetPaths.LocomotionAgentPrefab) != null)
            {
                Debug.Log($"{H1AssetPaths.LocomotionAgentPrefab} already exists; nothing was changed.");
                return;
            }

            var robotPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(H1AssetPaths.Prefab);
            if (robotPrefab == null)
            {
                Debug.LogError($"Could not find the robot prefab at {H1AssetPaths.Prefab}. Build it first.");
                return;
            }

            RobotSettingsAssets.EnsureFolder(Folder);
            EpisodeConfig episode = LoadOrCreateEpisodeConfig();
            var ranges = LoadOrCreate<VelocityTargetRanges>();

            var root = new GameObject("H1LocomotionAgent");
            try
            {
                var robot = (GameObject)PrefabUtility.InstantiatePrefab(robotPrefab, root.transform);
                int jointCount = robot.GetComponent<RobotBody>().JointCount;

                // The agent first: the Decision Requester requires an Agent on the same object.
                var agent = root.AddComponent<LocomotionAgent>();
                agent.Configure(episode);
                agent.MaxStep = Mathf.RoundToInt(EpisodeSeconds / Time.fixedDeltaTime);

                var behavior = root.GetComponent<BehaviorParameters>();
                behavior.BehaviorName = BehaviorName;
                behavior.BrainParameters.VectorObservationSize = LocomotionAgent.ObservationSize(jointCount);
                behavior.BrainParameters.ActionSpec = ActionSpec.MakeContinuous(jointCount);

                var requester = root.AddComponent<DecisionRequester>();
                requester.DecisionPeriod = Mathf.RoundToInt(PolicyPeriod / Time.fixedDeltaTime);
                requester.TakeActionsBetweenDecisions = false;

                root.AddComponent<RandomVelocityTargets>().Configure(ranges);

                PrefabUtility.SaveAsPrefabAsset(root, H1AssetPaths.LocomotionAgentPrefab);
                Debug.Log($"Created {H1AssetPaths.LocomotionAgentPrefab}.");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        // Reward terms are added by the locomotion rewards; until then only the episode end is set.
        private static EpisodeConfig LoadOrCreateEpisodeConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<EpisodeConfig>(EpisodeConfigPath);
            if (config != null)
            {
                return config;
            }

            TerminationCondition[] conditions =
            {
                LoadOrCreate<PelvisContactCondition>(),
                LoadOrCreate<LeanLimitCondition>()
            };

            config = ScriptableObject.CreateInstance<EpisodeConfig>();
            config.Configure(Array.Empty<RewardTerm>(), conditions);
            AssetDatabase.CreateAsset(config, EpisodeConfigPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created {EpisodeConfigPath}.");
            return config;
        }

        // Values use the defaults declared in each class.
        private static T LoadOrCreate<T>() where T : ScriptableObject
        {
            string path = $"{Folder}/{typeof(T).Name}.asset";
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
            {
                return asset;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"Created {path}.");
            return asset;
        }
    }
}
