using UnityEngine;

namespace FrecuenciaCero
{
    public enum RetenInteractionKind { Radio, Logbook, Map }

    [DisallowMultipleComponent]
    public sealed class NarrativeTarget : WorldInteraction
    {
        [SerializeField] private RetenInteractionKind kind = RetenInteractionKind.Radio;
        [SerializeField, Min(.1f)] private float interactionRange = .85f;
        [SerializeField, Min(.1f)] private float approachHalfWidth = .65f;
        [SerializeField] private Vector2 approachDirection = Vector2.down;
        public override string Prompt => kind == RetenInteractionKind.Radio ? "E: usar radio" :
            kind == RetenInteractionKind.Logbook ? "E: leer bitácora" : "E: examinar mapa";
        public override bool IsAvailable(InteractionProbe player) =>
            player.CanReach(InteractionPoint, interactionRange, approachDirection, approachHalfWidth);
        public override void Interact(NarrativeDirector director) => director.OpenTarget(kind);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(.55f, .72f, .66f, .75f);
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)approachDirection.normalized * interactionRange);
        }
    }
}
