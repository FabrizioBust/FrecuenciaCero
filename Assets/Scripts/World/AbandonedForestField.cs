using UnityEngine;

namespace FrecuenciaCero
{
    // Visual field only. These are the existing forest trail points, not an AI navigation graph.
    public static class AbandonedForestField
    {
        private static readonly Vector2[] Trail = {
            new Vector2(-8,16), new Vector2(-8,8), new Vector2(-6,4), new Vector2(-3,1),
            new Vector2(0,-3), new Vector2(5,-6), new Vector2(11,-10)
        };
        public static float PathDistance(Vector2 point)
        {
            float best = float.MaxValue;
            for (int i = 1; i < Trail.Length; i++)
            {
                Vector2 a = Trail[i - 1], delta = Trail[i] - a;
                best = Mathf.Min(best, Vector2.Distance(point, a + delta * Mathf.Clamp01(Vector2.Dot(point - a, delta) / delta.sqrMagnitude)));
            }
            return best;
        }
        public static float FogDensity(Vector2 point)
        {
            float path = Mathf.SmoothStep(0, 1, (PathDistance(point) - .7f) / 2f);
            float camp = Mathf.Exp(-(point - new Vector2(-8,8)).sqrMagnitude / 18);
            float clearing = Mathf.Exp(-(point - new Vector2(2,-4)).sqrMagnitude / 24);
            return Mathf.Lerp(.3f, 1, path) * (1 - .45f * camp) * (1 - .35f * clearing);
        }
    }
}
