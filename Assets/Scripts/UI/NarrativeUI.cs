using UnityEngine;

namespace FrecuenciaCero
{
    // IMGUI matches the existing battery HUD without changing cameras, canvases or render layers.
    [DisallowMultipleComponent]
    public sealed class NarrativeUI : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float objectiveNotificationSeconds = 3f;
        private NarrativeDirector director;
        private GUIStyle body, heading, small, centered, objectiveBody, objectiveHeader;
        private Vector2 documentScroll;
        private int objectiveRevision;
        private float notificationUntil;
        private float dialogueOpacity, documentOpacity;
        private DocumentDefinition shownDocument;
        private string shownSpeaker, shownText, shownFullText, shownHint;
        private static readonly Color Panel = new Color(.025f, .043f, .045f, .93f);
        private static readonly Color Border = new Color(.28f, .37f, .33f, .8f);
        private static readonly Color Text = new Color(.85f, .89f, .84f);

        private void Awake()
        {
            director = GetComponent<NarrativeDirector>();
            objectiveRevision = RetenMission.Session.ObjectiveRevision;
        }
        private void Update()
        {
            bool dialogueOpen = director != null && director.CurrentLine != null;
            dialogueOpacity = Mathf.MoveTowards(dialogueOpacity, dialogueOpen ? 1 : 0, Time.unscaledDeltaTime / .18f);
            documentOpacity = Mathf.MoveTowards(documentOpacity, director != null && director.Document != null ? 1 : 0, Time.unscaledDeltaTime / .18f);
            if (dialogueOpen)
            {
                shownSpeaker = director.CurrentLine.speaker; shownFullText = director.CurrentLine.text;
                shownText = director.VisibleText;
                shownHint = director.IsLineComplete ? "E / Enter: continuar" : "E / Enter: mostrar texto completo";
            }
            if (director != null && director.Document != null) shownDocument = director.Document;
            else if (documentOpacity == 0) shownDocument = null;
            int revision = RetenMission.Session.ObjectiveRevision;
            if (objectiveRevision == revision) return;
            objectiveRevision = revision;
            notificationUntil = Time.unscaledTime + objectiveNotificationSeconds;
        }
        public void ResetDocumentScroll() { documentScroll = Vector2.zero; }

        private void OnGUI()
        {
            if (director == null) return;
            EnsureStyles();
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;
            int oldDepth = GUI.depth;
            // Dialogue, documents and contextual prompts keep their established 640x360 scale.
            float scale = NarrativePanels.Scale;
            float width = Screen.width / scale, height = Screen.height / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            GUI.depth = -20;
            try
            {
                if (HudVisibility.IsVisible) { DrawObjective(); DrawFuseNotice(); }
                if (shownDocument != null && documentOpacity > 0) DrawDocument(width, height, shownDocument);
                else if (dialogueOpacity > 0) DrawDialogue(width, height);
                else if (!string.IsNullOrEmpty(director.ContextPrompt))
                {
                    Rect prompt = new Rect(width / 2 - 130, height - 48, 260, 30);
                    DrawPanel(prompt);
                    GUI.Label(prompt, director.ContextPrompt, centered);
                }
            }
            finally { GUI.matrix = oldMatrix; GUI.color = oldColor; GUI.depth = oldDepth; }
        }

        private void DrawObjective()
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = HudLayout.Scale;
            float width = Screen.width / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            try
            {
                float panelWidth = Mathf.Min(HudLayout.ObjectiveWidth, width - HudLayout.Margin * 2f);
                float textWidth = panelWidth - 24f;
                float textHeight = objectiveBody.CalcHeight(new GUIContent(RetenMission.Session.CurrentObjective), textWidth);
                float panelHeight = Mathf.Max(52f, textHeight + 34f);
                float y = HudLayout.StackObjective(width) ? HudLayout.InventoryBottom : HudLayout.Margin;
                Rect panel = new Rect(width - panelWidth - HudLayout.Margin, y, panelWidth, panelHeight);
                DrawPanel(panel);
                GUI.Label(new Rect(panel.x + 12, panel.y + 7, textWidth, 15),
                    Time.unscaledTime < notificationUntil ? "OBJETIVO ACTUALIZADO" : "OBJETIVO", objectiveHeader);
                GUI.Label(new Rect(panel.x + 12, panel.y + 26, textWidth, textHeight), RetenMission.Session.CurrentObjective, objectiveBody);
            }
            finally { GUI.matrix = previousMatrix; }
        }
        private void DrawDialogue(float width, float height)
        {
            NarrativePanels.Message(width, height, shownSpeaker, shownFullText, shownText, shownHint, false, dialogueOpacity);
        }
        private void DrawFuseNotice()
        {
            if (Time.unscaledTime >= RetenMission.FuseNoticeUntil) return;
            Matrix4x4 previous = GUI.matrix;
            float scale = HudLayout.Scale;
            float width = Screen.width / scale;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            try
            {
                float y = HudLayout.StackObjective(width) ? HudLayout.InventoryBottom + 86f : HudLayout.InventoryBottom;
                Rect panel = new Rect(HudLayout.Margin, y, 276f, 36f);
                DrawPanel(panel);
                GUI.Label(new Rect(panel.x + 10, panel.y + 8, panel.width - 20, 22), "Fusible de repuesto recogido", objectiveBody);
            }
            finally { GUI.matrix = previous; }
        }
        private void DrawDocument(float width, float height, DocumentDefinition document)
        {
            NarrativePanels.EnsureStyles();
            Rect panel = NarrativePanels.DocumentBounds(width, height, document);
            NarrativePanels.Panel(panel, documentOpacity);
            GUI.color = new Color(1, 1, 1, documentOpacity);
            float titleHeight = NarrativePanels.Heading.CalcHeight(new GUIContent(document.title), panel.width - 32);
            GUI.Label(new Rect(panel.x + 16, panel.y + 14, panel.width - 32, titleHeight), document.title, NarrativePanels.Heading);
            float contentY = panel.y + 23 + titleHeight;
            float textX = panel.x + 16, textWidth = panel.width - 32;
            if (document.illustration != null)
            {
                Sprite sprite = document.illustration;
                Rect area = new Rect(textX, contentY, 150, panel.yMax - 35 - contentY);
                Rect pixels = sprite.textureRect;
                float imageHeight = Mathf.Min(area.height, area.width * pixels.height / pixels.width);
                float imageWidth = imageHeight * pixels.width / pixels.height;
                GUI.DrawTextureWithTexCoords(new Rect(area.x, area.y, imageWidth, imageHeight), sprite.texture,
                    new Rect(pixels.x / sprite.texture.width, pixels.y / sprite.texture.height,
                        pixels.width / sprite.texture.width, pixels.height / sprite.texture.height));
                textX += 164; textWidth -= 164;
            }
            Rect viewport = new Rect(textX, contentY, textWidth, panel.yMax - 35 - contentY);
            float textHeight = NarrativePanels.Body.CalcHeight(new GUIContent(document.body), textWidth);
            bool scroll = textHeight > viewport.height + 1;
            if (scroll)
            {
                textWidth -= 18;
                textHeight = NarrativePanels.Body.CalcHeight(new GUIContent(document.body), textWidth);
                documentScroll = GUI.BeginScrollView(viewport, documentScroll, new Rect(0, 0, textWidth, textHeight));
                GUI.Label(new Rect(0, 0, textWidth, textHeight), document.body, NarrativePanels.Body);
                GUI.EndScrollView();
            }
            else GUI.Label(viewport, document.body, NarrativePanels.Body);
            GUI.Label(new Rect(panel.x + 16, panel.yMax - 27, panel.width - 32, 20),
                scroll ? "E / Escape: cerrar   ·   Rueda del ratón: leer" : "E / Escape: cerrar", NarrativePanels.Hint);
            GUI.color = Color.white;
        }
        private void EnsureStyles()
        {
            if (body != null) return;
            body = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true, richText = false, padding = new RectOffset(0, 0, 0, 0) };
            body.normal.textColor = Text;
            heading = new GUIStyle(body) { fontSize = 16, fontStyle = FontStyle.Bold };
            heading.normal.textColor = new Color(.67f, .78f, .7f);
            small = new GUIStyle(body) { fontSize = 12 };
            small.normal.textColor = new Color(.58f, .68f, .62f);
            centered = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter };
            objectiveBody = new GUIStyle(body) { fontSize = 14 };
            objectiveHeader = new GUIStyle(small) { fontSize = 11 };
        }
        private static void DrawPanel(Rect rect)
        {
            GUI.color = Panel; GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = Border;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.yMax - 1, rect.width, 1), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, 1, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 1, rect.y, 1, rect.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }
    }
}
