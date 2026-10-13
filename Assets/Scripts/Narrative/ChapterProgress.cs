namespace FrecuenciaCero
{
    // Chapter continuity belongs to the same mission session as health, inventory and collected IDs.
    public sealed class ChapterProgress
    {
        public bool ClosingStarted { get; private set; }
        public bool TwoStarted { get; private set; }
        public bool ArrivalFinished { get; private set; }
        public bool RadioCompleted { get; private set; }
        public bool WindowExtinguished { get; private set; }
        public bool DoorConversationCompleted { get; private set; }
        public bool CabinEntered { get; private set; }
        public bool StorageUnlocked { get; private set; }
        public bool BeginClosing() { if (ClosingStarted) return false; ClosingStarted = true; return true; }
        public bool StartTwo() { if (TwoStarted) return false; TwoStarted = true; return true; }
        public void FinishArrival() => ArrivalFinished = true;
        public void FinishRadio() => RadioCompleted = true;
        public void ExtinguishWindow() => WindowExtinguished = true;
        public void FinishDoorConversation() => DoorConversationCompleted = true;
        public void EnterCabin() => CabinEntered = true;
        // A future chapter event may unlock storage; room travel never resets this session flag.
        public void UnlockStorage() => StorageUnlocked = true;
    }
}
