using UnityEngine;

namespace FrecuenciaCero
{
    // Session-only presentation state. Scene loading never resets it or disables gameplay systems.
    public static class HudVisibility
    {
        private static bool visible = true;
        private static int cinematicScopes;
        private static int scopeGeneration;
        public static bool IsVisible { get { return visible && cinematicScopes == 0; } }
        public static void Toggle() { visible = !visible; }
        // Temporary presentation ownership; overlapping scene fades do not alter the player's TAB preference.
        public static System.IDisposable HideForCinematic() => new CinematicScope();
        private sealed class CinematicScope : System.IDisposable
        {
            private bool disposed;
            private readonly int generation;
            public CinematicScope() { generation = scopeGeneration; cinematicScopes++; }
            public void Dispose() { if (disposed) return; disposed = true; if (generation == scopeGeneration) cinematicScopes = Mathf.Max(0, cinematicScopes - 1); }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetSession() { visible = true; cinematicScopes = 0; scopeGeneration++; }
    }
}
