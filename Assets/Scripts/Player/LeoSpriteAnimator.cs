using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent, RequireComponent(typeof(SpriteRenderer), typeof(Rigidbody2D))]
    public sealed class LeoSpriteAnimator : MonoBehaviour
    {
        [SerializeField] private Sprite[] down = null;
        [SerializeField] private Sprite[] up = null;
        [SerializeField] private Sprite[] right = null;
        [SerializeField] private Sprite[] left = null;
        [SerializeField, Min(1f)] private float framesPerSecond = 6f;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Sprite[] facing;
        private float elapsed;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            facing = down;
        }

        private void LateUpdate()
        {
            Vector2 velocity = body.linearVelocity;
            bool moving = velocity.sqrMagnitude > 0.01f;
            if (moving)
            {
                Sprite[] next = Mathf.Abs(velocity.x) > Mathf.Abs(velocity.y) ?
                    (velocity.x > 0f ? right : left) : (velocity.y > 0f ? up : down);
                if (next != facing) { facing = next; elapsed = 0f; }
                elapsed += Time.deltaTime;
            }
            else elapsed = 0f;
            if (facing == null || facing.Length == 0) return;
            int frame = moving ? Mathf.FloorToInt(elapsed * framesPerSecond) % facing.Length : 0;
            if (facing[frame] != null) spriteRenderer.sprite = facing[frame];
        }
    }
}
