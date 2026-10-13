using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(200), RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public sealed class CinematicWalk : MonoBehaviour
    {
        private Rigidbody2D body;
        private Collider2D feet;
        private Vector2 velocity;
        public bool IsWalking { get; private set; }
        private void Awake() { body = GetComponent<Rigidbody2D>(); feet = GetComponent<Collider2D>(); }
        public IEnumerator Walk(Vector2 direction, float seconds, float speed = .75f)
        {
            IsWalking = true; velocity = direction.normalized * speed;
            for (float t = 0; t < seconds; t += Time.deltaTime) yield return null;
            Stop();
        }
        private void FixedUpdate()
        {
            if (!IsWalking) return;
            float distance = velocity.magnitude * Time.fixedDeltaTime;
            float allowed = distance;
            foreach (var hit in Physics2D.CircleCastAll(feet.bounds.center, .25f, velocity.normalized, distance + .035f))
                if (hit.collider != null && !hit.collider.isTrigger && hit.collider != feet && hit.rigidbody != body)
                    allowed = Mathf.Min(allowed, Mathf.Max(0, hit.distance - .035f));
            body.linearVelocity = distance > 0 ? velocity * (allowed / distance) : Vector2.zero;
        }
        public void Stop() { IsWalking = false; velocity = Vector2.zero; if (body != null) body.linearVelocity = Vector2.zero; }
        private void OnDisable() { if (IsWalking) Stop(); }
    }
}
