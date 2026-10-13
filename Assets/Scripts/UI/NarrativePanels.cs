using UnityEngine;

namespace FrecuenciaCero
{
    // Presentation shared by the existing reader/director and the roots cinematic.
    // Call only from OnGUI: it owns no input, timing, mission state or movement locks.
    public static class NarrativePanels
    {
        public static GUIStyle Body { get; private set; }
        public static GUIStyle Heading { get; private set; }
        public static GUIStyle Hint { get; private set; }
        public static float Scale => Mathf.Max(.1f, Mathf.Min(Screen.width / 640f, Screen.height / 360f));
        public static void EnsureStyles()
        {
            if (Body != null) return;
            Body = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true, richText = false, padding = new RectOffset(0, 0, 0, 0) };
            Body.normal.textColor = new Color(.85f, .89f, .84f);
            Heading = new GUIStyle(Body) { fontSize = 12, fontStyle = FontStyle.Bold };
            Heading.normal.textColor = new Color(.67f, .78f, .7f);
            Hint = new GUIStyle(Body) { fontSize = 10 };
            Hint.normal.textColor = new Color(.58f, .68f, .62f);
        }
        public static void Panel(Rect area, float opacity = 1f, bool tutorial = false)
        {
            Color previous = GUI.color;
            Fill(area, new Color(.025f, .043f, .045f, .94f * opacity));
            var border = new Color(.28f, .37f, .33f, .8f * opacity);
            Fill(new Rect(area.x, area.y, area.width, 1), border);
            Fill(new Rect(area.x, area.yMax - 1, area.width, 1), border);
            Fill(new Rect(area.x, area.y, 1, area.height), border);
            Fill(new Rect(area.xMax - 1, area.y, 1, area.height), border);
            if (tutorial) Fill(new Rect(area.x + 1, area.y + 1, 3, area.height - 2), new Color(.65f, .52f, .3f, .8f * opacity));
            GUI.color = previous;
        }
        public static Rect MessageBounds(float screenWidth, float screenHeight, string title, string fullText, bool tutorial, bool hasHint)
        {
            EnsureStyles();
            float desiredWidth = tutorial ? 350f : Mathf.Clamp(Body.CalcSize(new GUIContent(fullText ?? "")).x + 32, 260, 450);
            float width = Mathf.Min(desiredWidth, screenWidth - 48f);
            float titleHeight = string.IsNullOrEmpty(title) ? 0 : Heading.CalcHeight(new GUIContent(title), width - 32) + 7;
            float height = 32 + titleHeight + Body.CalcHeight(new GUIContent(fullText ?? ""), width - 32) + (hasHint ? 23 : 0);
            height = Mathf.Min(height, screenHeight - 40);
            return new Rect((screenWidth - width) / 2, tutorial ? (screenHeight - height) / 2 : screenHeight - height - 22, width, height);
        }
        public static void Message(float width, float height, string title, string fullText, string visibleText, string hint, bool tutorial, float opacity)
        {
            EnsureStyles();
            Rect area = MessageBounds(width, height, title, fullText, tutorial, !string.IsNullOrEmpty(hint));
            Panel(area, opacity, tutorial);
            Color previous = GUI.color;
            GUI.color = new Color(1, 1, 1, opacity);
            float y = area.y + 16, innerWidth = area.width - 32;
            if (!string.IsNullOrEmpty(title))
            {
                float titleHeight = Heading.CalcHeight(new GUIContent(title), innerWidth);
                GUI.Label(new Rect(area.x + 16, y, innerWidth, titleHeight), title, Heading); y += titleHeight + 7;
            }
            float bodyHeight = Body.CalcHeight(new GUIContent(fullText ?? ""), innerWidth);
            GUI.Label(new Rect(area.x + 16, y, innerWidth, bodyHeight), visibleText ?? "", Body);
            if (!string.IsNullOrEmpty(hint)) GUI.Label(new Rect(area.x + 16, area.yMax - 26, innerWidth, 16), hint, Hint);
            GUI.color = previous;
        }
        public static Rect DocumentBounds(float width, float height, DocumentDefinition document)
        {
            EnsureStyles();
            float panelWidth = Mathf.Min(document.illustration != null ? 520f : 340f, width - 48);
            float titleHeight = Heading.CalcHeight(new GUIContent(document.title), panelWidth - 32);
            float textWidth = panelWidth - 32 - (document.illustration != null ? 164 : 0);
            float contentHeight = Body.CalcHeight(new GUIContent(document.body), textWidth);
            float maximumHeight = height - 72;
            float panelHeight = document.illustration != null ? maximumHeight : Mathf.Min(maximumHeight, 58 + titleHeight + contentHeight);
            return new Rect((width - panelWidth) / 2, (height - panelHeight) / 2, panelWidth, panelHeight);
        }
        private static void Fill(Rect area, Color color) { GUI.color = color; GUI.DrawTexture(area, Texture2D.whiteTexture); }
    }
}
