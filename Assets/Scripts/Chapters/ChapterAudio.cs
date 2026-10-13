using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class ChapterAudio : MonoBehaviour
    {
        [Serializable] public sealed class Recording { public string cue; public AudioClip clip; }
        [Tooltip("Asignar grabaciones definitivas. Las voces sin clip sólo emiten el evento; no se sintetizan.")]
        [SerializeField] private Recording[] recordings = {
            new Recording { cue = "whisper_leo" }, new Recording { cue = "radio_warning" },
            new Recording { cue = "stranger_station" }, new Recording { cue = "leo_technician" }, new Recording { cue = "stranger_unsaid" }
        };
        [SerializeField] private UnityEvent<string> onCue = new UnityEvent<string>();
        private readonly Dictionary<string, AudioClip> effects = new Dictionary<string, AudioClip>();
        private AudioSource source;
        public event Action<string> CuePlayed;
        public string LastCue { get; private set; }
        private void Awake()
        {
            source = GetComponent<AudioSource>(); source.playOnAwake = false; source.loop = false; source.spatialBlend = 0;
            MenuPreferences.AudioChanged += RefreshVolume;
            MakeEffect("radio_static", 1.25f, 17);
            MakeEffect("radio_cut", .18f, 31);
            MakeEffect("knock", .8f, 47);
        }
        public void PlayCue(string cue)
        {
            LastCue = cue; CuePlayed?.Invoke(cue); onCue.Invoke(cue);
            AudioClip clip = null;
            foreach (var recording in recordings) if (recording != null && recording.cue == cue && recording.clip != null) { clip = recording.clip; break; }
            if (clip == null) effects.TryGetValue(cue, out clip);
            if (clip == null) return;
            RefreshVolume(); source.PlayOneShot(clip);
        }
        private void RefreshVolume() { if (source != null) source.volume = MenuPreferences.EffectsVolume * .65f; }
        private void MakeEffect(string cue, float seconds, int seed)
        {
            int count = Mathf.RoundToInt(seconds * 22050);
            var samples = new float[count]; var random = new System.Random(seed); float low = 0;
            for (int i = 0; i < count; i++)
            {
                float t = i / 22050f, noise = (float)random.NextDouble() * 2 - 1;
                low = low * .7f + noise * .3f;
                if (cue == "knock")
                {
                    float tap = t % .28f;
                    samples[i] = Mathf.Sin(t * Mathf.PI * 2 * 125) * Mathf.Exp(-tap * 45) * .32f + low * Mathf.Exp(-tap * 60) * .08f;
                }
                else samples[i] = (low * .12f + noise * .045f) * Mathf.Sin(Mathf.PI * i / count);
            }
            var clip = AudioClip.Create(cue + " — efecto local", count, 1, 22050, false); clip.SetData(samples, 0); effects.Add(cue, clip);
        }
        private void OnDestroy()
        {
            MenuPreferences.AudioChanged -= RefreshVolume;
            foreach (var clip in effects.Values) if (clip != null) Destroy(clip);
        }
    }
}
