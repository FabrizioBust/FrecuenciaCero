using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class FusePickup : WorldInteraction
    {
        [SerializeField] private SpriteRenderer artwork = null;
        [SerializeField] private float interactionRange = .95f;
        [SerializeField] private ToolboxVisual toolbox = null;
        [SerializeField] private DialogueSequence examineToolbox = null;
        public override string Prompt => toolbox != null && !RetenMission.Session.ToolboxExamined ? "[E] Examinar caja" :
            toolbox != null && !RetenMission.Session.ToolboxOpened ? "[E] Abrir caja" : "E: recoger fusible";
        private void OnEnable() { Refresh(); }
        private void Update() { Refresh(); }
        private void Refresh()
        {
            if (artwork != null) artwork.enabled = !RetenMission.Session.FuseCollected &&
                (toolbox == null || RetenMission.Session.ToolboxOpened && !toolbox.IsOpening);
        }
        public override bool IsAvailable(InteractionProbe player) => RetenMission.Session.GeneratorAvailable &&
            !RetenMission.Session.FuseCollected && (toolbox == null || !toolbox.IsOpening) &&
            player.CanReach(InteractionPoint, interactionRange, Vector2.down, 1f);
        public override void Interact(NarrativeDirector director)
        {
            if (director == null || director.IsOpen || RetenTravel.IsLoading || toolbox != null && toolbox.IsOpening) return;
            var state = RetenMission.Session;
            if (toolbox != null && !state.ToolboxExamined)
            {
                director.OpenDialogue(examineToolbox, () => state.ExamineToolbox());
                return;
            }
            if (toolbox != null && !state.ToolboxOpened)
            {
                if (state.OpenToolbox()) toolbox.BeginOpening();
                return;
            }
            RetenMission.CollectFuse();
            Refresh();
        }
    }
}
