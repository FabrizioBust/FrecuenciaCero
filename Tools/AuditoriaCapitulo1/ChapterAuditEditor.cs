using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ChapterAuditEditor
{
    static ChapterAuditEditor()
    {
        EditorApplication.playModeStateChanged+=state=>{if(state==PlayModeStateChange.EnteredPlayMode)StartProbe();};
        EditorApplication.delayCall+=()=>{if(EditorApplication.isPlaying)StartProbe();};
    }
    static void StartProbe()
    {
        if(!SessionState.GetBool("ChapterAudit.Play",false)||UnityEngine.Object.FindAnyObjectByType<ChapterAuditRuntime>()!=null)return;
        EditorApplication.LockReloadAssemblies();Application.runInBackground=true;
        EditorApplication.update+=()=>{EditorApplication.isPaused=false;EditorApplication.QueuePlayerLoopUpdate();foreach(var w in Resources.FindObjectsOfTypeAll<EditorWindow>())if(w.GetType().Name=="GameView")w.Repaint();};
        new GameObject("Auditoría automatizada",typeof(ChapterAuditRuntime));
    }
    public static string Reports => Path.GetFullPath(Path.Combine(Application.dataPath,"../../AuditoriaCapitulo1"));
    public static void Inspect()
    {
        Directory.CreateDirectory(Reports);
        var lines=new List<string>(); int missing=0,references=0;
        var ids=new Dictionary<string,string>();
        foreach(var entry in EditorBuildSettings.scenes.Where(s=>s.enabled))
        {
            var scene=EditorSceneManager.OpenScene(entry.path);
            var components=scene.GetRootGameObjects().SelectMany(g=>g.GetComponentsInChildren<Component>(true)).ToArray();
            lines.Add("SCENE "+entry.path+" components="+components.Length);
            foreach(var root in scene.GetRootGameObjects())
            foreach(var transform in root.GetComponentsInChildren<Transform>(true))
            {
                int count=GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject);
                missing+=count;if(count>0)lines.Add("MISSING SCRIPT "+transform.name);
            }
            foreach(var component in components.Where(c=>c!=null))
            {
                if(component is MonoBehaviour)
                {
                    var serialized=new SerializedObject(component);var property=serialized.GetIterator();
                    while(property.NextVisible(true))
                        if(property.propertyType==SerializedPropertyType.ObjectReference&&property.objectReferenceValue==null&&!property.objectReferenceEntityIdValue.Equals(default(UnityEngine.EntityId)))
                        {references++;lines.Add("MISSING REFERENCE "+component.name+" "+component.GetType().Name+" "+property.propertyPath);}
                }
                if(component is SupplyPickup pickup)
                {
                    if(string.IsNullOrWhiteSpace(pickup.UniqueId))lines.Add("EMPTY PICKUP "+pickup.name);
                    else if(ids.ContainsKey(pickup.UniqueId))lines.Add("DUPLICATE PICKUP "+pickup.UniqueId+" "+entry.path+" previous="+ids[pickup.UniqueId]);
                    else ids[pickup.UniqueId]=entry.path;
                }
                if(component is RetenArrival || component is RetenDoor || component is GeneratorAccessGate || component is ForestPassage)
                {
                    var so=new SerializedObject(component);var p=so.GetIterator();string fields="";
                    while(p.NextVisible(true))if(p.propertyType==SerializedPropertyType.String)fields+=" "+p.name+"="+p.stringValue;
                    lines.Add("TRAVEL "+component.GetType().Name+" "+component.name+" position="+component.transform.position+fields);
                }
            }
            lines.Add("CAMERAS "+components.OfType<Camera>().Count(c=>c.enabled&&c.gameObject.activeInHierarchy));
            lines.Add("LISTENERS "+components.OfType<AudioListener>().Count(c=>c.enabled&&c.gameObject.activeInHierarchy));
            if(scene.name.StartsWith("N1_")||scene.name=="N2_SenderoCabana")
            foreach(var target in components.OfType<WorldInteraction>())lines.Add("TARGET "+target.GetType().Name+" "+target.name+" point="+target.InteractionPoint);
        }
        lines.Add("RESULT missingScripts="+missing+" missingReferences="+references+" pickupIds="+ids.Count);
        File.WriteAllLines(Path.Combine(Reports,"InspeccionEscenas.txt"),lines);
        if(missing+references>0)throw new Exception("Referencias ausentes: "+(missing+references));
    }
    public static void Play()
    {
        Application.runInBackground=true;
        EditorApplication.update+=()=>{EditorApplication.isPaused=false;EditorApplication.QueuePlayerLoopUpdate();foreach(var w in Resources.FindObjectsOfTypeAll<EditorWindow>())if(w.GetType().Name=="GameView")w.Repaint();};
        var gameView=typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");EditorWindow.GetWindow(gameView);
        SessionState.SetBool("ChapterAudit.Play",true);
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_ExteriorReten.unity");
        EditorApplication.isPlaying=true;
    }
    public static void Build()
    {
        var levels=new List<string>();
        foreach(var guid in AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Audio/Procesados"}))
        {
            var path=AssetDatabase.GUIDToAssetPath(guid);var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);clip.LoadAudioData();
            var samples=new float[clip.samples*clip.channels];if(!clip.GetData(samples,0))throw new Exception("Audio sin datos: "+path);
            float peak=samples.Max(x=>Mathf.Abs(x));if(peak<=0||peak>=.5f)throw new Exception("Nivel incorrecto: "+path+" "+peak);
            levels.Add(path+" channels="+clip.channels+" rate="+clip.frequency+" peak="+peak);
        }
        File.WriteAllLines(Path.Combine(Reports,"NivelesAudio.txt"),levels);
        var buildFolder=Path.GetFullPath(Path.Combine(Application.dataPath,"../../../Builds/AuditoriaCapitulo1Windows"));
        Directory.CreateDirectory(buildFolder);
        var options=new BuildPlayerOptions{scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray(),locationPathName=Path.Combine(buildFolder,"FrecuenciaCero.exe"),target=BuildTarget.StandaloneWindows64,options=BuildOptions.Development};
        var report=BuildPipeline.BuildPlayer(options);
        File.WriteAllText(Path.Combine(Reports,"BuildWindows.txt"),"Result="+report.summary.result+" errors="+report.summary.totalErrors+" warnings="+report.summary.totalWarnings+" size="+report.summary.totalSize+" duration="+report.summary.totalTime);
        if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build Windows falló");
    }
}
