using System.Collections.Generic;
using UnityEngine;

namespace FrecuenciaCero
{
    // One scene controller selects one target; targets never read keys or draw prompts.
    public abstract class WorldInteraction : MonoBehaviour
    {
        public abstract string Prompt { get; }
        public virtual Vector2 InteractionPoint => transform.position;
        public abstract bool IsAvailable(InteractionProbe player);
        public abstract void Interact(NarrativeDirector director);
    }

    public sealed class InteractionProbe
    {
        public Transform Leo { get; }
        public Rigidbody2D Body { get; }
        public Collider2D Feet { get; }
        public Vector2 Position => Feet != null ? (Vector2)Feet.bounds.center : (Vector2)Leo.position;
        private readonly List<RaycastHit2D> hits = new List<RaycastHit2D>(8);

        public InteractionProbe(Transform leo)
        {
            Leo = leo;
            Body = leo.GetComponent<Rigidbody2D>();
            Feet = leo.GetComponent<Collider2D>();
        }

        public bool CanReach(Vector2 point, float range, Vector2 direction, float halfWidth)
        {
            if (Feet == null || !Feet.enabled) return false;
            Vector2 offset = Position - point;
            Vector2 front = direction.sqrMagnitude > .001f ? direction.normalized : Vector2.down;
            if (offset.sqrMagnitude > range * range || Vector2.Dot(offset, front) < -.12f ||
                Mathf.Abs(Vector2.Dot(offset, new Vector2(-front.y, front.x))) > halfWidth) return false;
            var filter = new ContactFilter2D { useTriggers = false };
            filter.SetLayerMask(Physics2D.GetLayerCollisionMask(Leo.gameObject.layer));
            Physics2D.Linecast(Position, point, filter, hits);
            foreach (RaycastHit2D hit in hits)
                if (hit.collider != null && hit.collider != Feet && hit.rigidbody != Body &&
                    !hit.collider.transform.IsChildOf(Leo)) return false;
            return true;
        }
    }
}
