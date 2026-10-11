using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class FusePickup : WorldInteraction
    {
        [SerializeField] private SpriteRenderer artwork = null;
        [SerializeField] private float interactionRange = .95f;
        public override string Prompt => "E: recoger fusible";
        private void OnEnable() { Refresh(); }
        private void Update() { Refresh(); }
        private void Refresh() { if (artwork != null) artwork.enabled = !RetenMission.Session.FuseCollected; }
        public override bool IsAvailable(InteractionProbe player) => RetenMission.Session.SupervisorCompleted &&
            !RetenMission.Session.FuseCollected && player.CanReach(InteractionPoint, interactionRange, Vector2.down, 1f);
        public override void Interact(NarrativeDirector director)
        {
            if (director.IsOpen) return;
            RetenMission.CollectFuse();
            Refresh();
        }
    }
}
