using System;

namespace FrecuenciaCero
{
    // Pure battery arithmetic, independent of rendering and Unity's frame lifecycle.
    public static class BatteryCharge
    {
        public static float Drain(float percent, float seconds, float fullDuration, bool isOn)
        {
            percent = Math.Clamp(percent, 0f, 100f);
            if (!isOn) return percent;
            return Math.Max(0f, percent - 100f * Math.Max(0f, seconds) / Math.Max(0.1f, fullDuration));
        }

        public static float Interpolate(float minimum, float maximum, float percent)
        {
            return minimum + (maximum - minimum) * Math.Clamp(percent / 100f, 0f, 1f);
        }
    }
}
