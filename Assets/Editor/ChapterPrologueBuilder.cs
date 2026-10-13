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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Explicit authoring recipe. Run only once; never overwrites an existing chapter II.
public static class ChapterPrologueBuilder
{
    const string Forest="Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity";
    const string Trail="Assets/Scenes/Nivel02/N2_SenderoCabana.unity",Inside="Assets/Scenes/Nivel02/N2_CabanaGuardabosques.unity";
    const string Art="Assets/Art/Nivel02/",Story="Assets/Narrative/Nivel02/";
    static Dictionary<string,Sprite> trees,props;
    static Material lit,unlit,outline;
    static Transform leo;
    static NarrativeDirector director;
    static Font font;
    static Sprite doorSprite;
    static DialogueSequence[] radio;
    static DialogueSequence doorDialogue;
    static void Set(UnityEngine.Object obj,Action<SerializedObject> action){var s=new SerializedObject(obj);action(s);s.ApplyModifiedPropertiesWithoutUndo();}
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||File.Exists(Trail)||File.Exists(Inside))throw new InvalidOperationException("El capítulo II ya existe o Play está activo; no se sobrescribirá.");
        Directory.CreateDirectory(Art);Directory.CreateDirectory(Story);Directory.CreateDirectory("Assets/Scenes/Nivel02");
        trees=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png").OfType<Sprite>().ToDictionary(s=>s.name);
        props=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png").OfType<Sprite>().ToDictionary(s=>s.name);
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");unlit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalUnlit.mat");
        outline=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel01/GeneratorPolish/Contorno.mat");font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        MakeNarrative();MakeArt();
        var forest=EditorSceneManager.OpenScene(Forest);Bind();
        if(leo.GetComponent<CinematicWalk>()==null)leo.gameObject.AddComponent<CinematicWalk>();
        var roots=UnityEngine.Object.FindAnyObjectByType<RootsEncounter>();
        var ending=roots.gameObject.AddComponent<ChapterOneEnding>();var cues=roots.gameObject.AddComponent<ChapterAudio>();
        var branch=Prop(null,"Movimiento tenue entre ramas del cierre",trees["Arbusto"],new Vector2(10.4f,-8.4f),.38f);branch.color=new Color(.38f,.43f,.45f,.7f);
        CreateClosing(ending);
        Set(ending,s=>{s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>();s.FindProperty("creature").objectReferenceValue=UnityEngine.Object.FindAnyObjectByType<Escarbador>();s.FindProperty("roots").objectReferenceValue=roots;s.FindProperty("audioCues").objectReferenceValue=cues;s.FindProperty("subtleBranches").objectReferenceValue=branch.transform;});
        Set(roots,s=>s.FindProperty("chapterEnding").objectReferenceValue=ending);EditorSceneManager.SaveScene(forest);
        var trail=CloneSystems(false);CreateTrail();EditorSceneManager.SaveScene(trail,Trail);
        // Reload the forest as the unchanged template for the compact vestibule systems.
        EditorSceneManager.OpenScene(Forest);Bind();var inside=CloneSystems(true);CreateInterior();EditorSceneManager.SaveScene(inside,Inside);
        var list=EditorBuildSettings.scenes.ToList();list.Add(new EditorBuildSettingsScene(Trail,true));list.Add(new EditorBuildSettingsScene(Inside,true));EditorBuildSettings.scenes=list.ToArray();
        AssetDatabase.SaveAssets();Directory.CreateDirectory("../Capitulos");File.WriteAllText("../Capitulos/Construccion.txt","PASS: original chapter I closing wired; two chapter II scenes, dialogues, door/window, arrivals and checkpoints authored; original build list retained\n");
    }
    static void Bind(){leo=UnityEngine.Object.FindAnyObjectByType<LeoMovement>().transform;director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();}
    static Scene CloneSystems(bool interior)
    {
        var source=SceneManager.GetActiveScene();
        var target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var system=UnityEngine.Object.Instantiate(leo.root.gameObject);system.name="Sistemas_Leo";SceneManager.MoveGameObjectToScene(system,target);
        var cam=UnityEngine.Object.Instantiate(Camera.main.gameObject);cam.name="Main Camera";SceneManager.MoveGameObjectToScene(cam,target);
        var narrative=UnityEngine.Object.Instantiate(director.gameObject);narrative.name="Narrativa_CapituloII";SceneManager.MoveGameObjectToScene(narrative,target);
        var battery=UnityEngine.Object.Instantiate(UnityEngine.Object.FindAnyObjectByType<BatteryUI>().gameObject);battery.name="Interfaz_Bateria";SceneManager.MoveGameObjectToScene(battery,target);
        var light=UnityEngine.Object.Instantiate(GameObject.Find("Global Light 2D"));light.name="Global Light 2D";SceneManager.MoveGameObjectToScene(light,target);
        if(!interior){var fog=UnityEngine.Object.Instantiate(UnityEngine.Object.FindAnyObjectByType<ForestFogController>().gameObject);fog.name="Niebla_SenderoCabana";SceneManager.MoveGameObjectToScene(fog,target);}
        EditorSceneManager.CloseScene(source,true);SceneManager.SetActiveScene(target);
        leo=system.GetComponentInChildren<LeoMovement>().transform;director=narrative.GetComponent<NarrativeDirector>();
        Vector2 start=interior?new Vector2(-1,-1.6f):new Vector2(-12,-13);leo.position=start;leo.GetComponent<Rigidbody2D>().position=start;
        cam.GetComponent<SmoothCameraFollow>().Configure(leo);cam.transform.position=new Vector3(start.x,start.y,-10);
        Set(cam.GetComponent<ForestCameraBounds>(),s=>s.FindProperty("decoratedBounds").rectValue=interior?new Rect(-10,-7,20,14):new Rect(-24,-20,48,40));
        if(interior)cam.GetComponent<PixelPerfectCamera>().assetsPPU=64;
        Set(leo.GetComponentInChildren<FlashlightController>(),s=>s.FindProperty("worldCamera").objectReferenceValue=cam.GetComponent<Camera>());
        Set(leo.GetComponent<SurvivalPlayer>(),s=>{s.FindProperty("forestCheckpoint").boolValue=true;s.FindProperty("safeCheckpoint").vector2Value=start;});
        Set(system.GetComponentInChildren<SurvivalHUD>(),s=>s.FindProperty("player").objectReferenceValue=leo.GetComponent<SurvivalPlayer>());
        Set(battery.GetComponent<BatteryUI>(),s=>{s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});
        Set(director,s=>{s.FindProperty("movement").objectReferenceValue=leo.GetComponent<LeoMovement>();s.FindProperty("allowInteriorEvents").boolValue=false;s.FindProperty("allowGeneratorEvents").boolValue=false;});
        Set(narrative.GetComponent<PlayerInteraction>(),s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("targets").arraySize=0;});
        light.GetComponent<Light2D>().intensity=interior?.5f:.53f;light.GetComponent<Light2D>().color=new Color(.59f,.68f,.8f);
        var cues=narrative.AddComponent<ChapterAudio>();
        var sound=(UnityEvent<string>)typeof(NarrativeDirector).GetField("onSoundCue",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(director);
        UnityEventTools.AddPersistentListener(sound,cues.PlayCue);
        return target;
    }
    static void MakeNarrative()
    {
        radio=new[]{Dialogue("Radio_Llamada","LEO","Central, ¿me reciben?"),Dialogue("Radio_Hallazgo","LEO","Encontré algo en el bosque. No sé qué demonios era."),
            Dialogue("Radio_Advertencia","RADIO","...No sigas... el sendero..."),Dialogue("Radio_Corte","LEO","¿Hola? ¿Quién habla?")};
        doorDialogue=ScriptableObject.CreateInstance<DialogueSequence>();doorDialogue.lines=new[]{
            new DialogueLine{speaker="DESCONOCIDO",text="¿Vienes de la estación?",soundCue="stranger_station"},
            new DialogueLine{speaker="LEO",text="Soy el técnico de telecomunicaciones. ¿Quién está ahí?",soundCue="leo_technician"},
            new DialogueLine{speaker="DESCONOCIDO",text="Entonces... todavía no saben lo que pasó.",soundCue="stranger_unsaid"}};
        AssetDatabase.CreateAsset(doorDialogue,Story+"Puerta_Desconocido.asset");
    }
    static DialogueSequence Dialogue(string name,string speaker,string text)
    {var asset=ScriptableObject.CreateInstance<DialogueSequence>();asset.lines=new[]{new DialogueLine{speaker=speaker,text=text}};AssetDatabase.CreateAsset(asset,Story+name+".asset");return asset;}
    static void MakeArt()
    {
        doorSprite=Sprite.Create(trees["Caseta_Reten"].texture,new Rect(679,650,65,113),Vector2.zero,64);doorSprite.name="Puerta original de la cabaña";AssetDatabase.CreateAsset(doorSprite,Art+"PuertaCabana.asset");
        BakeWindow();
        Bake(Art+"UmbralOscuro.png",32,64,64,(x,y)=>new Color(.025f+y/64f*.008f,.035f,.036f,1));
        Bake(Art+"SueloSenderoCabana.png",1536,1280,32,(x,y)=>
        {
            Vector2 p=new Vector2((x+.5f)/32-24,(y+.5f)/32-20);float grain=(float)ForestFogField.Hash(x/2,y/2),broad=Mathf.PerlinNoise(p.x*.2f+25,p.y*.2f+60);
            float path=Mathf.SmoothStep(0,1,(1.55f+(Mathf.PerlinNoise(p.x*2+40,p.y*2+50)-.5f)*.65f-CabinTrailField.PathDistance(p))/.8f);
            path=Mathf.Max(path,Mathf.Clamp01(1-(p-new Vector2(6.2f,8)).magnitude/3.8f)*.7f);
            float detail=(float)ForestFogField.Hash(x,y)<.1f?.76f:1;
            return new Color((30+path*31+broad*10+grain*7)/255f*detail,(40+path*10+broad*12+grain*7)/255f*detail,(36+path*3+broad*10+grain*6)/255f*detail,1);
        });
        Bake(Art+"NieblaSenderoCabana.png",768,640,16,(x,y)=>
        {Vector2 p=new Vector2((x+.5f)/16-24,(y+.5f)/16-20);float edge=Mathf.Clamp01(Mathf.Min((24-Mathf.Abs(p.x))/4,(20-Mathf.Abs(p.y))/4));return new Color(1,1,1,(.15f+Mathf.PerlinNoise(p.x*.2f+32,p.y*.24f+11)*.5f)*CabinTrailField.FogDensity(p)*edge);},false);
        BakeInteriorFloor();
    }
    static void BakeInteriorFloor()
    {
        Bake(Art+"SueloVestibulo.png",1280,896,64,(x,y)=>
        {
            int row=y/16,column=(x+row%2*40)/80;
            float grain=(float)ForestFogField.Hash(x/3,y/2),shift=(float)ForestFogField.Hash(column,row)*.026f;
            float fibre=Mathf.Sin(x*.24f+Mathf.Sin(y*.3f+row)*1.5f)*.006f;
            bool seam=y%16==0||(x+row%2*40)%80==0;
            return seam?new Color(.115f,.089f,.058f):new Color(.175f+grain*.015f+shift+fibre,.135f+grain*.012f+shift*.8f+fibre,.087f+grain*.01f+shift*.6f+fibre);
        });
    }
    static void BakeWindow()
    {
        Bake(Art+"VentanaCalida.png",44,44,64,(x,y)=>
            x<4||x>39||y<4||y>39||x>=20&&x<=23||y>=20&&y<=23?Color.clear:
            new Color(1,.63f,.3f,.28f+(float)ForestFogField.Hash(x/2,y/2)*.12f));
    }
    public static void RefineWindow()
    {
        if(!File.Exists(Trail))throw new InvalidOperationException("Primero crear el prólogo.");
        BakeWindow();var scene=EditorSceneManager.OpenScene(Trail);
        var glow=GameObject.Find("Cristales con luz tenue");glow.transform.position=new Vector3(7+.555f*.86f,8.5f+1.311f*.86f,0);
        GameObject.Find("Luz cálida de la ventana").transform.position=glow.transform.position;
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
    public static void RefineVestibule()
    {
        if(!File.Exists(Inside))throw new InvalidOperationException("Primero crear el prólogo.");
        BakeInteriorFloor();var scene=EditorSceneManager.OpenScene(Inside);
        var stove=GameObject.Find("Estufa apagada");if(stove!=null)stove.name="Estufa del vestíbulo";
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
    }
    static Sprite Bake(string path,int w,int h,int ppu,Func<int,int,Color> pixel,bool point=true)
    {
        var texture=new Texture2D(w,h,TextureFormat.RGBA32,false);var data=new Color[w*h];for(int y=0;y<h;y++)for(int x=0;x<w;x++)data[y*w+x]=pixel(x,y);
        texture.SetPixels(data);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=ppu;
        i.filterMode=point?FilterMode.Point:FilterMode.Bilinear;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=4096;i.alphaIsTransparency=true;i.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static SpriteRenderer Prop(Transform parent,string name,Sprite sprite,Vector2 p,float scale,bool solid=false)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer),typeof(YSort));if(parent!=null)obj.transform.SetParent(parent,false);obj.transform.position=p;obj.transform.localScale=Vector3.one*scale;
        var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;r.sortingOrder=Mathf.RoundToInt(-p.y*32);
        if(solid){var c=obj.AddComponent<BoxCollider2D>();c.offset=Vector2.up*.14f/scale;c.size=new Vector2(Mathf.Min(.95f,r.bounds.size.x*.7f)/scale,.30f/scale);}
        return r;
    }
    static void Ground(SpriteRenderer r,int order=-1800){UnityEngine.Object.DestroyImmediate(r.GetComponent<YSort>());r.sortingOrder=order;}
    static void Boundary(Transform parent,string name,Vector2 p,Vector2 size){var obj=new GameObject(name,typeof(BoxCollider2D));obj.transform.SetParent(parent,false);obj.transform.position=p;obj.GetComponent<BoxCollider2D>().size=size;}
    static void Arrival(string name,Vector2 p)
    {
        var obj=new GameObject("Aparicion_"+name,typeof(RetenArrival));obj.transform.position=p;
        Set(obj.GetComponent<RetenArrival>(),s=>{s.FindProperty("spawnId").stringValue=name;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("worldCamera").objectReferenceValue=Camera.main;s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});
    }
    static void Target(WorldInteraction target,SpriteRenderer art)
    {
        var o=art.gameObject.AddComponent<InteractionOutline>();Set(o,s=>{s.FindProperty("artwork").objectReferenceValue=art;s.FindProperty("target").objectReferenceValue=target;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("director").objectReferenceValue=director;s.FindProperty("outlineMaterial").objectReferenceValue=outline;});
        Set(director.GetComponent<PlayerInteraction>(),s=>{var a=s.FindProperty("targets");int n=a.arraySize;a.arraySize++;a.GetArrayElementAtIndex(n).objectReferenceValue=target;});
    }
    static void CreateTrail()
    {
        Ground(Prop(null,"Suelo del sendero de la cabaña",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"SueloSenderoCabana.png"),Vector2.zero,1));
        var forest=new GameObject("Arboledas densas del capítulo II").transform;
        for(int row=0;row<16;row++)for(int col=0;col<22;col++)
        {
            int seed=row*22+col;Vector2 p=new Vector2(-24+col*2.3f+(float)ForestFogField.Hash(seed,51)*1.2f,-20+row*2.7f+(float)ForestFogField.Hash(seed,63)*1.3f);
            float density=Mathf.PerlinNoise(p.x*.19f+12,p.y*.16f+47);
            if(density<.32f||CabinTrailField.PathDistance(p)<2.8f||p.x>3.3f&&p.x<10.5f&&p.y>5&&p.y<12)continue;
            var tree=Prop(forest,"Grupo de pinos antiguos "+seed,trees[seed%5==0?"Arbol_Frondoso":seed%2==0?"Pino_Alto":"Pino_Mediano"],p,.69f+seed%4*.10f,true);tree.flipX=seed%2==0;tree.color=new Color(.73f,.8f,.81f);
            if(seed%2==0)Prop(forest,"Maleza agrupada",trees["Arbusto"],p+new Vector2(.8f,-.55f),.28f+seed%3*.06f);
            if(seed%7==0)Prop(forest,"Rocas húmedas",trees["Roca_Musgo"],p+Vector2.left*.8f,.20f,true);
            if(seed%11==0)Prop(forest,"Tronco antiguo",trees["Tronco_Caido"],p+Vector2.down*.6f,.28f,true);
        }
        Boundary(forest,"Límite forestal oeste",new Vector2(-17,0),new Vector2(.8f,30));Boundary(forest,"Límite forestal este",new Vector2(17,0),new Vector2(.8f,30));
        Boundary(forest,"Límite forestal sur",new Vector2(0,-15),new Vector2(34,.8f));Boundary(forest,"Límite forestal norte",new Vector2(0,15),new Vector2(34,.8f));
        var details=new GameObject("Raíces y hojarasca de los márgenes").transform;var leaves=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Nivel01/VisualPolish/Hojarasca.png");
        for(int i=0;i<170;i++)
        {
            Vector2 p=new Vector2(Mathf.Lerp(-16,16,(float)ForestFogField.Hash(i,117)),Mathf.Lerp(-14,14,(float)ForestFogField.Hash(i,133)));
            float distance=CabinTrailField.PathDistance(p);if(distance<1.25f||distance>4.2f)continue;
            var r=Prop(details,"Hojas en tierra nocturna",leaves,p,.9f+i%3*.2f);Ground(r,-1785);
            if(i%4==0){var roots=Prop(details,"Raíces junto al sendero",trees["Raices"],p,.22f);Ground(roots,-1780);}
            if(i%5==0)Prop(details,"Brotes húmedos",trees["Arbusto"],p,.16f);
        }
        var fog=UnityEngine.Object.FindAnyObjectByType<ForestFogController>();Set(fog,s=>
        {s.FindProperty("useCabinTrailDensity").boolValue=true;s.FindProperty("useAbandonedForestDensity").boolValue=false;s.FindProperty("useExteriorDensity").boolValue=false;s.FindProperty("bankArea").rectValue=new Rect(-17,-15,34,30);s.FindProperty("bankCount").intValue=18;});
        var ambient=(SpriteRenderer)new SerializedObject(fog).FindProperty("ambient").objectReferenceValue;ambient.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"NieblaSenderoCabana.png");ambient.transform.localScale=Vector3.one;
        Arrival("capitulo2_inicio",new Vector2(-12,-13));Arrival("sendero_cabana_puerta",new Vector2(6.15f,7.5f));
        CreateCabin();
    }
    static void CreateCabin()
    {
        var root=new GameObject("Cabaña del guardabosques").transform;Vector2 origin=new Vector2(7,8.5f);float scale=.86f;
        var house=Prop(root,"Madera y techo envejecidos",trees["Caseta_Reten"],origin,scale);house.color=new Color(.85f,.88f,.87f);
        Boundary(root,"Pared sólida de la cabaña",new Vector2(7,10.45f),new Vector2(3.8f,3.1f));
        for(int i=0;i<3;i++)Prop(root,"Leña junto a la pared",trees["Tronco_Caido"],new Vector2(3.6f,9.2f+i*.30f),.30f,true);
        Prop(root,"Herramientas junto a la leña",props["Mochila_Herramientas"],new Vector2(3.9f,7.8f),.27f);
        var fence=Prop(root,"Cerca rota izquierda",trees["Cerca_Madera"],new Vector2(2.8f,6.4f),.5f,true);fence.transform.rotation=Quaternion.Euler(0,0,-4);
        Prop(root,"Cerca derecha cubierta de musgo",trees["Cerca_Madera"],new Vector2(10.2f,7.2f),.48f,true);
        foreach(var p in new[]{new Vector2(3.1f,6.1f),new Vector2(10.1f,6.5f),new Vector2(9.4f,9.3f),new Vector2(4.5f,11.7f)})Prop(root,"Vegetación de la cabaña",trees["Arbusto"],p,.35f);
        var glow=Prop(root,"Cristales con luz tenue",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"VentanaCalida.png"),origin+new Vector2(.555f,1.311f)*scale,scale);
        UnityEngine.Object.DestroyImmediate(glow.GetComponent<YSort>());glow.sortingOrder=house.sortingOrder+1;glow.sharedMaterial=unlit;
        var lampObject=new GameObject("Luz cálida de la ventana",typeof(Light2D));lampObject.transform.SetParent(root,false);lampObject.transform.position=glow.transform.position;
        var lamp=lampObject.GetComponent<Light2D>();lamp.lightType=Light2D.LightType.Point;lamp.color=new Color(1,.58f,.24f);lamp.intensity=.32f;lamp.pointLightOuterRadius=2.1f;lamp.pointLightInnerRadius=.15f;
        var story=new GameObject("Prólogo del capítulo II",typeof(ChapterTwoPrologue)).GetComponent<ChapterTwoPrologue>();
        Set(story,s=>{s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>();s.FindProperty("director").objectReferenceValue=director;s.FindProperty("audioCues").objectReferenceValue=director.GetComponent<ChapterAudio>();s.FindProperty("windowLight").objectReferenceValue=lamp;s.FindProperty("windowGlow").objectReferenceValue=glow;var a=s.FindProperty("radioLines");a.arraySize=radio.Length;for(int i=0;i<radio.Length;i++)a.GetArrayElementAtIndex(i).objectReferenceValue=radio[i];});
        Vector2 lowerLeft=origin+new Vector2((679-632-139.5f)/64,(650-583-44.1f)/64)*scale;
        var black=Prop(root,"Oscuridad tras el umbral",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"UmbralOscuro.png"),lowerLeft+new Vector2(65,113)/128*scale,1);
        UnityEngine.Object.DestroyImmediate(black.GetComponent<YSort>());black.sortingOrder=house.sortingOrder+1;black.sharedMaterial=unlit;black.transform.localScale=new Vector3(65/32f*scale,113/64f*scale,1);
        var doorRoot=new GameObject("Puerta de la cabaña",typeof(CabinDoor));doorRoot.transform.SetParent(root,false);doorRoot.transform.position=lowerLeft+Vector2.right*65/128*scale;
        var hinge=new GameObject("Bisagra de madera").transform;hinge.SetParent(root,false);hinge.position=lowerLeft;
        var leaf=Prop(hinge,"Hoja original de la puerta",doorSprite,lowerLeft,scale);UnityEngine.Object.DestroyImmediate(leaf.GetComponent<YSort>());leaf.sortingOrder=house.sortingOrder+2;
        var door=doorRoot.GetComponent<CabinDoor>();Set(door,s=>{s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>();s.FindProperty("story").objectReferenceValue=story;s.FindProperty("audioCues").objectReferenceValue=director.GetComponent<ChapterAudio>();s.FindProperty("conversation").objectReferenceValue=doorDialogue;s.FindProperty("doorLeaf").objectReferenceValue=hinge;});Target(door,leaf);
    }
    static void CreateInterior()
    {
        Ground(Prop(null,"Tablones continuos del vestíbulo",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"SueloVestibulo.png"),Vector2.zero,1));
        var root=new GameObject("Vestíbulo de la cabaña").transform;
        foreach(float x in new[]{-3f,0f,3f}){var wall=Prop(root,"Pared de madera húmeda",props["Pared_Madera_Humedad"],new Vector2(x,2.2f),.7f);Ground(wall,-200);}
        Boundary(root,"Pared norte",new Vector2(0,2.6f),new Vector2(8,.35f));Boundary(root,"Pared sur",new Vector2(0,-2.8f),new Vector2(8,.35f));
        Boundary(root,"Pared oeste",new Vector2(-4,0),new Vector2(.35f,6));Boundary(root,"Pared este",new Vector2(4,0),new Vector2(.35f,6));
        Prop(root,"Mesa junto al umbral",props["Mesa_Papeles_Taza"],new Vector2(2.3f,.5f),.33f,true);
        Prop(root,"Herramientas olvidadas del vestíbulo",props["Mochila_Herramientas"],new Vector2(2.4f,-.6f),.25f);
        Prop(root,"Estufa del vestíbulo",props["Estufa_Metalica"],new Vector2(-2.8f,1.1f),.38f,true);
        Prop(root,"Ventana cerrada",props["Ventana_Nocturna"],new Vector2(.7f,2.1f),.33f);
        var doorArt=Prop(root,"Puerta hacia el sendero",props["Puerta_Madera"],new Vector2(-1,-2.7f),.36f);
        var back=new GameObject("Regreso al sendero",typeof(RetenDoor));back.transform.position=new Vector2(-1,-2.1f);
        Set(back.GetComponent<RetenDoor>(),s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("targetScene").stringValue=Trail;s.FindProperty("targetSpawn").stringValue="sendero_cabana_puerta";s.FindProperty("prompt").stringValue="[E] Volver al sendero";s.FindProperty("approachDirection").vector2Value=Vector2.up;s.FindProperty("interactionRange").floatValue=1.3f;});Target(back.GetComponent<RetenDoor>(),doorArt);
        Arrival("cabana_umbral",new Vector2(-1,-1.6f));
        var entry=new GameObject("Cruzar el umbral",typeof(CabinVestibule)).GetComponent<CabinVestibule>();Set(entry,s=>s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>());
    }
    static void CreateClosing(ChapterOneEnding ending)
    {
        var root=new GameObject("Cierre cinematográfico del capítulo I",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=31000;
        var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var black=Rect(root.transform,"Fundido del cierre",0,0,1920,1080);Stretch(black);black.gameObject.AddComponent<Image>().color=Color.black;var blackout=black.gameObject.AddComponent<CanvasGroup>();
        var card=Rect(black,"Capítulo I completado",0,0,1920,1080);Stretch(card);var group=card.gameObject.AddComponent<CanvasGroup>();
        var bg=Rect(card,"Bosque nocturno del cierre",0,0,1920,1080);Stretch(bg);var picture=bg.gameObject.AddComponent<RawImage>();picture.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Menu/Menu_FondoLimpio.png");picture.color=new Color(.52f,.59f,.64f);picture.raycastTarget=false;
        var ratio=bg.gameObject.AddComponent<AspectRatioFitter>();ratio.aspectMode=AspectRatioFitter.AspectMode.EnvelopeParent;ratio.aspectRatio=picture.texture.width/(float)picture.texture.height;
        var vignette=Rect(card,"Oscurecimiento de bordes",0,0,1920,1080);Stretch(vignette);vignette.gameObject.AddComponent<ChapterVignetteGraphic>();
        var design=Rect(card,"Composición del cierre",0,0,1920,1080);design.anchorMin=design.anchorMax=design.pivot=new Vector2(.5f,.5f);design.anchoredPosition=Vector2.zero;design.gameObject.AddComponent<MenuDesignScale>();
        var atmosphere=Rect(design,"Niebla y baliza distante",0,0,1920,1080);atmosphere.gameObject.AddComponent<MenuAtmosphereGraphic>();
        Label(design,"FRECUENCIA CERO",340,820,1240,62,27,new Color(.75f,.81f,.83f));
        Label(design,"EL SENDERO DEL RETÉN",260,704,1400,110,59,new Color(.91f,.93f,.91f),FontStyle.Bold);
        Label(design,"Capítulo I completado",340,640,1240,55,26,new Color(.8f,.38f,.35f));
        Label(design,"«Hay cosas en este bosque que prefieren permanecer ocultas».",310,468,1300,100,30,new Color(.8f,.84f,.84f),FontStyle.Italic);
        var buttonRect=Rect(design,"CONTINUAR",760,292,400,70);var image=buttonRect.gameObject.AddComponent<Image>();image.color=new Color(0,0,0,.58f);var button=buttonRect.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
        var highlight=buttonRect.gameObject.AddComponent<MenuButtonHighlight>();highlight.label=Label(buttonRect,"CONTINUAR",62,0,330,70,27,new Color(.8f,.79f,.75f));highlight.marker=Label(buttonRect,"▶",14,0,45,70,20,Color.clear);
        var edge=Rect(buttonRect,"Borde de selección",0,0,3,70).gameObject.AddComponent<Image>();highlight.border=edge;
        var border=Rect(buttonRect,"Contorno",0,0,400,70).gameObject.AddComponent<MenuBorderGraphic>();border.edge=edge;border.raycastTarget=false;
        var nav=button.navigation;nav.mode=Navigation.Mode.Explicit;nav.selectOnUp=nav.selectOnDown=button;button.navigation=nav;
        var events=new GameObject("EventSystem del cierre",typeof(EventSystem),typeof(InputSystemUIInputModule));events.transform.SetParent(root.transform,false);events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        Set(ending,s=>{s.FindProperty("view").objectReferenceValue=canvas;s.FindProperty("blackout").objectReferenceValue=blackout;s.FindProperty("closingCard").objectReferenceValue=group;s.FindProperty("continueButton").objectReferenceValue=button;});
        root.SetActive(false);
    }
    static RectTransform Rect(Transform p,string name,float x,float y,float w,float h)
    {var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(p,false);var r=obj.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);return r;}
    static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);}
    static Text Label(Transform p,string content,float x,float y,float w,float h,int size,Color tint,FontStyle style=FontStyle.Normal)
    {var text=Rect(p,content,x,y,w,h).gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.text=content;text.color=tint;text.fontStyle=style;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;return text;}
}
