using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent, RequireComponent(typeof(AudioSource))]
    public sealed class TreeFallEvent : MonoBehaviour
    {
        [SerializeField] private NarrativeDirector director;
        [SerializeField] private DialogueSequence reaction;
        [SerializeField] private Camera worldCamera;
        [SerializeField] private Light2D[] interiorLights = new Light2D[0];
        [SerializeField] private AudioClip woodCreak;
        [SerializeField] private AudioClip impact;
        [SerializeField, Range(0f, 1f)] private float volume = .65f;
        private AudioSource source;
        private AudioClip generatedCreak, generatedImpact;
        private float[] intensities;
        private bool running, projectionChanged;
        private float shakeUntil;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.volume = MenuPreferences.EffectsVolume;
            intensities = new float[interiorLights.Length];
            for (int i = 0; i < interiorLights.Length; i++)
                if (interiorLights[i] != null) intensities[i] = interiorLights[i].intensity;
            if (woodCreak == null) woodCreak = generatedCreak = MakeClip(false);
            if (impact == null) impact = generatedImpact = MakeClip(true);
        }
        private void Start()
        {
            // Subscribe after Pixel Perfect has registered its render callbacks.
            RenderPipelineManager.beginCameraRendering += BeginRender;
            RenderPipelineManager.endCameraRendering += EndRender;
        }
        private void Update()
        {
            if (running || director == null || director.IsOpen || RetenTravel.IsLoading) return;
            MissionProgress state = RetenMission.Session;
            if (state.TreeFallen)
            {
                if (!state.TreeReactionCompleted) director.OpenDialogue(reaction, state.CompleteTreeReaction);
                return;
            }
            if (state.TryBeginTreeFall()) StartCoroutine(Fall());
        }
        private IEnumerator Fall()
        {
            running = true;
            source.PlayOneShot(woodCreak, volume);
            yield return new WaitForSecondsRealtime(1.25f);
            RetenMission.Session.CompleteTreeFall();
            source.PlayOneShot(impact, volume);
            shakeUntil = Time.unscaledTime + .28f;
            float end = Time.unscaledTime + .32f;
            while (Time.unscaledTime < end)
            {
                float flicker = Mathf.Sin(Time.unscaledTime * 95f) > 0 ? .78f : .94f;
                for (int i = 0; i < interiorLights.Length; i++)
                    if (interiorLights[i] != null) interiorLights[i].intensity = intensities[i] * flicker;
                yield return null;
            }
            RestoreLights();
            running = false;
            // Update opens the existing dialogue UI once it is free, without interrupting documents.
        }
        private void BeginRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera != worldCamera || Time.unscaledTime >= shakeUntil) return;
            float fade = Mathf.Clamp01((shakeUntil - Time.unscaledTime) / .28f);
            Matrix4x4 projection = camera.projectionMatrix;
            projection.m03 += Mathf.Sin(Time.unscaledTime * 107f) * .007f * fade;
            projection.m13 += Mathf.Sin(Time.unscaledTime * 83f) * .005f * fade;
            camera.projectionMatrix = projection;
            projectionChanged = true;
        }
        private void EndRender(ScriptableRenderContext context, Camera camera)
        {
            if (camera == worldCamera && projectionChanged)
            {
                camera.ResetProjectionMatrix();
                projectionChanged = false;
            }
        }
        private void RestoreLights()
        {
            if (intensities == null) return;
            for (int i = 0; i < interiorLights.Length; i++)
                if (interiorLights[i] != null) interiorLights[i].intensity = intensities[i];
        }
        private void OnDisable()
        {
            StopAllCoroutines();
            // Leaving during the buildup still records the accident; it cannot replay on reentry.
            if (running) RetenMission.Session.CompleteTreeFall();
            running = false;
            RestoreLights();
            if (projectionChanged && worldCamera != null) worldCamera.ResetProjectionMatrix();
            projectionChanged = false;
            RenderPipelineManager.beginCameraRendering -= BeginRender;
            RenderPipelineManager.endCameraRendering -= EndRender;
        }
        private void OnDestroy()
        {
            if (generatedCreak != null) Destroy(generatedCreak);
            if (generatedImpact != null) Destroy(generatedImpact);
        }
        private static AudioClip MakeClip(bool isImpact)
        {
            const int rate = 22050;
            float duration = isImpact ? .75f : 1.25f;
            var samples = new float[Mathf.RoundToInt(rate * duration)];
            var random = new System.Random(isImpact ? 612 : 611);
            float brown = 0f;
            for (int i = 0; i < samples.Length; i++)
            {
                float t = i / (float)rate, phase = t / duration;
                float noise = (float)(random.NextDouble() * 2 - 1);
                brown = brown * .93f + noise * .07f;
                float envelope = isImpact ? Mathf.Exp(-t * 8f) * Mathf.Min(1, t * 500f) :
                    (.03f + phase * phase * .7f) * Mathf.Min(1, t * 15f) * Mathf.Clamp01((duration - t) * 30f);
                float signal = isImpact ? brown * 1.9f + Mathf.Sin(t * 2 * Mathf.PI * (68f - phase * 32f)) * .35f :
                    brown * 1.8f + noise * .14f * Mathf.Pow(Mathf.Abs(Mathf.Sin(t * 21f)), 12f);
                samples[i] = Mathf.Clamp(signal * envelope, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create(isImpact ? "Provisional_Impacto_Arbol" : "Provisional_Crujido_Arbol", samples.Length, 1, rate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
