using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class GeneratorEntryLamp : MonoBehaviour
    {
        [SerializeField] private Light2D lamp;
        [SerializeField] private Transform leo, triggerPoint;
        [SerializeField] private SpriteRenderer pole;
        [SerializeField] private MeshRenderer glassMask;
        [SerializeField] private float triggerRange = 1.35f;
        private readonly float[] intervals = { .12f,.23f,.08f,.37f,.16f,.29f,.07f,.41f,.11f,.32f,.09f,.55f };
        private float age, intensity;
        private Color poleColor;
        private MaterialPropertyBlock glassProperties;
        public bool IsFlickering { get; private set; }
        private void Awake() { intensity = lamp.intensity; poleColor = pole.color; glassProperties = new MaterialPropertyBlock(); }
        private void Start() { Set(RetenMission.Session.GeneratorRepaired || !RetenMission.Session.EntryLampFailureStarted ? 1 : 0); }
        private void Update()
        {
            if (GameplayPause.IsPaused) return;
            var progress = RetenMission.Session;
            if (progress.GeneratorRepaired) { IsFlickering = false; Set(1); return; }
            if (IsFlickering)
            {
                age += Time.deltaTime;
                float until = 0;
                for (int i=0;i<intervals.Length;i++)
                {
                    until += intervals[i];
                    if (age < until) { Set(i % 2 == 0 ? .82f + i % 3 * .06f : .02f); return; }
                }
                IsFlickering = false; Set(0); return;
            }
            if (progress.EntryLampFailureStarted) { Set(0); return; }
            if (leo != null && Vector2.Distance(leo.position,triggerPoint.position) <= triggerRange && progress.TryBeginEntryLampFailure())
            { age = 0; IsFlickering = true; }
        }
        private void Set(float level)
        {
            lamp.enabled = level > 0;
            lamp.intensity = intensity * level;
            pole.color = Color.Lerp(new Color(.55f,.60f,.67f,poleColor.a),poleColor,level);
            if(glassMask != null)
            {
                glassMask.sortingOrder = pole.sortingOrder + 1;
                glassMask.enabled = level < .99f;
                glassProperties.SetColor("_Color",new Color(1,1,1,1-level));
                glassMask.SetPropertyBlock(glassProperties);
            }
        }
    }
}
