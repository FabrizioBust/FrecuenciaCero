using UnityEngine;
using UnityEngine.InputSystem;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class LeoMovement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 3f;
        [SerializeField] private InputActionAsset inputActions;
        [SerializeField, Min(1f)] private float runMultiplier = 1.65f;
        private Rigidbody2D body;
        private InputAction move;
        private Vector2 direction;

        public float Speed => speed;
        public bool IsRunning { get; private set; }
        public bool IsBlocked { get; set; }

        public void Configure(InputActionAsset actions) => inputActions = actions;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            // Own a copy so enabling/disabling Leo never changes the project's shared actions.
            move = inputActions != null ? inputActions.FindAction("Player/Move", true).Clone() : null;
            if (move == null)
            {
                Debug.LogError("Leo necesita el asset Input System con Player/Move.", this);
                enabled = false;
            }
        }

        private void OnEnable() => move?.Enable();

        private void Update()
        {
            var keyboard = Keyboard.current;
            IsRunning = !IsBlocked && keyboard != null && (keyboard.leftShiftKey.isPressed || keyboard.rightShiftKey.isPressed);
            direction = IsBlocked ? Vector2.zero : Vector2.ClampMagnitude(move.ReadValue<Vector2>(), 1f);
        }

        private void FixedUpdate() => body.linearVelocity = IsBlocked ? Vector2.zero : direction * speed * (IsRunning ? runMultiplier : 1f);

        private void OnDisable()
        {
            move?.Disable();
            direction = Vector2.zero;
            IsRunning = false;
            if (body != null) body.linearVelocity = Vector2.zero;
        }

        private void OnDestroy() => move?.Dispose();
    }
}
