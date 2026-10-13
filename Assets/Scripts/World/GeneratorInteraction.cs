using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class GeneratorInteraction : WorldInteraction
    {
        [SerializeField] private DialogueSequence examine = null;
        [SerializeField] private DialogueSequence missingFuse = null;
        [SerializeField] private DialogueSequence install = null;
        [SerializeField] private DialogueSequence running = null;
        [SerializeField] private float interactionRange = 1.2f;
        public override string Prompt => RetenMission.Session.GeneratorRepaired ? "E: examinar generador" :
            RetenMission.Session.GeneratorExamined && RetenMission.Session.HasFuse ? "E: instalar fusible" : "E: examinar generador";
        public override bool IsAvailable(InteractionProbe player) => RetenMission.Session.GeneratorAvailable &&
            player.CanReach(InteractionPoint, interactionRange, Vector2.down, 1.2f);
        public override void Interact(NarrativeDirector director)
        {
            MissionProgress state = RetenMission.Session;
            if (state.GeneratorRepaired) director.OpenDialogue(running, null);
            else if (!state.GeneratorExamined) director.OpenDialogue(examine, () => state.ExamineGenerator());
            else if (state.HasFuse) director.OpenDialogue(install, RetenMission.RepairGenerator);
            else director.OpenDialogue(missingFuse, null);
        }
    }
}
