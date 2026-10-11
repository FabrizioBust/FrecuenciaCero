using UnityEngine;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(300), DisallowMultipleComponent]
    public sealed class RetenArrival : MonoBehaviour
    {
        [SerializeField] private string spawnId = "entrada";
        [SerializeField] private Transform leo = null;
        [SerializeField] private Camera worldCamera = null;
        [SerializeField] private FlashlightBattery battery = null;
        [SerializeField] private FlashlightController flashlight = null;
        private void Start()
        {
            if (leo == null || !RetenTravel.TryArrive(gameObject.scene.name, spawnId, battery, flashlight)) return;
            Rigidbody2D body = leo.GetComponent<Rigidbody2D>();
            leo.position = new Vector3(transform.position.x, transform.position.y, leo.position.z);
            if (body != null)
            {
                body.position = transform.position;
                body.linearVelocity = Vector2.zero;
            }
            Physics2D.SyncTransforms();
            if (worldCamera != null)
                worldCamera.transform.position = new Vector3(leo.position.x, leo.position.y, -10f);
        }
    }
}
