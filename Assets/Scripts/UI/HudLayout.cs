using UnityEngine;

namespace FrecuenciaCero
{
    // Only the corner HUD uses this reference. Dialogue/document scaling stays independent.
    public static class HudLayout
    {
        public const float ReferenceWidth = 1280f;
        public const float ReferenceHeight = 720f;
        public const float SizeMultiplier = 1.12f;
        public const float Margin = 16f;
        public const float BatteryWidth = 208f;
        public const float BatteryHeight = 62f;
        public const float ObjectiveWidth = 320f;
        public const float InventoryBottom = 196f;
        public static float Scale => SizeMultiplier * Mathf.Max(.75f, Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight));
        // Narrow previews place the objective below the battery instead of overlapping it.
        public static bool StackObjective(float width) => width < BatteryWidth + ObjectiveWidth + Margin * 3f;
    }
}
