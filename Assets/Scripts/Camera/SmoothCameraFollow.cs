using UnityEngine;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(Camera))]
    public sealed class SmoothCameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.15f;
        private Vector3 velocity;

        public void Configure(Transform followTarget) => target = followTarget;

        private void Start()
        {
            if (target != null)
                transform.position = new Vector3(target.position.x, target.position.y, -10f);
        }

        private void LateUpdate()
        {
            if (target == null) return;
            Vector3 destination = new Vector3(target.position.x, target.position.y, -10f);
            transform.position = Vector3.SmoothDamp(transform.position, destination, ref velocity,
                smoothTime, Mathf.Infinity, Time.deltaTime);
        }
    }
}
