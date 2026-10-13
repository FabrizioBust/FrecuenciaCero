using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class CabinRoomDoor : WorldInteraction
    {
        [SerializeField] private SurvivalPlayer leo;
        [SerializeField] private string targetScene, targetSpawn, destination;
        [SerializeField] private Vector2 approachDirection = Vector2.down;
        [SerializeField] private bool requiresStorageUnlock;
        private bool busy, previousMovement;
        private System.IDisposable hiddenHud;
        public string TargetScene => targetScene;
        public string TargetSpawn => targetSpawn;
        public Vector2 ApproachDirection => approachDirection;
        public bool CanEnter => !requiresStorageUnlock || RetenMission.Session.Chapters.StorageUnlocked;
        public override string Prompt => CanEnter ? "E — " + destination : "E — Examinar el almacén cerrado";
        public override bool IsAvailable(InteractionProbe player) => !busy && !RetenTravel.IsLoading && player.Leo == leo.transform &&
            player.CanReach(InteractionPoint, 1.3f, approachDirection, .9f);
        public override void Interact(NarrativeDirector director)
        {
            if (busy || RetenTravel.IsLoading || director == null || director.IsOpen || !leo.CanAct || leo.Aiming) return;
            if (!CanEnter) { SurvivalPlayer.Notify("La puerta del almacén está cerrada."); return; }
            busy = true; previousMovement = leo.GetComponent<LeoMovement>().enabled;
            leo.Cinematic = true; leo.GetComponent<LeoMovement>().enabled = false;
            leo.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;
            hiddenHud = HudVisibility.HideForCinematic(); StartCoroutine(Travel());
        }
        private IEnumerator Travel()
        {
            var fade = GeneratorGateFade.Create(); yield return fade.Blend(0, 1, .45f);
            if (RetenTravel.Begin(targetScene, targetSpawn, leo.GetComponentInChildren<FlashlightBattery>(), leo.GetComponentInChildren<FlashlightController>()))
            { fade.FinishAfterTravel(); yield break; }
            yield return fade.Blend(1, 0, .35f); Destroy(fade.gameObject); Release();
        }
        private void Release()
        {
            hiddenHud?.Dispose(); hiddenHud = null;
            if (leo != null) { leo.Cinematic = false; leo.GetComponent<LeoMovement>().enabled = previousMovement; leo.GetComponent<LeoMovement>().IsBlocked = false; }
            busy = false;
        }
        private void OnDisable() { if (busy) Release(); }
    }
}
