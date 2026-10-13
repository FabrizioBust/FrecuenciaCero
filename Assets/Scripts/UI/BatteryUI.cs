using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class BatteryUI : MonoBehaviour
    {
        [SerializeField] private FlashlightBattery battery = null;
        [SerializeField] private FlashlightController flashlight = null;
        private GUIStyle labelStyle, percentStyle, hintStyle, keyStyle;

        private void OnGUI()
        {
            if (!HudVisibility.IsVisible) return;
            if (battery == null || flashlight == null) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13, alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset(0, 0, 0, 0), richText = false
                };
                labelStyle.normal.textColor = new Color(.85f, .89f, .84f);
                percentStyle = new GUIStyle(labelStyle) { alignment = TextAnchor.MiddleRight };
                hintStyle = new GUIStyle(labelStyle) { fontSize = 12 };
                hintStyle.normal.textColor = new Color(.65f, .73f, .67f);
                keyStyle = new GUIStyle(hintStyle) { fontSize = 11, alignment = TextAnchor.MiddleCenter };
            }
            Matrix4x4 previousMatrix = GUI.matrix;
            Color previousColor = GUI.color;
            int previousDepth = GUI.depth;
            float scale = HudLayout.Scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            GUI.depth = -20;
            try
            {
                float x = HudLayout.Margin, y = HudLayout.Margin + 36f;
                float barWidth = HudLayout.BatteryWidth - 24f;
                DrawRect(new Rect(x, y, HudLayout.BatteryWidth, HudLayout.BatteryHeight), new Color(.025f, .043f, .045f, .9f));
                DrawRect(new Rect(x, y, HudLayout.BatteryWidth, 1f), new Color(.28f, .37f, .33f, .8f));
                GUI.color = Color.white;
                GUI.Label(new Rect(x + 12, y + 7, 110, 18), "BATERÍA", labelStyle);
                GUI.Label(new Rect(x + 132, y + 7, 64, 18), $"{Mathf.CeilToInt(battery.Percent)}%", percentStyle);
                DrawRect(new Rect(x + 12, y + 30, barWidth, 5), new Color(.16f, .21f, .2f));
                DrawRect(new Rect(x + 12, y + 30, barWidth * battery.NormalizedCharge, 5),
                    Color.Lerp(new Color(.7f, .35f, .23f), new Color(.56f, .7f, .57f), battery.NormalizedCharge));
                DrawRect(new Rect(x + 12, y + 40, 18, 16), new Color(.15f, .21f, .18f));
                GUI.color = Color.white;
                GUI.Label(new Rect(x + 12, y + 40, 18, 16), "F", keyStyle);
                GUI.Label(new Rect(x + 38, y + 39, 158, 18), flashlight.IsOn ? "apagar linterna" : "encender linterna", hintStyle);
            }
            finally
            {
                GUI.color = previousColor;
                GUI.matrix = previousMatrix;
                GUI.depth = previousDepth;
            }
        }

        private static void DrawRect(Rect rectangle, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rectangle, Texture2D.whiteTexture);
        }
    }
}
