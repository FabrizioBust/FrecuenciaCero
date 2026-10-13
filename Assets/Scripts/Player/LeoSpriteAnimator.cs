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
        [Tooltip("Atlas original: el primer fotograma lateral mira a la izquierda y los demás a la derecha.")]
        [SerializeField] private bool[] rightFlipX = { true, false, false, false };
        [SerializeField] private bool[] leftFlipX = { false, true, true, true };
        [SerializeField, Min(1f)] private float framesPerSecond = 6f;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        private Sprite[] facing;
        private float elapsed;
        // Cinematic facing changes only the visual pose, never controls, speed or flashlight aim.
        public void Face(Vector2 direction)
        {
            if (direction.sqrMagnitude < .001f) return;
            facing = Mathf.Abs(direction.x) > Mathf.Abs(direction.y) ? (direction.x > 0 ? right : left) : (direction.y > 0 ? up : down);
            elapsed = 0;
        }

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
                float horizontal = Mathf.Abs(velocity.x), vertical = Mathf.Abs(velocity.y);
                // Keep the previous axis for a diagonal tie instead of changing it with float noise.
                bool horizontalFacing = facing == right || facing == left;
                bool horizontalMovement = Mathf.Abs(horizontal - vertical) <= .01f ? horizontalFacing : horizontal > vertical;
                Sprite[] next = horizontalMovement ?
                    (velocity.x > 0f ? right : left) : (velocity.y > 0f ? up : down);
                if (next != facing) { facing = next; elapsed = 0f; }
                elapsed += Time.deltaTime;
            }
            else elapsed = 0f;
            if (facing == null || facing.Length == 0) return;
            int frame = moving ? Mathf.FloorToInt(elapsed * framesPerSecond) % facing.Length : 0;
            if (facing[frame] != null)
            {
                spriteRenderer.sprite = facing[frame];
                bool[] flips = facing == right ? rightFlipX : facing == left ? leftFlipX : null;
                spriteRenderer.flipX = flips != null && frame < flips.Length && flips[frame];
            }
        }
    }
}
