using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class IntroHistoriaController : MonoBehaviour
    {
        public const float Duration = 40;
        public const float SkipHoldDuration = 1;
        public const string SupervisorLine = "Leo, llevamos horas sin recibir señal de la estación. Necesito que revises el retén";
        public const string LeoLine = "Entendido. Estoy llegando";

        [Header("Composición de la introducción; sin referencias al gameplay")]
        public Transform truck, arrival;
        public IntroParallaxLayer[] layers;
        public Light2D headlights;
        public Light2D secondaryHeadlight;
        public MeshRenderer headlightCone;
        public MeshRenderer secondaryBeam;
        public CanvasGroup fade, locationGroup, dialogueGroup, skipGroup;
        public Text location, timeOfNight, speaker, subtitle;
        public Image skipProgress;

        [Header("Audio opcional. Asignar grabaciones propias o con licencia verificada")]
        public AudioClip engineLoop, roadLoop, windLoop, forestLoop;
        public AudioClip supervisorVoice, leoVoice, engineStop;
        public AudioSource engineSource, roadSource, windSource, forestSource, voiceSource;

        private float elapsed, holdTime, headlightIntensity, secondaryIntensity;
        private bool transitioning, waitForSpaceRelease;
        private int voiceCue;
        private Vector3 truckOrigin, arrivalOrigin;
        private MaterialPropertyBlock beamProperties;
        public float Elapsed => elapsed;
        public float SkipProgress => holdTime / SkipHoldDuration;
        public bool IsTransitioning => transitioning;
        public float TravelDistance => DistanceAt(elapsed);

        private void Awake()
        {
            truckOrigin = truck.localPosition;
            arrivalOrigin = arrival.localPosition;
            headlightIntensity = headlights.intensity;
            secondaryIntensity = secondaryHeadlight != null ? secondaryHeadlight.intensity : 0;
            beamProperties = new MaterialPropertyBlock();
            fade.alpha = 1;
            locationGroup.alpha = dialogueGroup.alpha = 0;
            location.text = timeOfNight.text = speaker.text = subtitle.text = "";
            skipProgress.fillAmount = 0;
            skipProgress.rectTransform.localScale = new Vector3(0, 1, 1);
            // Space used to submit Nueva partida must be released before a new hold.
            waitForSpaceRelease = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            ConfigureLoop(engineSource, engineLoop);
            ConfigureLoop(roadSource, roadLoop);
            ConfigureLoop(windSource, windLoop);
            ConfigureLoop(forestSource, forestLoop);
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
            voiceSource.spatialBlend = 0;
            MenuPreferences.AudioChanged += RefreshAudio;
            RefreshAudio();
        }

        private void Start()
        {
            if (!Application.CanStreamedLevelBeLoaded(MainMenuController.GameplayScene))
            {
                Debug.LogError("IntroHistoria: falta N1_ExteriorReten en el Build.", this);
                enabled = false;
                return;
            }
            ApplyTimeline();
        }

        private static void ConfigureLoop(AudioSource source, AudioClip clip)
        {
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0;
            source.clip = clip;
            source.volume = 0;
            if (clip != null) source.Play();
        }

        private void Update()
        {
            if (transitioning) return;
            elapsed = Mathf.Min(Duration, elapsed + Time.unscaledDeltaTime);
            ApplyTimeline();
            bool pressed = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;
            if (waitForSpaceRelease)
            {
                if (!pressed) waitForSpaceRelease = false;
                holdTime = 0;
            }
            else holdTime = pressed ? Mathf.Min(SkipHoldDuration, holdTime + Time.unscaledDeltaTime) : 0;
            skipProgress.fillAmount = SkipProgress;
            // Also scale the simple UI quad: an Image without a sprite ignores fillAmount.
            skipProgress.rectTransform.localScale = new Vector3(SkipProgress, 1, 1);
            if (holdTime >= SkipHoldDuration || elapsed >= Duration) BeginDeparture();
        }

        // Continuous distance and speed: cruise until 28 s, ease to a complete stop at 36 s.
        public static float DistanceAt(float time)
        {
            float braking = Mathf.Clamp(time - 28, 0, 8);
            return 1.6f * Mathf.Clamp(time, 0, 28) + 1.6f * (braking - braking * braking / 16);
        }

        private void ApplyTimeline()
        {
            float distance = TravelDistance;
            foreach (var layer in layers) layer.SetDistance(distance);
            float braking = Mathf.SmoothStep(0, 1, (elapsed - 28) / 8);
            truck.localPosition = truckOrigin + new Vector3(.65f * braking,
                Mathf.Sin(elapsed * 4) * .009f * (1 - braking), 0);
            arrival.localPosition = arrivalOrigin + Vector3.left * distance;
            float lamp = 1 - Mathf.SmoothStep(0, 1, (elapsed - 36.5f) / 1);
            headlights.intensity = headlightIntensity * lamp;
            if (secondaryHeadlight != null) secondaryHeadlight.intensity = secondaryIntensity * lamp;
            headlightCone.GetPropertyBlock(beamProperties);
            beamProperties.SetColor("_Color", new Color(1, 1, 1, lamp));
            headlightCone.SetPropertyBlock(beamProperties);
            if (secondaryBeam != null) secondaryBeam.SetPropertyBlock(beamProperties);
            fade.alpha = 1 - Mathf.SmoothStep(0, 1, elapsed / 2);
            if (elapsed > 38) fade.alpha = Mathf.SmoothStep(0, 1, (elapsed - 38) / 2);
            skipGroup.alpha = Mathf.SmoothStep(0, 1, (elapsed - 1.5f) / .8f) * (1 - fade.alpha);
            locationGroup.alpha = Mathf.SmoothStep(0, 1, (elapsed - 3) / .7f) *
                (1 - Mathf.SmoothStep(0, 1, (elapsed - 9) / 1));
            location.text = Reveal("CIMA DEL ÁGUILA", elapsed - 3, 14);
            timeOfNight.text = Reveal("02:30 A. M.", elapsed - 4.2f, 12);
            if (elapsed >= 12 && elapsed < 23)
            {
                speaker.text = "SUPERVISOR";
                subtitle.text = Reveal(SupervisorLine, elapsed - 12, 26);
                dialogueGroup.alpha = Mathf.SmoothStep(0, 1, (elapsed - 12) / .35f) *
                    (1 - Mathf.SmoothStep(0, 1, (elapsed - 22.4f) / .6f));
                CueVoice(1, supervisorVoice);
            }
            else if (elapsed >= 23 && elapsed < 28)
            {
                speaker.text = "LEO";
                subtitle.text = Reveal(LeoLine, elapsed - 23, 26);
                dialogueGroup.alpha = Mathf.SmoothStep(0, 1, (elapsed - 23) / .35f) *
                    (1 - Mathf.SmoothStep(0, 1, (elapsed - 27.4f) / .6f));
                CueVoice(2, leoVoice);
            }
            else dialogueGroup.alpha = 0;
            if (elapsed >= 36) CueVoice(3, engineStop);
            RefreshAudio();
        }

        private static string Reveal(string value, float time, float rate)
        { return value.Substring(0, Mathf.Clamp(Mathf.FloorToInt(time * rate), 0, value.Length)); }

        private void CueVoice(int cue, AudioClip clip)
        {
            if (voiceCue >= cue) return;
            voiceCue = cue;
            voiceSource.Stop();
            voiceSource.clip = clip;
            if (clip != null) voiceSource.Play();
        }

        private void RefreshAudio()
        {
            float entry = Mathf.SmoothStep(0, 1, elapsed / 2);
            float exit = 1 - Mathf.SmoothStep(0, 1, (elapsed - 38) / 2);
            float speed = 1 - Mathf.Clamp01((elapsed - 28) / 8);
            float volume = MenuPreferences.EffectsVolume * entry * exit;
            engineSource.volume = volume * .5f * (elapsed < 36 ? Mathf.Lerp(.35f, 1, speed) : 0);
            roadSource.volume = volume * .25f * speed;
            windSource.volume = volume * .2f;
            forestSource.volume = volume * .25f;
            voiceSource.volume = MenuPreferences.EffectsVolume * exit;
            if (elapsed >= 36)
            {
                engineSource.Stop();
                roadSource.Stop();
            }
        }

        private void BeginDeparture()
        {
            if (transitioning) return;
            transitioning = true;
            StartCoroutine(Depart());
        }

        private IEnumerator Depart()
        {
            float from = fade.alpha;
            var sources = new[] { engineSource, roadSource, windSource, forestSource, voiceSource };
            var volumes = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++) volumes[i] = sources[i].volume;
            for (float t = 0; t < .8f; t += Time.unscaledDeltaTime)
            {
                float progress = Mathf.SmoothStep(0, 1, t / .8f);
                fade.alpha = Mathf.Lerp(from, 1, progress);
                skipGroup.alpha = 1 - progress;
                for (int i = 0; i < sources.Length; i++) sources[i].volume = volumes[i] * (1 - progress);
                yield return null;
            }
            fade.alpha = 1;
            foreach (var source in sources) source.Stop();
            // The menu has already reset the session. Preserve that same instance and preferences.
            yield return SceneManager.LoadSceneAsync(MainMenuController.GameplayScene, LoadSceneMode.Single);
        }

        private void OnDestroy() { MenuPreferences.AudioChanged -= RefreshAudio; }
    }
}
