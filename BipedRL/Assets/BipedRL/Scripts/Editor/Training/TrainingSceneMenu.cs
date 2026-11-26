using System.IO;
using BipedRL.Editor.RobotBuilding;
using BipedRL.Training;
using Unity.MLAgents;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BipedRL.Editor.Training
{
    /// <summary>
    /// Editor menu that creates the milestone 1 training scene: ground, light, camera and an
    /// <see cref="AgentGridSpawner"/> with the stand agent prefab. An existing scene is never
    /// overwritten.
    /// </summary>
    public static class TrainingSceneMenu
    {
        private const string ScenesFolder = "Assets/BipedRL/Scenes/Training";
        private const string StandScenePath = ScenesFolder + "/StandTraining.unity";

        [MenuItem("BipedRL/Training/Create Stand Training Scene")]
        private static void CreateStandTrainingScene()
        {
            if (File.Exists(H1AssetPaths.ToFullPath(StandScenePath)))
            {
                Debug.Log($"{StandScenePath} already exists; nothing was changed.");
                return;
            }

            var agentPrefab = AssetDatabase.LoadAssetAtPath<Agent>(H1AssetPaths.StandAgentPrefab);
            if (agentPrefab == null)
            {
                Debug.LogError($"Could not find an agent prefab at {H1AssetPaths.StandAgentPrefab}.");
                return;
            }

            // The new scene replaces the open one, so unsaved changes must be dealt with first.
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            // A 20 m x 20 m ground holds the default 4 x 4 grid of robots 4 m apart.
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.localScale = new Vector3(2f, 1f, 2f);

            new GameObject("AgentSpawner").AddComponent<AgentGridSpawner>().Configure(agentPrefab);

            // Looks down at the whole grid from behind.
            Camera.main.transform.SetPositionAndRotation(new Vector3(0f, 12f, -14f), Quaternion.Euler(40f, 0f, 0f));

            RobotSettingsAssets.EnsureFolder(ScenesFolder);
            EditorSceneManager.SaveScene(scene, StandScenePath);
            Debug.Log($"Created {StandScenePath}.");
        }
    }
}
