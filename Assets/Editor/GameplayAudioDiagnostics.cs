using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FrecuenciaCero;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Editor-only diagnosis: does not author scenes, attach gameplay components or change audio assets.
public static class GameplayAudioDiagnostics
{
    private static string Folder => Path.GetFullPath(Path.Combine(Application.dataPath, "../Temp/RevisionAudio"));
    private static PropertyInfo MuteProperty()
    {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
        var p = typeof(EditorUtility).GetProperty("audioMasterMute", flags);
        if (p != null) return p;
        var type = AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType("UnityEditor.AudioUtil")).FirstOrDefault(t => t != null);
        return type?.GetProperty("masterMute", flags);
    }
    private static string EditorMute()
    {
        var p = MuteProperty(); return p == null ? "not exposed by this editor" : p.GetValue(null).ToString();
    }
    private static string Hierarchy(Transform t) => t.parent == null ? t.name : Hierarchy(t.parent) + "/" + t.name;

    [MenuItem("Tools/Frecuencia Cero/Audio/Guardar diagnóstico de escena actual")]
    public static void Inspect()
    {
        Directory.CreateDirectory(Folder);
        var scene = SceneManager.GetActiveScene();
        var lines = new List<string>
        {
            "Project=" + Path.GetFullPath(Path.Combine(Application.dataPath, "..")),
            "Scene=" + scene.path + " dirty=" + scene.isDirty,
            "Play=" + EditorApplication.isPlaying + " EditorMasterMute=" + EditorMute(),
            "EffectsVolume=" + MenuPreferences.EffectsVolume + " MusicVolume=" + MenuPreferences.MusicVolume,
            "ListenerVolume=" + AudioListener.volume + " ListenerPause=" + AudioListener.pause,
            "GameplayPause=" + GameplayPause.IsPaused + " TimeScale=" + Time.timeScale,
            "AudioConfiguration=" + JsonUtility.ToJson(AudioSettings.GetConfiguration())
        };
        foreach (var leo in UnityEngine.Object.FindObjectsByType<LeoMovement>(FindObjectsInactive.Include))
        {
            var foot = leo.GetComponent<LeoFootstepAudio>();
            lines.Add("LEO " + Hierarchy(leo.transform) + " active=" + leo.gameObject.activeInHierarchy + " enabled=" + leo.enabled + " footsteps=" + (foot != null));
            lines.Add("COMPONENTS " + string.Join(", ", leo.GetComponents<Component>().Select(c => c == null ? "Missing script" : c.GetType().Name)));
            if (foot == null) continue;
            var so = new SerializedObject(foot);
            var sounds = so.FindProperty("sounds").objectReferenceValue as GameSoundPalette;
            var map = so.FindProperty("surfaceMap").objectReferenceValue;
            lines.Add("FOOTSTEPS enabled=" + foot.enabled + " palette=" + (sounds != null ? AssetDatabase.GetAssetPath(sounds) : "NULL") + " surface=" + (map != null ? map.name : "NULL") + " gain=" + so.FindProperty("gain").floatValue);
            lines.Add("FOOTSTEPS played=" + foot.PlayedCount + " lastSurface=" + foot.LastSurface + " lastClip=" + (foot.LastClip != null ? foot.LastClip.name : "NULL"));
            if (sounds != null) foreach (FootstepSurface surface in Enum.GetValues(typeof(FootstepSurface)))
            {
                var clips = sounds.Steps(surface) ?? Array.Empty<AudioClip>();
                lines.Add("BANK " + surface + " clips=" + clips.Length + " null=" + clips.Count(c => c == null));
            }
        }
        var listeners = UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsInactive.Include);
        lines.Add("LISTENERS active=" + listeners.Count(a => a.isActiveAndEnabled));
        foreach (var listener in listeners) lines.Add("LISTENER " + Hierarchy(listener.transform) + " enabled=" + listener.enabled);
        foreach (var source in UnityEngine.Object.FindObjectsByType<AudioSource>(FindObjectsInactive.Include))
            lines.Add("SOURCE " + Hierarchy(source.transform) + " active=" + source.isActiveAndEnabled + " clip=" + (source.clip != null ? source.clip.name : "NULL (may be assigned by script)") +
                " volume=" + source.volume + " mute=" + source.mute + " loop=" + source.loop + " spatial=" + source.spatialBlend + " playing=" + source.isPlaying + " virtual=" + source.isVirtual + " ignorePause=" + source.ignoreListenerPause);
        File.WriteAllLines(Path.Combine(Folder, "EscenaActual.txt"), lines);
        Debug.Log("Diagnóstico de audio guardado en " + Folder + ". EffectsVolume=" + MenuPreferences.EffectsVolume);
    }

    [MenuItem("Tools/Frecuencia Cero/Audio/Restaurar efectos al 80 por ciento")]
    public static void RestoreEffects()
    {
        Directory.CreateDirectory(Folder);
        float before = MenuPreferences.EffectsVolume;
        string muteBefore = EditorMute();
        // Preserve music, display and progress preferences; zero is still a valid user choice.
        MenuPreferences.SetEffects(.8f);
        PlayerPrefs.Save();
        var mute = MuteProperty();
        if (mute != null && mute.CanWrite && (bool)mute.GetValue(null)) mute.SetValue(null, false);
        File.WriteAllText(Path.Combine(Folder, "CorreccionVolumen.txt"), "EffectsBefore=" + before + " EffectsAfter=" + MenuPreferences.EffectsVolume +
            " EditorMuteBefore=" + muteBefore + " EditorMuteAfter=" + EditorMute() + "\nOnly effects volume and an observed editor mute are restored; no scene/assets saved.");
        Inspect();
    }

    [MenuItem("Tools/Frecuencia Cero/Audio/Seleccionar Leo y sus pisadas")]
    public static void SelectLeo()
    {
        var leo = UnityEngine.Object.FindAnyObjectByType<LeoMovement>();
        if (leo != null) { Selection.activeGameObject = leo.gameObject; EditorGUIUtility.PingObject(leo.gameObject); }
    }
}
