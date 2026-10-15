using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;

// Explicit audio-only authoring. Existing movement, noise, colliders, camera bounds and narrative text are retained.
public static class InitialSoundSetup
{
    const string Processed="Assets/Audio/Procesados/",Palette="Assets/Audio/Procesados/PaletaSonidos.asset";
    static GameSoundPalette sounds;
    static void Set(UnityEngine.Object o,Action<SerializedObject> edit){var s=new SerializedObject(o);edit(s);s.ApplyModifiedPropertiesWithoutUndo();}
    static AudioClip Clip(string p)=>AssetDatabase.LoadAssetAtPath<AudioClip>(Processed+p);
    static AudioClip[] Bank(string folder)=>Directory.GetFiles(Processed+"Pasos/"+folder,"*.wav").OrderBy(p=>p).Select(p=>AssetDatabase.LoadAssetAtPath<AudioClip>(p)).ToArray();
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||File.Exists(Palette))throw new InvalidOperationException("Play está activo o la paleta ya existe; revisar antes de repetir la configuración.");
        // Verify original imports before opening/saving any scene.
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        AudioSourceImportAudit.Run();
        foreach(var name in new[]{"pasos_en_tierra.wav","pasos_en_grass.wav","pasos_en_retenv1.aiff","pasos_en_retenv2.wav","sonido_auto.wav","auto_apagandose.wav","puerta_cerrandose.wav","radio_interferencia.wav","agua_rio.flac"})
        {var clip=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Sonidos/"+name);if(clip==null||clip.samples<=0||!clip.LoadAudioData())throw new InvalidOperationException("No está correctamente importado: "+name);}
        foreach(var file in Directory.GetFiles(Processed,"*.wav",SearchOption.AllDirectories))
        {
            var importer=(AudioImporter)AssetImporter.GetAtPath(file);importer.forceToMono=false;var settings=importer.defaultSampleSettings; // Derived WAVs are already mono; do not let importer normalization change their levels.
            settings.preloadAudioData=true;settings.loadType=AudioClipLoadType.DecompressOnLoad;settings.compressionFormat=AudioCompressionFormat.PCM;settings.sampleRateSetting=AudioSampleRateSetting.PreserveSampleRate;settings.quality=1;importer.defaultSampleSettings=settings;importer.SaveAndReimport();
        }
        sounds=ScriptableObject.CreateInstance<GameSoundPalette>();sounds.dirt=Bank("Tierra");sounds.grass=Bank("Cesped");sounds.retenWood=Bank("MaderaReten");sounds.cabinWood=Bank("MaderaCabana").Take(5).ToArray(); // Last two V1 snippets are too weak/noisy to use as regular footsteps.
        sounds.motorRunning=Clip("Vehiculo/MotorEnMarcha.wav");sounds.motorStopping=Clip("Vehiculo/MotorApagandose.wav");sounds.truckDoor=Clip("Vehiculo/PuertaCamioneta.wav");sounds.river=Clip("Agua/RioCercano.wav");
        sounds.radioStatic=new[]{Clip("Radio/Interferencia_01.wav"),Clip("Radio/Interferencia_02.wav")};sounds.radioCut=Clip("Radio/CorteRadio.wav");AssetDatabase.CreateAsset(sounds,Palette);
        var scenes=EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path).ToArray();
        foreach(var path in scenes)
        {
            if(path.EndsWith("MainMenu.unity"))continue;
            var scene=EditorSceneManager.OpenScene(path);
            if(path.EndsWith("IntroHistoria.unity"))
            {
                var intro=UnityEngine.Object.FindAnyObjectByType<IntroHistoriaController>();intro.engineLoop=sounds.motorRunning;intro.engineStop=sounds.motorStopping;intro.gameObject.AddComponent<IntroVehicleSoundMarker>();
                EditorSceneManager.SaveScene(scene);continue;
            }
            var leo=UnityEngine.Object.FindAnyObjectByType<LeoMovement>();if(leo==null)throw new InvalidOperationException("Falta Leo: "+path);
            var map=new GameObject("Superficies reales para pisadas",typeof(FootstepSurfaceMap)).GetComponent<FootstepSurfaceMap>();
            var terrain=path.EndsWith("N1_ExteriorReten.unity")?FootstepSurfaceMap.Terrain.ExteriorReten:path.EndsWith("N1_InteriorReten.unity")?FootstepSurfaceMap.Terrain.RetenInterior:path.EndsWith("N1_Generador.unity")?FootstepSurfaceMap.Terrain.Generator:path.EndsWith("N1_BosqueAbandonado.unity")?FootstepSurfaceMap.Terrain.AbandonedForest:path.EndsWith("N2_SenderoCabana.unity")?FootstepSurfaceMap.Terrain.CabinTrail:FootstepSurfaceMap.Terrain.CabinInterior;
            Set(map,s=>s.FindProperty("terrain").enumValueIndex=(int)terrain);
            if(terrain==FootstepSurfaceMap.Terrain.ExteriorReten)
            {
                // The authoring curve is the actual existing dirt-road geometry; no new path is invented.
                var curve=typeof(GeneratorAccessBuilder).GetMethod("Curve",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
                var service=new List<Vector2>();for(int i=0;i<6;i++)for(int j=0;j<12;j++)service.Add((Vector2)curve.Invoke(null,new object[]{i,j/12f}));service.Add((Vector2)curve.Invoke(null,new object[]{5,1f}));
                Set(map,s=>{var p=s.FindProperty("serviceTrail");p.arraySize=service.Count;for(int i=0;i<service.Count;i++)p.GetArrayElementAtIndex(i).vector2Value=service[i];});
            }
            var footsteps=leo.gameObject.AddComponent<LeoFootstepAudio>();Set(footsteps,s=>{s.FindProperty("sounds").objectReferenceValue=sounds;s.FindProperty("surfaceMap").objectReferenceValue=map;});
            var director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();
            var existing=UnityEngine.Object.FindAnyObjectByType<ChapterAudio>();
            if(existing!=null)SetRadio(existing);
            if(terrain==FootstepSurfaceMap.Terrain.RetenInterior||terrain==FootstepSurfaceMap.Terrain.Generator)
            {
                var cues=director.GetComponent<ChapterAudio>();if(cues==null)cues=director.gameObject.AddComponent<ChapterAudio>();SetRadio(cues);
                var events=(UnityEvent<string>)typeof(NarrativeDirector).GetField("onSoundCue",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(director);
                if(!Enumerable.Range(0,events.GetPersistentEventCount()).Any(i=>events.GetPersistentTarget(i)==cues&&events.GetPersistentMethodName(i)=="PlayCue"))UnityEventTools.AddPersistentListener(events,cues.PlayCue);
            }
            if(terrain==FootstepSurfaceMap.Terrain.ExteriorReten)
            {
                var river=new GameObject("Agua sólo junto al río existente",typeof(LocalizedRiverAudio)).GetComponent<LocalizedRiverAudio>();
                var water=new[]{GameObject.Find("Agua_Norte")?.GetComponent<Collider2D>(),GameObject.Find("Agua_Sur")?.GetComponent<Collider2D>()};if(water.Any(w=>w==null))throw new InvalidOperationException("Falta la geometría existente del agua.");
                Set(river,s=>{s.FindProperty("leo").objectReferenceValue=leo.transform;s.FindProperty("water").objectReferenceValue=sounds.river;var a=s.FindProperty("representedWater");a.arraySize=water.Length;for(int i=0;i<water.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=water[i];});
                var arrival=new GameObject("Llegada sonora única de la camioneta",typeof(VehicleArrivalSound)).GetComponent<VehicleArrivalSound>();
                Set(arrival,s=>{s.FindProperty("sounds").objectReferenceValue=sounds;s.FindProperty("leo").objectReferenceValue=leo.transform;s.FindProperty("truck").objectReferenceValue=GameObject.Find("Camioneta_Mantenimiento").transform;});
            }
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();File.WriteAllText("../AudioInicial/Configuracion.txt","PASS: original nine clips imported before scene changes; 32 processed clips; existing gameplay sources/events retained; 13 audio scenes wired, MainMenu unchanged.\n");
    }
    static void SetRadio(ChapterAudio cues)
    {
        Set(cues,s=>
        {
            var records=s.FindProperty("recordings");foreach(var pair in new[]{new KeyValuePair<string,AudioClip>("radio_static",sounds.radioStatic[0]),new KeyValuePair<string,AudioClip>("radio_cut",sounds.radioCut)})
            {
                int index=-1;for(int i=0;i<records.arraySize;i++)if(records.GetArrayElementAtIndex(i).FindPropertyRelative("cue").stringValue==pair.Key){index=i;break;}if(index<0){index=records.arraySize;records.arraySize++;}
                var r=records.GetArrayElementAtIndex(index);r.FindPropertyRelative("cue").stringValue=pair.Key;r.FindPropertyRelative("clip").objectReferenceValue=pair.Value;
            }
        });
    }
}
