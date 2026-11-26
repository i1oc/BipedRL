using UnityEngine;

namespace BipedRL.Robot
{
    /// <summary>
    /// Reports whether a robot body touches the environment. On the feet it plays the role
    /// of the real robot's foot sensors; on the other bodies it detects falls.
    /// </summary>
    [RequireComponent(typeof(ArticulationBody))]
    public sealed class ContactSensor : MonoBehaviour
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
