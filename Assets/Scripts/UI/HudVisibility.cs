using UnityEngine;

namespace FrecuenciaCero
{
    // Session-only presentation state. Scene loading never resets it or disables gameplay systems.
    public static class HudVisibility
    {
        private static bool visible = true;
        public static bool IsVisible { get { return visible; } }
        public static void Toggle() { visible = !visible; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSession() { visible = true; }
    }
}
