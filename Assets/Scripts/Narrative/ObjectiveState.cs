namespace FrecuenciaCero
{
    // Reusable session objective record. Future missions can use their own keys and descriptions.
    public sealed class ObjectiveState
    {
        public string Key { get; private set; }
        public string Description { get; private set; }
        public int Revision { get; private set; }
        public ObjectiveState(string key, string description) { Key = key; Description = description; }
        public void Set(string key, string description)
        {
            if (Key == key && Description == description) return;
            Key = key;
            Description = description;
            Revision++;
        }
    }
}
