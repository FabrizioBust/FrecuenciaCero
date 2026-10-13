using UnityEngine;
using UnityEngine.InputSystem;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent, RequireComponent(typeof(NarrativeDirector))]
    public sealed class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Transform leo = null;
        [SerializeField] private WorldInteraction[] targets = new WorldInteraction[0];
        private NarrativeDirector director;
        private InteractionProbe probe;
        private LeoMovement movement;
        private InputAction interact, confirm, cancel, toggleHud;
        public WorldInteraction FocusTarget { get; private set; }
        private readonly InteractionButtonGate eGate = new InteractionButtonGate();
        private readonly InteractionButtonGate enterGate = new InteractionButtonGate();
        private readonly InteractionButtonGate escapeGate = new InteractionButtonGate();
        private readonly InteractionButtonGate tabGate = new InteractionButtonGate();

        private void Awake()
        {
            director = GetComponent<NarrativeDirector>();
            if (leo == null) { Debug.LogError("Falta Leo en PlayerInteraction.", this); enabled = false; return; }
            probe = new InteractionProbe(leo);
            movement = leo.GetComponent<LeoMovement>();
            interact = new InputAction("RetenInteract", InputActionType.Button, "<Keyboard>/e");
            confirm = new InputAction("NarrativeContinue", InputActionType.Button, "<Keyboard>/enter");
            confirm.AddBinding("<Keyboard>/numpadEnter");
            cancel = new InputAction("DocumentClose", InputActionType.Button, "<Keyboard>/escape");
            toggleHud = new InputAction("ToggleHud", InputActionType.Button, "<Keyboard>/tab");
        }
        private void OnEnable()
        {
            eGate.Reset(); enterGate.Reset(); escapeGate.Reset(); tabGate.Reset();
            interact?.Enable(); confirm?.Enable(); cancel?.Enable(); toggleHud?.Enable();
        }
        private void OnDisable()
        {
            interact?.Disable(); confirm?.Disable(); cancel?.Disable(); toggleHud?.Disable();
            if (director != null) director.SetPrompt(null);
        }
        private void OnDestroy() { interact?.Dispose(); confirm?.Dispose(); cancel?.Dispose(); toggleHud?.Dispose(); }

        private void Update()
        {
            // TAB only changes drawing. Process it even while a narrative interface is open.
            if (tabGate.Sample(toggleHud.IsPressed(), toggleHud.WasPressedThisFrame())) HudVisibility.Toggle();
            // Sample all keys once. Each branch returns after consuming this frame.
            bool e = eGate.Sample(interact.IsPressed(), interact.WasPressedThisFrame());
            bool enter = enterGate.Sample(confirm.IsPressed(), confirm.WasPressedThisFrame());
            bool escape = escapeGate.Sample(cancel.IsPressed(), cancel.WasPressedThisFrame());
            director.SetPrompt(null);
            FocusTarget = null;
            if (RetenTravel.IsLoading) return;
            if (director.IsOpen)
            {
                director.HandleModalInput(e || enter, e || escape);
                director.TryStartAnomaly();
                return;
            }
            // Events have priority over doors, including the frame a document closes.
            if (director.TryStartAnomaly()) return;
            if (movement != null && (!movement.enabled || movement.IsBlocked)) return;
            WorldInteraction nearest = null;
            float nearestDistance = float.PositiveInfinity;
            foreach (WorldInteraction target in targets)
            {
                if (target == null || !target.isActiveAndEnabled || !target.IsAvailable(probe)) continue;
                float distance = (probe.Position - target.InteractionPoint).sqrMagnitude;
                if (distance >= nearestDistance) continue;
                nearestDistance = distance;
                nearest = target;
            }
            if (nearest == null) return;
            FocusTarget = nearest;
            director.SetPrompt(nearest.Prompt);
            if (!e) return;
            director.SetPrompt(null);
            nearest.Interact(director);
        }
    }
}
