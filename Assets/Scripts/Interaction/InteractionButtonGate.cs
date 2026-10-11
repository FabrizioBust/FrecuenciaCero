namespace FrecuenciaCero
{
    // Holding E across scene loading cannot activate the new scene's door.
    public sealed class InteractionButtonGate
    {
        private bool released;
        public bool Sample(bool held, bool pressedThisFrame)
        {
            if (!held) released = true;
            if (!released || !pressedThisFrame) return false;
            released = false;
            return true;
        }
        public void Reset() { released = false; }
    }
}
