using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Foot sensor: reports whether a foot body touches the environment, like the foot
    /// contact sensors of the real robot.
    /// </summary>
    [RequireComponent(typeof(ArticulationBody))]
    public sealed class FootContact : MonoBehaviour
    {
        private int environmentContacts;

        public bool IsTouching => environmentContacts > 0;

        private void OnCollisionEnter(Collision collision)
        {
            if (IsEnvironment(collision))
            {
                environmentContacts++;
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (IsEnvironment(collision))
            {
                environmentContacts = Mathf.Max(0, environmentContacts - 1);
            }
        }

        // Other robot parts, such as the other foot, are not ground. Robots never touch
        // each other because each one lives on a layer that ignores the others.
        private static bool IsEnvironment(Collision collision)
        {
            return collision.articulationBody == null;
        }
    }
}
