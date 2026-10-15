using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(LeoMovement))]
    public sealed class SurvivalPlayer : MonoBehaviour
    {
        [SerializeField, Min(.1f)] private float invulnerability = 1f;
        [SerializeField] private Vector2 safeCheckpoint = new Vector2(-8, 8);
        [SerializeField] private bool forestCheckpoint;
        private LeoMovement movement;
        private SpriteRenderer sprite;
        private NarrativeDirector director;
        private float immuneUntil;
        private float healedUntil;
        private Color originalColor;
        private bool retrying;
        public bool Dead => RetenMission.Session.Survival.Health <= 0;
        public bool Cinematic { get; set; }
        public bool Aiming { get; set; }
        public bool CanAct => !GameplayPause.InputsBlocked && !Dead && !Cinematic && !RetenTravel.IsLoading && movement.enabled && (director == null || !director.IsOpen);
        public float DeathFade { get; private set; }
        public float HealPulse => Mathf.Clamp01((healedUntil - Time.time) / .55f);
        public static string Notice { get; private set; }
        public static float NoticeUntil { get; private set; }
        public static void Notify(string message) { Notice = message; NoticeUntil = GameplayPause.PresentationTime + 3f; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetNotice() { Notice = null; NoticeUntil = 0; }
        private void Awake()
        {
            movement = GetComponent<LeoMovement>(); sprite = GetComponent<SpriteRenderer>();
            director = FindAnyObjectByType<NarrativeDirector>(); originalColor = sprite.color;
            RetenMission.Session.Survival.ConfigureMedkitCapacity(SurvivalInventorySettings.MedkitCapacity);
        }
        private void Update()
        {
            if (GameplayPause.InputsBlocked) return;
            movement.IsBlocked = Dead || Cinematic || Aiming;
            // Healing is visual only: it grants no extra invulnerability and never masks a damage flash.
            sprite.color = Time.time < immuneUntil ? Color.Lerp(originalColor, new Color(1,.25f,.25f), Mathf.PingPong(Time.time * 14, 1)) :
                Time.time < healedUntil ? Color.Lerp(originalColor, new Color(.65f, 1f, .75f), Mathf.Clamp01((healedUntil - Time.time) / .55f)) : originalColor;
            if (Dead) DeathFade = Mathf.MoveTowards(DeathFade, .9f, Time.unscaledDeltaTime * .8f);
            if (Dead && DeathFade > .8f && Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame) Retry();
            if (CanAct && !Aiming && Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                var battery = GetComponentInChildren<FlashlightBattery>();
                if (battery != null && battery.Percent < 99 && RetenMission.Session.Survival.UseBattery()) battery.RestoreCharge(100);
            }
            if (CanAct && !Aiming && Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame) UseMedkit();
        }
        public bool UseMedkit()
        {
            if (!CanAct || Aiming || movement.IsBlocked) return false;
            var inventory = RetenMission.Session.Survival;
            if (inventory.Medkits <= 0) { Notify("No tienes botiquines."); return false; }
            if (inventory.Health >= SurvivalProgress.MaximumHealth) { Notify("Salud completa. Botiquín conservado."); return false; }
            if (!inventory.UseMedkit()) return false;
            healedUntil = Time.time + .55f;
            Notify("Un corazón recuperado."); return true;
        }
        public bool TakeDamage(int amount = 1)
        {
            if (GameplayPause.IsPaused || Cinematic || Dead || Time.time < immuneUntil || !RetenMission.Session.Survival.Damage(amount)) return false;
            immuneUntil = Time.time + invulnerability;
            if (Dead) { Aiming = false; movement.IsBlocked = true; GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; }
            return true;
        }
        public void Retry()
        {
            if (!Dead || retrying) return;
            retrying = true; StartCoroutine(RetryRoutine());
        }
        private IEnumerator RetryRoutine()
        {
            var fade = GeneratorGateFade.Create(); yield return fade.Blend(0, 1, .3f);
            RetenMission.Session.Survival.RetryHealth();
            // Collected IDs, spent items, mission and tutorial remain coherent across retries.
            if (forestCheckpoint)
            {
                foreach (var enemy in FindObjectsByType<Escarbador>(FindObjectsSortMode.None)) enemy.ResetAtHideout();
                var body = GetComponent<Rigidbody2D>(); body.position = safeCheckpoint; body.linearVelocity = Vector2.zero;
                transform.position = new Vector3(safeCheckpoint.x, safeCheckpoint.y, transform.position.z);
                Physics2D.SyncTransforms(); Camera.main.transform.position = new Vector3(safeCheckpoint.x, safeCheckpoint.y, -10);
                immuneUntil = Time.time + invulnerability; DeathFade = 0; movement.IsBlocked = false;
                yield return fade.Blend(1, 0, .5f); Destroy(fade.gameObject); retrying = false;
            }
            else
            {
                if (RetenTravel.Begin("Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity", "bosque_checkpoint",
                    GetComponentInChildren<FlashlightBattery>(), GetComponentInChildren<FlashlightController>())) fade.FinishAfterTravel();
                else { yield return fade.Blend(1, 0, .3f); Destroy(fade.gameObject); retrying = false; }
            }
        }
    }
}
