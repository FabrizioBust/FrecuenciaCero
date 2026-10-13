using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    // Short-lived visual overlay, following GeneratorGateFade. RetenTravel remains the only loader.
    public sealed class ChapterTransitionOverlay : MonoBehaviour
    {
        private CanvasGroup blackout, words;
        public static ChapterTransitionOverlay Create(string title, string subtitle)
        {
            var root = new GameObject("Transición de capítulo", typeof(Canvas), typeof(CanvasScaler), typeof(ChapterTransitionOverlay));
            DontDestroyOnLoad(root); var canvas = root.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 32760;
            var scaler = root.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = .5f;
            var view = root.GetComponent<ChapterTransitionOverlay>();
            var black = new GameObject("Negro", typeof(RectTransform), typeof(Image), typeof(CanvasGroup)); black.transform.SetParent(root.transform, false);
            Stretch(black.GetComponent<RectTransform>()); black.GetComponent<Image>().color = Color.black; black.GetComponent<Image>().raycastTarget = false;
            view.blackout = black.GetComponent<CanvasGroup>(); view.blackout.alpha = 0;
            var words = new GameObject("Rótulo", typeof(RectTransform), typeof(CanvasGroup)); words.transform.SetParent(black.transform, false); Stretch(words.GetComponent<RectTransform>());
            view.words = words.GetComponent<CanvasGroup>(); view.words.alpha = 0;
            Label(words.transform, title, 52, 50); Label(words.transform, subtitle, 29, -38);
            return view;
        }
        private static void Stretch(RectTransform r) { r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = r.offsetMax = Vector2.zero; }
        private static void Label(Transform parent, string content, int size, float y)
        {
            var obj = new GameObject(content, typeof(RectTransform), typeof(Text)); obj.transform.SetParent(parent, false);
            var r = obj.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = r.pivot = new Vector2(.5f,.5f); r.anchoredPosition = new Vector2(0,y); r.sizeDelta = new Vector2(1200, 100);
            var text = obj.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.text = content; text.alignment = TextAnchor.MiddleCenter; text.color = new Color(.85f,.89f,.88f); text.raycastTarget = false;
        }
        private static IEnumerator Blend(CanvasGroup group, float to, float seconds)
        {
            float from = group.alpha;
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime) { group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0,1,t/seconds)); yield return null; }
            group.alpha = to;
        }
        public IEnumerator Cover() { yield return Blend(blackout, 1, .7f); }
        public IEnumerator ShowTitle() { yield return Blend(words, 1, .55f); yield return new WaitForSecondsRealtime(1.65f); yield return Blend(words, 0, .4f); }
        public IEnumerator Reveal() { yield return Blend(words, 0, .15f); yield return Blend(blackout, 0, .9f); Destroy(gameObject); }
    }
}
