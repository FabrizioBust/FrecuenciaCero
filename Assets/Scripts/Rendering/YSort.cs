using UnityEngine;

namespace FrecuenciaCero
{
    // The transform marks the feet/trunk base; the artwork extends above that point.
    [ExecuteAlways, RequireComponent(typeof(SpriteRenderer))]
    public sealed class YSort : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;

        private void OnEnable() => spriteRenderer = GetComponent<SpriteRenderer>();

        private void LateUpdate()
        {
            if (spriteRenderer != null)
                spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.y * 32f);
        }
    }
}
