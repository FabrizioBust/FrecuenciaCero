using System;
using System.Collections.Generic;
using UnityEngine;

namespace FrecuenciaCero
{
    public static class MenuPreferences
    {
        public const string MusicKey = "FC.Audio.Music";
        public const string EffectsKey = "FC.Audio.Effects";
        public const string FullscreenKey = "FC.Display.Fullscreen";
        public const string WidthKey = "FC.Display.Width";
        public const string HeightKey = "FC.Display.Height";
        public static float MusicVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, .45f));
        public static float EffectsVolume => Mathf.Clamp01(PlayerPrefs.GetFloat(EffectsKey, .8f));
        public static event Action AudioChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearListeners() { AudioChanged = null; }

        public static void SetMusic(float value)
        {
            PlayerPrefs.SetFloat(MusicKey, Mathf.Clamp01(value));
            AudioChanged?.Invoke();
        }
        public static void SetEffects(float value)
        {
            PlayerPrefs.SetFloat(EffectsKey, Mathf.Clamp01(value));
            AudioChanged?.Invoke();
        }
        public static List<Vector2Int> CompatibleResolutions()
        {
            var values = new List<Vector2Int>();
            foreach (Resolution resolution in Screen.resolutions)
            {
                var size = new Vector2Int(resolution.width, resolution.height);
                if (!values.Contains(size)) values.Add(size);
            }
            // In windowed/editor environments the current window size may not be a monitor mode.
            var current = new Vector2Int(Screen.width, Screen.height);
            if (current.x > 0 && current.y > 0 && !values.Contains(current)) values.Add(current);
            values.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.y.CompareTo(b.y));
            return values;
        }
        public static void ApplyDisplay()
        {
            if (!PlayerPrefs.HasKey(FullscreenKey)) return;
            var modes = CompatibleResolutions();
            var size = new Vector2Int(PlayerPrefs.GetInt(WidthKey, Screen.width), PlayerPrefs.GetInt(HeightKey, Screen.height));
            if (!modes.Contains(size)) size = new Vector2Int(Screen.width, Screen.height);
            Screen.SetResolution(size.x, size.y, PlayerPrefs.GetInt(FullscreenKey) == 1 ?
                FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
        public static void SetDisplay(Vector2Int size, bool fullscreen)
        {
            PlayerPrefs.SetInt(WidthKey, size.x);
            PlayerPrefs.SetInt(HeightKey, size.y);
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
            Screen.SetResolution(size.x, size.y, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
    }
}
