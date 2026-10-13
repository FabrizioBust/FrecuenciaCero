using UnityEngine;
namespace FrecuenciaCero
{
    [DefaultExecutionOrder(20), RequireComponent(typeof(LeoMovement), typeof(Rigidbody2D))]
    public sealed class MovementNoise : MonoBehaviour
    {
        [SerializeField] private float walkInterval = .48f, runInterval = .28f;
        [SerializeField] private float walkRadius = 6f, runRadius = 13f;
        private LeoMovement movement;
        private Rigidbody2D body;
        private Collider2D feet;
        private Vector2 previous;
        private float next;
        public float Level { get; private set; }
        private void Awake() { movement = GetComponent<LeoMovement>(); body = GetComponent<Rigidbody2D>(); feet = GetComponent<Collider2D>(); previous = body.position; }
        private void FixedUpdate()
        {
            float displacement = (body.position - previous).magnitude;
            previous = body.position;
            bool moving = movement.enabled && !movement.IsBlocked && body.linearVelocity.sqrMagnitude > .04f && displacement > .001f && displacement < .5f;
            Level = moving ? (movement.IsRunning ? 1f : .45f) : 0f;
            if (!moving) { next = 0; return; }
            if (Time.time < next) return;
            bool running = movement.IsRunning;
            next = Time.time + (running ? runInterval : walkInterval);
            NoiseEvents.Emit(new NoiseEvent(feet.bounds.center, running ? 1f : .45f,
                running ? runRadius : walkRadius, running ? NoiseKind.Run : NoiseKind.Walk));
        }
    }
}
