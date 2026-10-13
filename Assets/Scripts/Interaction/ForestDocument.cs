using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class ForestDocument : WorldInteraction
    {
        [SerializeField] private DocumentDefinition document;
        [SerializeField] private float range = 1.2f;
        [SerializeField] private Vector2 interactionOffset;
        public override Vector2 InteractionPoint => (Vector2)transform.position + interactionOffset;
        public override string Prompt => "[E] " + (document != null ? document.title : "Examinar");
        public override bool IsAvailable(InteractionProbe player) => player.CanReach(InteractionPoint, range, Vector2.down, range);
        public override void Interact(NarrativeDirector director) => director.OpenDocument(document, null);
    }
}
