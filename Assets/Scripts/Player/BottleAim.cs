using UnityEngine;
using UnityEngine.InputSystem;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(150), RequireComponent(typeof(SurvivalPlayer))]
    public sealed class BottleAim : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float maximumRange = 8f;
        [SerializeField] private Sprite bottleSprite;
        [SerializeField] private Material bottleMaterial;
        private SurvivalPlayer player;
        private Collider2D feet;
        public bool IsAiming => player != null && player.Aiming;
        public bool InRange { get; private set; }
        public Vector2 PredictedImpact { get; private set; }
        private Vector2 selected;
        private void Awake() { player = GetComponent<SurvivalPlayer>(); feet = GetComponent<Collider2D>(); }
        private void LateUpdate()
        {
            if (GameplayPause.InputsBlocked) return;
            var keyboard = Keyboard.current; var mouse = Mouse.current;
            if (!player.CanAct || RetenMission.Session.Survival.Bottles <= 0) { player.Aiming = false; return; }
            if (!IsAiming && keyboard != null && keyboard.qKey.wasPressedThisFrame &&
                (mouse == null || !mouse.rightButton.isPressed))
            { player.Aiming = true; GetComponent<LeoMovement>().IsBlocked = true; GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; return; }
            if (!IsAiming) return;
            if (mouse != null && mouse.rightButton.wasPressedThisFrame)
            {
                player.Aiming = false; InRange = false;
                // Remove only the aiming lock; narrative, death and travel still own control.
                if (player.CanAct) GetComponent<LeoMovement>().IsBlocked = false;
                return;
            }
            if (mouse == null || Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Mathf.Abs(ray.direction.z) < .0001f) return;
            selected = ray.origin + ray.direction * (-ray.origin.z / ray.direction.z);
            Vector2 origin = feet.bounds.center;
            InRange = (selected - origin).magnitude <= maximumRange;
            PredictedImpact = BottleProjectile.FindImpact(origin, selected, feet);
            if (!mouse.leftButton.wasPressedThisFrame || !InRange) return;
            if (!RetenMission.Session.Survival.SpendBottle()) return;
            var obj = new GameObject("Botella lanzada", typeof(SpriteRenderer), typeof(BottleProjectile));
            var renderer = obj.GetComponent<SpriteRenderer>(); renderer.sprite = bottleSprite; renderer.sharedMaterial = bottleMaterial;
            obj.transform.localScale = Vector3.one * .7f;
            obj.GetComponent<BottleProjectile>().Launch(origin, PredictedImpact, feet);
            player.Aiming = false;
        }
        private void OnDisable() { if (player != null) player.Aiming = false; }
    }
}
