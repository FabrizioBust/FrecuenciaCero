using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class GeneratorVisual : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer machine = null;
        [SerializeField] private Sprite offSprite = null;
        [SerializeField] private Sprite onSprite = null;
        [SerializeField] private Light2D[] poweredLights = null;
        [SerializeField, Min(.1f)] private float startDuration = 1.6f;
        private bool lastPowered;
        private float startedAt = -100f;
        private Vector3 restPosition;
        private void Awake() { if (machine != null) restPosition = machine.transform.localPosition; }
        private void Start() { lastPowered = RetenMission.Session.GeneratorRepaired; Apply(); }
        private void Update()
        {
            bool powered = RetenMission.Session.GeneratorRepaired;
            if (powered && !lastPowered) startedAt = Time.time;
            lastPowered = powered;
            Apply();
        }
        private void Apply()
        {
            bool powered = RetenMission.Session.GeneratorRepaired;
            float age = Time.time - startedAt;
            bool starting = powered && age < startDuration;
            if (machine != null)
            {
                machine.sprite = powered && (!starting || Mathf.Sin(age * 18f) > -.3f) ? onSprite : offSprite;
                // Move only the artwork; the collider lives on a separate stationary object.
                float shake = starting ? Mathf.Sin(age * 46f) * .012f * (1f - age / startDuration) : 0f;
                machine.transform.localPosition = restPosition + Vector3.right * shake;
            }
            if (poweredLights == null) return;
            foreach (Light2D lamp in poweredLights)
                if (lamp != null) lamp.enabled = powered && (!starting || Mathf.Sin(age * 13f) > -.45f);
        }
        private void OnDisable() { if (machine != null) machine.transform.localPosition = restPosition; }
    }
}
