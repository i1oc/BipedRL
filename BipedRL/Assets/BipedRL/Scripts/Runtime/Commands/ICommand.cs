namespace BipedRL.Commands
{
    /// <summary>
    /// Command pattern: an action packed as an object with everything it needs, so whoever
    /// triggers it (the random perturbator, the player's input) does not know how it is done.
    /// </summary>
    public interface ICommand
    {
        void Execute();
    }
}
