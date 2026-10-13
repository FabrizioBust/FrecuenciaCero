using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class SupplyPickup : WorldInteraction
    {
        [SerializeField] private string uniqueId;
        [SerializeField] private SupplyKind kind;
        [SerializeField] private SpriteRenderer artwork;
        [SerializeField] private float range = 1.1f;
        public SupplyKind Kind => kind;
        public string UniqueId => uniqueId;
        public override string Prompt => "[E] " + (kind == SupplyKind.Medkit ? "Recoger botiquín" : kind == SupplyKind.Battery ? "Recoger batería" : "Recoger botella");
        private void Start() => Refresh();
        private void Refresh()
        {
            if (RetenMission.Session.Survival.WasCollected(uniqueId)) gameObject.SetActive(false);
        }
        public override bool IsAvailable(InteractionProbe player) => !RetenMission.Session.Survival.WasCollected(uniqueId) &&
            player.CanReach(InteractionPoint, range, Vector2.down, range);
        public override void Interact(NarrativeDirector director)
        {
            if (director == null || director.IsOpen) return;
            if (RetenMission.Session.Survival.Collect(uniqueId, kind)) { Refresh(); return; }
            SurvivalPlayer.Notify(kind == SupplyKind.Medkit ? "No caben más botiquines. El objeto permanece aquí." : "No cabe más en el inventario.");
        }
    }
}
