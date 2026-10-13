using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class ForestPassage : WorldInteraction
    {
        [SerializeField] private Transform leo;
        [SerializeField] private string destination = "Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity";
        [SerializeField] private string arrival = "bosque_entrada";
        [SerializeField] private bool requiresGenerator = true;
        [SerializeField] private Vector2 approachDirection = Vector2.up;
        [SerializeField] private float range = 1.8f;
        private bool departing;
        public override string Prompt => requiresGenerator && !RetenMission.Session.ForestAvailable ? "[E] Examinar sendero" : "[E] Continuar por el sendero";
        public override bool IsAvailable(InteractionProbe player) => !departing && !RetenTravel.IsLoading && player.CanReach(InteractionPoint, range, approachDirection, range);
        public override void Interact(NarrativeDirector director)
        {
            if (departing || director == null || director.IsOpen) return;
            if (requiresGenerator && !RetenMission.Session.ForestAvailable)
            { SurvivalPlayer.Notify("Primero debo reparar el generador y comprobar qué ocurrió."); return; }
            departing = true; StartCoroutine(Travel());
        }
        private IEnumerator Travel()
        {
            var movement = leo.GetComponent<LeoMovement>(); movement.enabled = false;
            var fade = GeneratorGateFade.Create(); yield return fade.Blend(0, 1, .6f);
            if (RetenTravel.Begin(destination, arrival, leo.GetComponentInChildren<FlashlightBattery>(), leo.GetComponentInChildren<FlashlightController>()))
            { fade.FinishAfterTravel(); yield break; }
            yield return fade.Blend(1, 0, .3f); Destroy(fade.gameObject); movement.enabled = true; departing = false;
        }
    }
}
