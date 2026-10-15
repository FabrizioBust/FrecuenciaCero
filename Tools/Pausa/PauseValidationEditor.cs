using System.IO;
using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.Linq;
using UnityEditor.Build.Reporting;

[InitializeOnLoad]
public static class PauseValidationEditor
{
    static PauseValidationEditor()
    {
        EditorApplication.playModeStateChanged += s => { if (s == PlayModeStateChange.EnteredPlayMode) StartProbe(); };
        EditorApplication.delayCall += () => { if (EditorApplication.isPlaying) StartProbe(); };
    }
    static void StartProbe()
    {
        if (!SessionState.GetBool("PauseValidation.Play", false) || UnityEngine.Object.FindAnyObjectByType<PauseValidationRuntime>() != null) return;
        EditorApplication.LockReloadAssemblies();
        EditorApplication.update += Pump;
        new GameObject("Pruebas de pausa", typeof(PauseValidationRuntime));
    }
    static void Pump()
    {
        EditorApplication.isPaused = false;
        EditorApplication.QueuePlayerLoopUpdate();
        foreach (var w in Resources.FindObjectsOfTypeAll<EditorWindow>()) if (w.GetType().Name == "GameView") w.Repaint();
    }
    public static void Play()
    {
        Application.runInBackground = true;
        EditorApplication.update += Pump;
        EditorWindow.GetWindow(typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView"));
        SessionState.SetBool("PauseValidation.Play", true);
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_ExteriorReten.unity");
        EditorApplication.isPlaying = true;
    }
    public static void SetSize(int width, int height)
    {
        var assembly = typeof(EditorWindow).Assembly;
        var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
        var sizes = sizesType.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
        var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new[] { Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeGroupType"), "Standalone") });
        var sizeType = assembly.GetType("UnityEditor.GameViewSize");
        var item = Activator.CreateInstance(sizeType, Enum.Parse(assembly.GetType("UnityEditor.GameViewSizeType"), "FixedResolution"), width, height, "Pause validation " + width + "x" + height);
        group.GetType().GetMethod("AddCustomSize").Invoke(group, new[] { item });
        int index = (int)group.GetType().GetMethod("GetTotalCount").Invoke(group, null) - 1;
        var viewType = assembly.GetType("UnityEditor.GameView");
        var view = EditorWindow.GetWindow(viewType);
        viewType.GetProperty("selectedSizeIndex", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).SetValue(view, index);
        view.Repaint();
    }
    public static void Build()
    {
        string reports = Path.GetFullPath(Path.Combine(Application.dataPath, "../../PausaValidacion"));
        string output = Path.GetFullPath(Path.Combine(Application.dataPath, "../../../Builds/PausaValidacionWindows"));
        Directory.CreateDirectory(reports); Directory.CreateDirectory(output);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = Path.Combine(output, "FrecuenciaCero.exe"),
            target = BuildTarget.StandaloneWindows64, options = BuildOptions.Development
        });
        File.WriteAllText(Path.Combine(reports, "BuildWindows.txt"), "Result=" + report.summary.result +
            " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings + " size=" + report.summary.totalSize + " duration=" + report.summary.totalTime);
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("Build Windows de pausa falló");
    }
}
