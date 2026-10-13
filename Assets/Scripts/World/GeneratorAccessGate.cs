using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    // PlayerInteraction owns E and the prompt. RetenTravel owns scene loading and arrival state.
    [DisallowMultipleComponent]
    public sealed class GeneratorAccessGate : WorldInteraction
    {
        [SerializeField] private Transform leo;
        [SerializeField] private Transform gateLeaf;
        [SerializeField] private string targetScene = "Assets/Scenes/Nivel01/N1_Generador.unity";
        [SerializeField] private string targetSpawn = "generador_entrada";
        [SerializeField, Min(.1f)] private float interactionRange = 1.65f;
        [SerializeField] private Vector2 approachDirection = new Vector2(-1, -.65f);
        [SerializeField, Min(.1f)] private float approachHalfWidth = 1.1f;
        private bool opening;
        public override string Prompt => "[E] Abrir reja";
        public bool IsOpening => opening;
        public override bool IsAvailable(InteractionProbe player) => !opening && !RetenTravel.IsLoading &&
            leo != null && player.Leo == leo &&
            player.CanReach(InteractionPoint, interactionRange, approachDirection, approachHalfWidth);

        public override void Interact(NarrativeDirector director)
        {
            if (director == null || director.IsOpen || leo == null || !IsAvailable(new InteractionProbe(leo))) return;
            // Reserve this gate immediately, including the animation before RetenTravel becomes busy.
            opening = true;
            StartCoroutine(Depart());
        }

        private IEnumerator Depart()
        {
            var movement = leo.GetComponent<LeoMovement>();
            bool restoreMovement = movement != null && movement.enabled;
            if (movement != null) movement.enabled = false;
            var body = leo.GetComponent<Rigidbody2D>();
            if (body != null) body.linearVelocity = Vector2.zero;
            Vector3 closedScale = gateLeaf != null ? gateLeaf.localScale : Vector3.one;
            float elapsed = 0;
            while (elapsed < .32f)
            {
                elapsed += Time.unscaledDeltaTime;
                if (gateLeaf != null)
                {
                    // Simple projected swing around the hinge; the original fence sprite is preserved.
                    float swing = Mathf.SmoothStep(0, 1, elapsed / .32f);
                    gateLeaf.localScale = new Vector3(closedScale.x * Mathf.Lerp(1, .14f, swing), closedScale.y, closedScale.z);
                }
                yield return null;
            }
            var fade = GeneratorGateFade.Create();
            yield return fade.Blend(0, 1, .22f);
            if (RetenTravel.Begin(targetScene, targetSpawn, leo.GetComponentInChildren<FlashlightBattery>(),
                leo.GetComponentInChildren<FlashlightController>()))
            {
                fade.FinishAfterTravel();
                yield break;
            }
            // Failed or unavailable destinations must not leave movement or the screen locked.
            yield return fade.Blend(1, 0, .18f);
            Destroy(fade.gameObject);
            if (gateLeaf != null) gateLeaf.localScale = closedScale;
            if (movement != null) movement.enabled = restoreMovement;
            opening = false;
        }
    }
}
