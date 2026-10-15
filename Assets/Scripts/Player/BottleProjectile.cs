using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class BottleProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 11f;
        private Vector2 origin, destination, ground;
        private Collider2D owner;
        private float distance, travelled;
        private bool broken;
        private SpriteRenderer artwork;
        private float brokenAt;
        public void Launch(Vector2 from, Vector2 to, Collider2D player)
        {
            origin = ground = from; destination = to; owner = player; distance = Vector2.Distance(from, to);
            transform.position = from; artwork = GetComponent<SpriteRenderer>();
        }
        public static Vector2 FindImpact(Vector2 from, Vector2 to, Collider2D owner)
        {
            Vector2 offset = to - from; float distance = offset.magnitude;
            var hits = Physics2D.CircleCastAll(from, .07f, offset.normalized, distance);
            foreach (var hit in hits)
                if (hit.collider != owner && !hit.collider.isTrigger && (owner == null || hit.rigidbody != owner.attachedRigidbody) && hit.collider.GetComponent<Escarbador>() == null)
                    return hit.centroid;
            return to;
        }
        private void Update()
        {
            if (GameplayPause.IsPaused) return;
            if (broken)
            {
                artwork.color = new Color(.65f,.8f,.7f,Mathf.Clamp01(1-(Time.time-brokenAt)*2));
                if (Time.time-brokenAt > .5f) Destroy(gameObject); return;
            }
            Vector2 next = Vector2.MoveTowards(ground, destination, speed * Time.deltaTime);
            Vector2 impact = FindImpact(ground, next, owner);
            bool collision = (impact-next).sqrMagnitude > .00001f;
            travelled += Vector2.Distance(ground, impact); ground = impact;
            float t = distance > .01f ? Mathf.Clamp01(travelled / distance) : 1;
            transform.position = ground + Vector2.up * (Mathf.Sin(t * Mathf.PI) * .55f);
            transform.rotation = Quaternion.Euler(0,0,travelled*160);
            artwork.sortingOrder = Mathf.RoundToInt(-ground.y * 32) + 5;
            if (collision || Vector2.Distance(ground,destination) < .02f)
            {
                broken = true; brokenAt = Time.time; transform.position = ground;
                transform.localScale = new Vector3(.85f,.15f,1); transform.rotation = Quaternion.identity;
                NoiseEvents.Emit(new NoiseEvent(ground, 1f, 16f, NoiseKind.Bottle));
            }
        }
    }
}
