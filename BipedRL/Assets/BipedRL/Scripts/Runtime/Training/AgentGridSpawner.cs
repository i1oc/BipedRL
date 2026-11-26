using Unity.MLAgents;
using UnityEngine;

namespace BipedRL.Training
{
    /// <summary>
    /// Creates copies of an agent prefab in a grid centred on this object, so a training
    /// scene only holds the ground and this spawner, and the number of robots is a setting.
    /// </summary>
    public sealed class AgentGridSpawner : MonoBehaviour
    {
        [SerializeField] private Agent agentPrefab;

        [Min(1)]
        [SerializeField] private int rows = 4;

        [Min(1)]
        [SerializeField] private int columns = 4;

        // A robot falls less than 1.5 m before its episode ends, so 4 m keeps neighbours
        // from ever touching without needing a physics layer per robot.
        [Tooltip("Distance between neighbouring robots, in metres.")]
        [Min(1f)]
        [SerializeField] private float spacing = 4f;

        /// <summary>Used by editor tools that create training scenes.</summary>
        public void Configure(Agent prefab)
        {
            agentPrefab = prefab;
        }

        private void Awake()
        {
            Vector3 firstPosition = transform.position -
                                    new Vector3((columns - 1) * spacing / 2f, 0f, (rows - 1) * spacing / 2f);

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Vector3 position = firstPosition + new Vector3(column * spacing, 0f, row * spacing);
                    Agent agent = Instantiate(agentPrefab, position, transform.rotation, transform);
                    agent.name = $"{agentPrefab.name} {row}-{column}";
                }
            }
        }
    }
}
