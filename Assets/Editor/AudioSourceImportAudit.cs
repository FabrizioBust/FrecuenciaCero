using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

// Read-only asset audit, apart from Unity's normal metadata import. Never edits scenes or originals.
public static class AudioSourceImportAudit
{
    public static void VerifyProcessedLevels()
    {
        var report=new StringBuilder();int count=0;
        foreach(var path in Directory.GetFiles("Assets/Audio/Procesados","*.wav",SearchOption.AllDirectories))
        {
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);importer.forceToMono=false;importer.SaveAndReimport();
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);clip.LoadAudioData();var data=new float[clip.samples*clip.channels];
            if(clip.channels!=1||!clip.GetData(data,0))throw new InvalidOperationException("No se puede validar PCM: "+path);
            float peak=0;foreach(var value in data)peak=Mathf.Max(peak,Mathf.Abs(value));
            if(peak<=0||peak>=.5f)throw new InvalidOperationException("Nivel de importación incorrecto: "+path+" "+peak);
            report.AppendLine("PASS: "+path+" | mono | peak="+peak);count++;
        }
        report.AppendLine("ALL PASS: "+count+" imported clips retain headroom");File.WriteAllText("../AudioInicial/NivelesImportados.txt",report.ToString());
    }
    public static void Run()
    {
        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var report=new StringBuilder();
        foreach(var path in Directory.GetFiles("Assets/Audio/Sonidos").Where(p=>!p.EndsWith(".meta")).OrderBy(p=>p))
        {
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            if(clip==null){report.AppendLine("NOT AUDIOCLIP: "+path+" ("+AssetDatabase.GetMainAssetTypeAtPath(path)?.Name+")");continue;}
            bool loaded=clip.LoadAudioData();report.AppendLine("IMPORTED: "+path+" | duration="+clip.length+" | rate="+clip.frequency+" | channels="+clip.channels+" | samples="+clip.samples+" | loaded="+loaded+" | state="+clip.loadState);
        }
        string output=Path.GetFileName(Directory.GetCurrentDirectory())=="AudioVerificationUnity"?"../AudioInicial":"Temp/AudioInicial";
        Directory.CreateDirectory(output);File.WriteAllText(output+"/ImportacionOriginales.txt",report.ToString());
    }
}
