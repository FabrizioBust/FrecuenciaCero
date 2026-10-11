using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(100)] // Aim after the existing camera's LateUpdate.
    [DisallowMultipleComponent, RequireComponent(typeof(Light2D))]
    public sealed class FlashlightController : MonoBehaviour
    {
        [SerializeField] private Camera worldCamera;
        [SerializeField] private bool startsOn = true;
        [SerializeField, Min(0.01f)] private float aimSmoothTime = 0.08f;

        private Light2D beam;
        private InputAction toggleAction;
        private InputAction pointerAction;
        private float angularVelocity;
        public bool IsOn { get; private set; }

        private void Awake()
        {
            beam = GetComponent<Light2D>();
            if (worldCamera == null) worldCamera = Camera.main;
            IsOn = startsOn;
            // Local actions use the project's existing Input System without editing shared bindings.
            toggleAction = new InputAction("ToggleFlashlight", InputActionType.Button, "<Keyboard>/f");
            pointerAction = new InputAction("FlashlightAim", InputActionType.Value, "<Mouse>/position",
                expectedControlType: "Vector2");
            if (worldCamera == null)
                Debug.LogError("La linterna necesita una cámara para apuntar al mouse.", this);
        }

        private void OnEnable()
        {
            toggleAction?.Enable();
            pointerAction?.Enable();
            if (beam != null) beam.enabled = IsOn;
        }

        private void Update()
        {
            if (toggleAction.WasPressedThisFrame()) SetOn(!IsOn);
        }

        private void LateUpdate()
        {
            if (worldCamera == null || pointerAction.controls.Count == 0) return;
            Vector2 screenPosition = pointerAction.ReadValue<Vector2>();
            Ray ray = worldCamera.ScreenPointToRay(screenPosition);
            Plane xyPlane = new Plane(Vector3.forward, transform.position);
            if (!xyPlane.Raycast(ray, out float distance)) return;
            Vector2 direction = ray.GetPoint(distance) - transform.position;
            if (direction.sqrMagnitude < 0.0001f) return;

            // URP's Point Light cone is centered on the local +Y axis.
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.z, targetAngle,
                ref angularVelocity, aimSmoothTime, Mathf.Infinity, Time.deltaTime);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        public void SetOn(bool on)
        {
            IsOn = on;
            if (beam != null) beam.enabled = on && isActiveAndEnabled;
        }

        private void OnDisable()
        {
            toggleAction?.Disable();
            pointerAction?.Disable();
            angularVelocity = 0f;
            if (beam != null) beam.enabled = false;
        }

        private void OnDestroy()
        {
            toggleAction?.Dispose();
            pointerAction?.Dispose();
        }
    }
}
