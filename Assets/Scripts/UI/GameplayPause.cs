using UnityEngine;
using UnityEngine.InputSystem;

namespace FrecuenciaCero
{
    // One pause owner; action inputs remain gated through the resume click and its release.
    public static class GameplayPause
    {
        public static bool IsPaused { get; private set; }
        public static bool InputsBlocked
        {
            // A scene unload or disabled pause owner must not strand the release gate.
            get { UpdateReleaseGate(); return IsPaused || waitingForRelease; }
        }
        public static float PresentationTime => Time.unscaledTime - elapsedPause -
            (IsPaused ? Time.unscaledTime - startedAt : 0);
        private static bool waitingForRelease;
        private static int resumedFrame;
        private static float startedAt, elapsedPause;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            IsPaused = waitingForRelease = false;
            startedAt = elapsedPause = 0;
            resumedFrame = -1;
        }
        internal static void Begin()
        {
            startedAt = Time.unscaledTime;
            IsPaused = true;
        }
        internal static void End()
        {
            if (!IsPaused) return;
            elapsedPause += Time.unscaledTime - startedAt;
            IsPaused = false;
            waitingForRelease = true;
            resumedFrame = Time.frameCount;
        }
        internal static void UpdateReleaseGate()
        {
            if (!waitingForRelease || IsPaused || Time.frameCount <= resumedFrame) return;
            var k = Keyboard.current;
            var m = Mouse.current;
            bool held = k != null && (k.escapeKey.isPressed || k.enterKey.isPressed ||
                k.numpadEnterKey.isPressed || k.eKey.isPressed || k.hKey.isPressed ||
                k.rKey.isPressed || k.fKey.isPressed || k.qKey.isPressed || k.tabKey.isPressed);
            held |= m != null && (m.leftButton.isPressed || m.rightButton.isPressed);
            // The release frame is still blocked, including any press/release within one update.
            bool changed = k != null && (k.escapeKey.wasReleasedThisFrame || k.enterKey.wasReleasedThisFrame ||
                k.numpadEnterKey.wasReleasedThisFrame || k.eKey.wasReleasedThisFrame || k.hKey.wasReleasedThisFrame ||
                k.rKey.wasReleasedThisFrame || k.fKey.wasReleasedThisFrame || k.qKey.wasReleasedThisFrame || k.tabKey.wasReleasedThisFrame);
            changed |= m != null && (m.leftButton.wasReleasedThisFrame || m.rightButton.wasReleasedThisFrame);
            if (!held && !changed) waitingForRelease = false;
        }
    }
}
