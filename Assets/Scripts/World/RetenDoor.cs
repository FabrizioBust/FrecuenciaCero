using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class RetenDoor : WorldInteraction
    {
        // Preserve original fields. PlayerInteraction now owns E and the contextual prompt.
        [SerializeField] private Transform leo = null;
        [SerializeField] private string targetScene = "Assets/Scenes/Nivel01/N1_InteriorReten.unity";
        [SerializeField] private string targetSpawn = "entrada";
        [SerializeField] private string prompt = "E: entrar al retén";
        [SerializeField, Min(.1f)] private float interactionRange = 1.05f;
        [SerializeField, Min(.1f)] private float approachHalfWidth = .8f;
        [SerializeField] private Vector2 approachDirection = Vector2.down;
        public override string Prompt => prompt;
        public override bool IsAvailable(InteractionProbe player) => !RetenTravel.IsLoading && leo == player.Leo &&
            player.CanReach(InteractionPoint, interactionRange, approachDirection, approachHalfWidth);

        public override void Interact(NarrativeDirector director)
        {
            if (leo == null || director == null || director.IsOpen) return;
            if (!RetenTravel.Begin(targetScene, targetSpawn, leo.GetComponentInChildren<FlashlightBattery>(),
                    leo.GetComponentInChildren<FlashlightController>())) return;
            Rigidbody2D body = leo.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
            LeoMovement movement = leo.GetComponent<LeoMovement>();
            if (movement != null) movement.enabled = false;
        }
    }
}