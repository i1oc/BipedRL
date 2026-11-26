using System;

namespace BipedRL.Editor.Urdf
{
    /// <summary>
    /// Thrown when a URDF document is malformed or describes an invalid robot.
    /// </summary>
    public sealed class UrdfFormatException : Exception
    {
        public UrdfFormatException(string message) : base(message)
        {
        }

        public UrdfFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
