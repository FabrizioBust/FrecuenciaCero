using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    // Short-lived visual overlay only. The existing RetenTravel remains the sole scene loader.
    public sealed class GeneratorGateFade : MonoBehaviour
    {
        private CanvasGroup group;
        public static GeneratorGateFade Create()
        {
            var root = new GameObject("Fundido temporal de la reja", typeof(Canvas), typeof(CanvasGroup), typeof(GeneratorGateFade));
            DontDestroyOnLoad(root);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32760;
            var fade = root.GetComponent<GeneratorGateFade>();
            fade.group = root.GetComponent<CanvasGroup>();
            fade.group.alpha = 0;
            fade.group.blocksRaycasts = false;
            var cover = new GameObject("Negro", typeof(RectTransform), typeof(Image));
            cover.transform.SetParent(root.transform, false);
            var rect = cover.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = cover.GetComponent<Image>();
            image.color = Color.black; image.raycastTarget = false;
            return fade;
        }
        public IEnumerator Blend(float from, float to, float duration)
        {
            float time = 0;
            group.alpha = from;
            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0, 1, time / duration));
                yield return null;
            }
            group.alpha = to;
        }
        public void FinishAfterTravel() => StartCoroutine(Reveal());
        private IEnumerator Reveal()
        {
            while (RetenTravel.IsLoading) yield return null;
            yield return null; // RetenArrival has restored the player and camera before revealing.
            yield return Blend(1, 0, .25f);
            Destroy(gameObject);
        }
    }
}
