using UnityEngine;

namespace BipedRL.Agents
{
    /// <summary>
    /// The stepping rhythm of Unitree's H1 training: a phase that goes from 0 to 1 every
    /// period, with the right leg half a period behind the left one. The policy observes it
    /// and the rewards ask each foot to touch the ground in the first part of its leg's cycle.
    /// </summary>
    public sealed class GaitClock
    {
        // Trained policies depend on these values, so they are constants.
        public const float Period = 0.8f;
        private const float RightLegOffset = 0.5f;

        /// <summary>Phase of the left leg, in [0, 1).</summary>
        public float Phase { get; private set; }

        /// <summary>Sets the phase for a time since the episode started, in seconds.</summary>
        public void Update(float episodeTime)
        {
            Phase = Mathf.Repeat(episodeTime, Period) / Period;
        }

        /// <summary>Phase of a leg in [0, 1); legs follow the robot's foot order, left first.</summary>
        public float LegPhase(int leg)
        {
            return leg == 0 ? Phase : Mathf.Repeat(Phase + RightLegOffset, 1f);
        }
    }
}
