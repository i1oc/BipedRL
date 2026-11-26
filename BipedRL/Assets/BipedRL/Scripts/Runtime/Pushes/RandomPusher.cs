using System;
using BipedRL.Robot;
using Unity.MLAgents;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BipedRL.Pushes
{
    /// <summary>
    /// Pushes the robot in a random horizontal direction at random intervals. During training
    /// the strength comes from the trainer, so it can grow as the robot improves; in the game it
    /// comes from the push settings. Every push is raised as an event for the HUD and the level.
    /// </summary>
    public sealed class RandomPusher : MonoBehaviour
    {
        /// <summary>ML-Agents environment parameter with the push strength, in N·s.</summary>
        public const string StrengthParameter = "push_strength";

        [SerializeField] private PushSettings settings;

        private RobotBody robot;
        private float timeUntilNextPush;

        /// <summary>Observer pattern: raised after every push.</summary>
        public event Action<PushCommand> PushApplied;

        private void Awake()
        {
            robot = GetComponentInChildren<RobotBody>();
            if (robot == null)
            {
                throw new InvalidOperationException($"{name} needs a RobotBody among its children.");
            }

            ScheduleNextPush();
        }

        private void FixedUpdate()
        {
            timeUntilNextPush -= Time.fixedDeltaTime;
            if (timeUntilNextPush > 0f)
            {
                return;
            }

            Push();
            ScheduleNextPush();
        }

        private void Push()
        {
            float strength = Academy.Instance.EnvironmentParameters.GetWithDefault(StrengthParameter, settings.DefaultStrength);

            // A training curriculum can start at zero to let the robot learn to stand first.
            if (strength <= 0f)
            {
                return;
            }

            float angle = Random.Range(0f, 2f * Mathf.PI);
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            var push = new PushCommand(robot, direction * strength);
            push.Execute();
            PushApplied?.Invoke(push);
        }

        private void ScheduleNextPush()
        {
            timeUntilNextPush = Random.Range(settings.MinInterval, settings.MaxInterval);
        }
    }
}
