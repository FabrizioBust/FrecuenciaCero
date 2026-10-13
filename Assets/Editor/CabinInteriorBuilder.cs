using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Explicit authoring pass. No automatic import hook; existing additional rooms are protected.
public static class CabinInteriorBuilder
{
    const string Scenes="Assets/Scenes/Nivel02/", Art="Assets/Art/Nivel02/Interiores/", Story="Assets/Narrative/Nivel02/Cabana/";
    const string Main="N2_CabanaGuardabosques", Trail="N2_SenderoCabana";
    static readonly string[] Rooms={Main,"N2_CabanaCocina","N2_CabanaComedor","N2_CabanaBano","N2_CabanaDespacho","N2_CabanaDormitorio","N2_CabanaAlmacen"};
    static readonly string[] Furniture={"Sillon","CamaPersonal","ArmarioRopa","MesitaLampara","CocinaLena","Fregadero","Alacena","Preparacion","MesaDosTazas","Vajillero","Inodoro","Lavabo","Banera","EspejoToalla","BotiquinPared","PercheroBotas"};
    static readonly string[] Details={"Biblioteca","Fotografias","Cuadro","RadioDomestica","Baul","Lena","BotasRopa","Cortinas","HerramientasCuerda","CajasPiezas","Bidones","Sacos","Tuberias","ToallasEstante","Cuadernos","LamparaPared"};
    static Dictionary<string,Sprite> sprites;
    static Material lit,outline; static Transform leo,decor; static NarrativeDirector director; static Camera camera; static Sprite floorTexture;
    static void Set(UnityEngine.Object o,Action<SerializedObject> edit){var s=new SerializedObject(o);edit(s);s.ApplyModifiedPropertiesWithoutUndo();}
    public static void Apply()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Rooms.Skip(1).Any(n=>File.Exists(Scenes+n+".unity")))throw new InvalidOperationException("Detener Play. Las habitaciones adicionales existentes no se sobrescriben.");
        if(!File.Exists(Art+"MobiliarioCabana_Atlas.png")||!File.Exists(Art+"DetallesCabana_Atlas.png"))throw new InvalidOperationException("Faltan los dos atlas terminados de la cabaña.");
        Directory.CreateDirectory(Story);
        sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png").OfType<Sprite>().ToDictionary(s=>s.name);
        foreach(var pair in Slice(Art+"MobiliarioCabana_Atlas.png",Furniture).Concat(Slice(Art+"DetallesCabana_Atlas.png",Details)))sprites.Add(pair.Key,pair.Value);
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");outline=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel01/GeneratorPolish/Contorno.mat");
        floorTexture=LoadFloor();RedesignClosing();
        BuildRoom(0,11,6);BuildRoom(1,8,6);BuildRoom(2,8,6);BuildRoom(3,5.8f,5.2f);BuildRoom(4,9,7);BuildRoom(5,8,6);BuildRoom(6,6.5f,5.5f);
        var list=EditorBuildSettings.scenes.ToList();foreach(var room in Rooms.Skip(1))if(!list.Any(s=>s.path==Scenes+room+".unity"))list.Add(new EditorBuildSettingsScene(Scenes+room+".unity",true));EditorBuildSettings.scenes=list.ToArray();AssetDatabase.SaveAssets();
        File.WriteAllText("../ReconstruccionCabana/Construccion.txt","PASS: new original-image closing; seven furnished rooms; paired arrivals, doors, outlines, colliders, warm lighting, storage progress lock and persistent supplies authored.\n");
    }
    static Dictionary<string,Sprite> Slice(string path,string[] names)
    {
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.filterMode=FilterMode.Point;
        importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.isReadable=true;importer.maxTextureSize=4096;importer.SaveAndReimport();
        var t=AssetDatabase.LoadAssetAtPath<Texture2D>(path);var data=t.GetPixels32();int cw=t.width/4,ch=t.height/4;var result=new Dictionary<string,Sprite>();
        for(int i=0;i<16;i++)
        {
            int ox=i%4*cw,oy=(3-i/4)*ch,minx=cw,miny=ch,maxx=-1,maxy=-1;
            for(int y=0;y<ch;y++)for(int x=0;x<cw;x++)if(data[(oy+y)*t.width+ox+x].a>24){minx=Math.Min(minx,x);maxx=Math.Max(maxx,x);miny=Math.Min(miny,y);maxy=Math.Max(maxy,y);}
            if(maxx<0)throw new InvalidOperationException("Celda vacía: "+names[i]);
            var sprite=Sprite.Create(t,new Rect(ox+minx,oy+miny,maxx-minx+1,maxy-miny+1),new Vector2(.5f,.06f),64,0,SpriteMeshType.FullRect);sprite.name=names[i];AssetDatabase.CreateAsset(sprite,Art+names[i]+".asset");result.Add(names[i],sprite);
        }
        return result;
    }
    static void RedesignClosing()
    {
        const string path="Assets/Art/ChapterClosing/CierreCapituloI_BosqueLimpio.png";
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType=TextureImporterType.Default;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.maxTextureSize=4096;importer.SaveAndReimport();
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity");
        var root=scene.GetRootGameObjects().Single(o=>o.name=="Cierre cinematográfico del capítulo I");var design=root.transform.Find("Fundido del cierre/Capítulo I completado/Composición del cierre");
        UnityEngine.Object.DestroyImmediate(design.Find("FRECUENCIA CERO").gameObject);
        UnityEngine.Object.DestroyImmediate(design.Find("Niebla y baliza distante").gameObject);
        var bg=root.GetComponentsInChildren<RawImage>(true).Single();bg.texture=AssetDatabase.LoadAssetAtPath<Texture2D>(path);bg.color=Color.white;bg.GetComponent<AspectRatioFitter>().aspectRatio=bg.texture.width/(float)bg.texture.height;
        var title=design.Find("EL SENDERO DEL RETÉN").GetComponent<Text>();Position(title.rectTransform,210,638,1500,110);title.fontSize=56;title.fontStyle=FontStyle.Bold;title.color=new Color(.78f,.77f,.70f);
        var subtitle=design.Find("Capítulo I completado").GetComponent<Text>();Position(subtitle.rectTransform,360,568,1200,55);subtitle.fontSize=28;subtitle.color=new Color(.65f,.66f,.61f);
        var quote=design.GetComponentsInChildren<Text>(true).Single(t=>t.text.StartsWith("«Hay cosas"));Position(quote.rectTransform,400,417,1120,110);quote.text="«Hay cosas en este bosque\nque prefieren permanecer ocultas».";quote.fontSize=29;quote.color=new Color(.63f,.66f,.63f);
        var button=design.GetComponentsInChildren<Button>(true).Single();Position(button.GetComponent<RectTransform>(),780,275,360,70);
        var old=button.GetComponent<MenuButtonHighlight>();var highlight=button.gameObject.AddComponent<ChapterContinueHighlight>();highlight.label=old.label;highlight.marker=old.marker;highlight.border=old.border;UnityEngine.Object.DestroyImmediate(old);
        Position(highlight.label.rectTransform,15,0,315,70);Position(highlight.marker.rectTransform,314,0,36,70);highlight.label.fontSize=25;
        button.GetComponentsInChildren<MenuBorderGraphic>(true).Single().rectTransform.sizeDelta=new Vector2(360,70);
        EditorSceneManager.SaveScene(scene);
    }
    static void Position(RectTransform r,float x,float y,float w,float h){r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
    static Scene Systems(int index,float width,float height)
    {
        var old=EditorSceneManager.OpenScene(Scenes+Main+".unity");var sourceLeo=UnityEngine.Object.FindAnyObjectByType<LeoMovement>().transform;
        var target=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var roots=new[]{sourceLeo.root.gameObject,Camera.main.gameObject,UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>().gameObject,UnityEngine.Object.FindAnyObjectByType<BatteryUI>().gameObject,GameObject.Find("Global Light 2D")};
        foreach(var o in roots){var copy=UnityEngine.Object.Instantiate(o);copy.name=o.name;SceneManager.MoveGameObjectToScene(copy,target);}EditorSceneManager.CloseScene(old,true);SceneManager.SetActiveScene(target);
        leo=UnityEngine.Object.FindAnyObjectByType<LeoMovement>().transform;director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();camera=Camera.main;
        var start=index==0?new Vector2(-1,-1.6f):new Vector2(0,-height/2+1.15f);leo.position=start;leo.GetComponent<Rigidbody2D>().position=start;
        camera.transform.position=new Vector3(start.x,start.y,-10);camera.GetComponent<SmoothCameraFollow>().Configure(leo);
        var pixel=camera.GetComponent<PixelPerfectCamera>();pixel.assetsPPU=32;pixel.refResolutionX=320;pixel.refResolutionY=180;
        Set(camera.GetComponent<ForestCameraBounds>(),s=>s.FindProperty("decoratedBounds").rectValue=new Rect(-width/2-.8f,-height/2-1,width+1.6f,height+2.6f));
        Set(leo.GetComponentInChildren<FlashlightController>(),s=>s.FindProperty("worldCamera").objectReferenceValue=camera);
        Set(leo.GetComponent<SurvivalPlayer>(),s=>{s.FindProperty("forestCheckpoint").boolValue=true;s.FindProperty("safeCheckpoint").vector2Value=start;});
        Set(leo.root.GetComponentInChildren<SurvivalHUD>(),s=>s.FindProperty("player").objectReferenceValue=leo.GetComponent<SurvivalPlayer>());
        var battery=UnityEngine.Object.FindAnyObjectByType<BatteryUI>();Set(battery,s=>{s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});
        Set(director,s=>{s.FindProperty("movement").objectReferenceValue=leo.GetComponent<LeoMovement>();s.FindProperty("allowInteriorEvents").boolValue=false;s.FindProperty("allowGeneratorEvents").boolValue=false;});
        Set(director.GetComponent<PlayerInteraction>(),s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("targets").arraySize=0;});
        var global=GameObject.Find("Global Light 2D").GetComponent<Light2D>();global.intensity=index==6?.19f:.28f;global.color=new Color(.66f,.68f,.72f);
        decor=new GameObject("Vivienda forestal — "+Rooms[index]).transform;
        if(index==0){var entry=new GameObject("Entrada desde el sendero",typeof(CabinVestibule)).GetComponent<CabinVestibule>();Set(entry,s=>s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>());Arrival("cabana_umbral",start);}
        else new GameObject("Contexto de la habitación",typeof(CabinRoomContext));
        return target;
    }
    static void BuildRoom(int index,float width,float height)
    {
        var scene=Systems(index,width,height);sprites["SueloDomestico"]=floorTexture;var floor=Prop("Suelo de madera doméstica detallada", "SueloDomestico",Vector2.zero,5.2f);Ground(floor,-2000);floor.transform.localScale=Vector3.one;floor.drawMode=SpriteDrawMode.Sliced;floor.size=new Vector2(width+1,height+1.2f);floor.color=new Color(.86f+index%3*.04f,.83f+index%2*.04f,.77f);
        Wall("Pared norte",new Vector2(0,height/2-.1f),new Vector2(width+1,1.2f));Wall("Pared sur",new Vector2(0,-height/2-.4f),new Vector2(width+1,.75f));
        Wall("Pared oeste",new Vector2(-width/2-.22f,0),new Vector2(.5f,height+1.8f));Wall("Pared este",new Vector2(width/2+.22f,0),new Vector2(.5f,height+1.8f));
        Boundary("Colisión norte",new Vector2(0,height/2),new Vector2(width,.25f));Boundary("Colisión sur",new Vector2(0,-height/2),new Vector2(width,.25f));Boundary("Colisión oeste",new Vector2(-width/2,0),new Vector2(.25f,height));Boundary("Colisión este",new Vector2(width/2,0),new Vector2(.25f,height));
        switch(index){case 0:Living();break;case 1:Kitchen();break;case 2:Dining();break;case 3:Bathroom();break;case 4:Office();break;case 5:Bedroom();break;case 6:Storage();break;}
        if(index!=3&&index!=6){Prop("Ventana: bosque nocturno", "Ventana_Nocturna",new Vector2(width/2-1.35f,height/2+.1f),.85f);}
        EditorSceneManager.SaveScene(scene,Scenes+Rooms[index]+".unity");
    }
    static SpriteRenderer Prop(string name,string key,Vector2 p,float width,float depth=0)
    {
        var sprite=sprites[key];float scale=width/(sprite.rect.width/sprite.pixelsPerUnit);var o=new GameObject(name,typeof(SpriteRenderer),typeof(YSort));o.transform.SetParent(decor,false);o.transform.position=p;o.transform.localScale=new Vector3(scale,scale,1);
        var r=o.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;r.sortingOrder=Mathf.RoundToInt(-p.y*32);
        if(depth>0){var c=o.AddComponent<BoxCollider2D>();c.offset=new Vector2(0,depth*.5f/scale);c.size=new Vector2(width*.82f/scale,depth/scale);}
        return r;
    }
    static void Ground(SpriteRenderer r,int order){UnityEngine.Object.DestroyImmediate(r.GetComponent<YSort>());r.sortingOrder=order;}
    static void Wall(string name,Vector2 p,Vector2 size){var r=Prop(name,"Pared_Madera_Humedad",p,3);Ground(r,-500);r.transform.localScale=new Vector3(.2625641f,.2625641f,1);r.drawMode=SpriteDrawMode.Tiled;r.size=size/.2625641f;}
    static Sprite LoadFloor()
    {
        string path=Art+"TablonesDomesticos.png";AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);var i=(TextureImporter)AssetImporter.GetAtPath(path);i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;i.spritePixelsPerUnit=128;i.filterMode=FilterMode.Point;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.maxTextureSize=4096;i.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    public static void RefineWood()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||Rooms.Any(n=>!File.Exists(Scenes+n+".unity")))throw new InvalidOperationException("Falta una habitación o Play está activo.");
        var floor=LoadFloor();
        foreach(var room in Rooms)
        {
            var scene=EditorSceneManager.OpenScene(Scenes+room+".unity");
            var renderer=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Single(r=>r.name=="Tablones envejecidos del retén"||r.name=="Suelo de madera doméstica detallada");
            var floorSize=renderer.size;renderer.name="Suelo de madera doméstica detallada";renderer.sprite=floor;renderer.drawMode=SpriteDrawMode.Sliced;renderer.size=floorSize;
            foreach(var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.name.StartsWith("Pared ")))
            {
                var size=Vector2.Scale(r.size,new Vector2(r.transform.localScale.x,r.transform.localScale.y));
                r.transform.localScale=new Vector3(.2625641f,.2625641f,1);r.drawMode=SpriteDrawMode.Tiled;r.size=size/.2625641f;
            }
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
    }
    public static void RefineDeskAccess()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Detener Play.");
        var scene=EditorSceneManager.OpenScene(Scenes+Rooms[4]+".unity");
        GameObject.Find("Examinar — Escritorio y cajones").transform.position=new Vector2(-.95f,.62f);
        EditorSceneManager.SaveScene(scene);
    }
    static void Boundary(string name,Vector2 p,Vector2 size){var o=new GameObject(name,typeof(BoxCollider2D));o.transform.SetParent(decor,false);o.transform.position=p;o.GetComponent<BoxCollider2D>().size=size;}
    static void Lamp(string name,Vector2 p,float strength,float radius,Color color){var o=new GameObject(name,typeof(Light2D));o.transform.SetParent(decor,false);o.transform.position=p;var l=o.GetComponent<Light2D>();l.lightType=Light2D.LightType.Point;l.color=color;l.intensity=strength;l.pointLightOuterRadius=radius;l.pointLightInnerRadius=.12f;}
    static void Warm(Vector2 p,float strength=.85f,float radius=3.4f)=>Lamp("Lámpara cálida localizada",p,strength,radius,new Color(1,.72f,.41f));
    static void Rug(Vector2 p,float width){var r=Prop("Alfombra doméstica desgastada","Alfombra_Desgastada",p,width);Ground(r,-1800);r.color=new Color(.8f,.74f,.7f);}
    static void Arrival(string id,Vector2 p){var o=new GameObject("Aparicion_"+id,typeof(RetenArrival));o.transform.SetParent(decor,false);o.transform.position=p;Set(o.GetComponent<RetenArrival>(),s=>{s.FindProperty("spawnId").stringValue=id;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("worldCamera").objectReferenceValue=camera;s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});}
    static void Target(WorldInteraction target,SpriteRenderer art){var o=art.gameObject.AddComponent<InteractionOutline>();Set(o,s=>{s.FindProperty("artwork").objectReferenceValue=art;s.FindProperty("target").objectReferenceValue=target;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("director").objectReferenceValue=director;s.FindProperty("outlineMaterial").objectReferenceValue=outline;});Set(director.GetComponent<PlayerInteraction>(),s=>{var a=s.FindProperty("targets");int n=a.arraySize;a.arraySize++;a.GetArrayElementAtIndex(n).objectReferenceValue=target;});}
    static void Door(string name,Vector2 p,string room,string arrival,string ownArrival,bool north=true,bool locked=false)
    {
        Vector2 approach=north?Vector2.down:Vector2.up;var art=Prop("Puerta — "+name,"Puerta_Madera",p-new Vector2(0,north?0:.6f),1.05f);
        var o=new GameObject("Acceso — "+name,typeof(CabinRoomDoor));o.transform.SetParent(decor,false);o.transform.position=p;
        Set(o.GetComponent<CabinRoomDoor>(),s=>{s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>();s.FindProperty("targetScene").stringValue=Scenes+room+".unity";s.FindProperty("targetSpawn").stringValue=arrival;s.FindProperty("destination").stringValue=name;s.FindProperty("approachDirection").vector2Value=approach;s.FindProperty("requiresStorageUnlock").boolValue=locked;});Target(o.GetComponent<CabinRoomDoor>(),art);if(ownArrival!=null)Arrival(ownArrival,p+approach*.9f);
    }
    static void Document(string id,string title,string body,SpriteRenderer artwork,Vector2? point=null)
    {
        var d=ScriptableObject.CreateInstance<DocumentDefinition>();d.title=title;d.body=body;AssetDatabase.CreateAsset(d,Story+id+".asset");
        var o=new GameObject("Examinar — "+title,typeof(ForestDocument));o.transform.SetParent(decor,false);o.transform.position=point??((Vector2)artwork.transform.position+Vector2.down*.38f);Set(o.GetComponent<ForestDocument>(),s=>{s.FindProperty("document").objectReferenceValue=d;s.FindProperty("range").floatValue=1.05f;});Target(o.GetComponent<ForestDocument>(),artwork);
    }
    static void Pickup(string id,SupplyKind kind,Vector2 p)
    {
        string path=kind==SupplyKind.Battery?"Assets/Art/Nivel01/Survival/Bateria.png":"Assets/Art/Nivel01/Survival/BotiquinRojo.png";
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(path);sprites[id]=sprite;var r=Prop("Suministro doméstico — "+id,id,p,.35f);var pickup=r.gameObject.AddComponent<SupplyPickup>();Set(pickup,s=>{s.FindProperty("uniqueId").stringValue=id;s.FindProperty("kind").intValue=(int)kind;s.FindProperty("artwork").objectReferenceValue=r;});Target(pickup,r);
    }
    static void Living()
    {
        Door("Entrar a la cocina",new Vector2(-3.4f,2.05f),Rooms[1],"cocina_sala","sala_cocina");Door("Entrar al despacho",new Vector2(0,2.05f),Rooms[4],"despacho_sala","sala_despacho");Door("Entrar al comedor",new Vector2(3.4f,2.05f),Rooms[2],"comedor_sala","sala_comedor");
        Door("Volver al sendero",new Vector2(-1,-2.1f),Trail,"sendero_cabana_puerta",null,false);
        Prop("Estufa de leña habitada","Estufa_Metalica",new Vector2(-4.6f,.3f),.85f,.55f);Lamp("Calor de la estufa",new Vector2(-4.5f,1.1f),.85f,3,new Color(1,.45f,.2f));
        Prop("Leña seca junto a la estufa","Lena",new Vector2(-4.4f,-.9f),.85f,.35f);Prop("Sillón antiguo de tela","Sillon",new Vector2(-2.7f,-.05f),1.25f,.65f);
        Rug(new Vector2(1,-.7f),3.4f);var table=Prop("Mesa auxiliar con radio","Mesa_Papeles_Taza",new Vector2(1.9f,-.15f),1.25f,.55f);Prop("Radio doméstica antigua","RadioDomestica",new Vector2(1.9f,.45f),.56f);
        Document("RadioSala","Radio vieja","Una radio doméstica de madera. El dial está gastado y el cable ha sido reparado. No se oye ninguna emisión.",table);
        Prop("Biblioteca doméstica y cajones","Biblioteca",new Vector2(4.5f,.4f),1.25f,.45f);Prop("Perchero, abrigo y botas embarradas","PercheroBotas",new Vector2(3.7f,-1.5f),1.15f,.3f);
        var photos=Prop("Fotografías familiares enmarcadas","Fotografias",new Vector2(-1.7f,2.9f),1.0f);Document("FotografiasSala","Fotografías antiguas","Fotografías de una vivienda y de varias personas. El tiempo ha borrado parte de las imágenes.",photos,new Vector2(-1.7f,1.85f));
        Prop("Cuadro del paisaje forestal","Cuadro",new Vector2(1.7f,2.95f),1.1f);Prop("Lámpara junto al sillón","LamparaPared",new Vector2(-2.6f,2.9f),.38f);Warm(new Vector2(1.7f,.8f));Warm(new Vector2(-2.5f,.6f),.65f,2.8f);
    }
    static void Kitchen()
    {
        Door("Volver a la sala",new Vector2(0,-2.1f),Main,"sala_cocina","cocina_sala",false);Door("Entrar al baño",new Vector2(-2.5f,2.0f),Rooms[3],"bano_cocina","cocina_bano");
        Prop("Cocina de leña, ollas y sartenes","CocinaLena",new Vector2(2.6f,1.25f),1.45f,.7f);Warm(new Vector2(2.6f,1.9f),1.0f,3.2f);
        Prop("Fregadero de esmalte y grifo","Fregadero",new Vector2(-.6f,1.25f),1.35f,.6f);Prop("Alacena: platos, frascos y alimentos","Alacena",new Vector2(1,1.35f),1.25f,.45f);
        var prep=Prop("Mesa de preparación con cuchillo decorativo","Preparacion",new Vector2(-2.25f,-.5f),1.6f,.65f);Document("PreparacionCocina","Mesa de preparación","Hay verduras cortadas y un cuchillo de cocina sobre la tabla. La cocina parece haberse usado recientemente.",prep);
        Prop("Sacos y alimentos almacenados","Sacos",new Vector2(2.8f,-.8f),.8f,.35f);Prop("Estante de utensilios","ToallasEstante",new Vector2(-3.3f,.55f),.65f);Warm(new Vector2(-.4f,1.6f),.65f,2.8f);Pickup("cabana_cocina_bateria_01",SupplyKind.Battery,new Vector2(2.3f,-1.7f));
    }
    static void Dining()
    {
        Door("Volver a la sala",new Vector2(0,-2.1f),Main,"sala_comedor","comedor_sala",false);Rug(new Vector2(0,-.6f),3.4f);
        var table=Prop("Mesa, mantel y dos tazas utilizadas","MesaDosTazas",new Vector2(0,-.05f),2.4f,1.0f);Document("DosTazas","Dos tazas","Dos tazas usadas descansan sobre el mantel desgastado. En una queda una marca oscura de café.",table,new Vector2(-1.25f,-.55f));
        Prop("Tercera silla de comedor","Silla_Madera",new Vector2(-.9f,1.35f),.55f,.35f);Prop("Cuarta silla de comedor","Silla_Madera",new Vector2(1.0f,-.85f),.55f,.35f);
        Prop("Armario de vajilla","Vajillero",new Vector2(-2.8f,1.1f),1.25f,.45f);Prop("Fotografías sobre el comedor","Fotografias",new Vector2(.3f,2.85f),1.25f);Prop("Mesa lateral con lámpara","MesitaLampara",new Vector2(2.85f,.5f),.85f,.35f);Warm(new Vector2(.1f,1.1f),.85f,3.5f);Warm(new Vector2(2.7f,1.2f),.65f,2.2f);
    }
    static void Bathroom()
    {
        Door("Volver a la cocina",new Vector2(0,-1.7f),Rooms[1],"cocina_bano","bano_cocina",false);
        Prop("Bañera antigua y ducha","Banera",new Vector2(-1.35f,.7f),1.85f,.8f);Prop("Lavabo de madera envejecida","Lavabo",new Vector2(1.4f,.9f),1.05f,.5f);Prop("Inodoro antiguo","Inodoro",new Vector2(1.9f,-.7f),.7f,.6f);
        Prop("Espejo y toalla gastada","EspejoToalla",new Vector2(1.45f,2.3f),.65f);Prop("Tuberías y válvulas","Tuberias",new Vector2(-2.35f,1.8f),.65f);Prop("Toallas en estantería","ToallasEstante",new Vector2(-2.1f,-.5f),.7f,.25f);
        var cabinet=Prop("Botiquín de pared con manchas de humedad","BotiquinPared",new Vector2(.1f,2.3f),.6f);Document("BotiquinPared","Botiquín de pared","Un pequeño armario de primeros auxilios. El metal presenta manchas de humedad y la pintura está descascarada.",cabinet,new Vector2(.1f,1.5f));
        Pickup("cabana_bano_botiquin_01",SupplyKind.Medkit,new Vector2(-1.6f,-1.4f));Warm(new Vector2(.6f,1.4f),.58f,2.4f);
    }
    static void Office()
    {
        Door("Volver a la sala",new Vector2(0,-2.6f),Main,"sala_despacho","despacho_sala",false);Door("Entrar al dormitorio",new Vector2(-2.65f,2.45f),Rooms[5],"dormitorio_despacho","despacho_dormitorio");Door("Entrar al almacén",new Vector2(2.8f,2.45f),Rooms[6],"almacen_despacho","despacho_almacen",true,true);
        var desk=Prop("Escritorio: radio, libros, cajones y lámpara","Escritorio_Radio_Bitacora",new Vector2(.05f,1.0f),2.65f,.75f);Document("CuadernoDespacho","Escritorio y cajones","Un cuaderno de trabajo descansa junto a la radio de comunicaciones. Los cajones tienen tiradores gastados. Hay anotaciones de mantenimiento y varias páginas en blanco.",desk,new Vector2(-.95f,.62f));
        Prop("Silla del escritorio","Silla_Madera",new Vector2(.05f,-.45f),.6f,.4f);var map=Prop("Mapas del bosque en la pared","Mapa_CimaDelAguila",new Vector2(0,3.4f),1.3f);Document("MapaDespacho","Mapa forestal","Un mapa antiguo del bosque, con senderos y cursos de agua. Tiene marcas de uso y esquinas dobladas.",map,new Vector2(-1.6f,1.7f));
        Prop("Biblioteca y archivadores de madera","Biblioteca",new Vector2(-3.5f,.2f),1.4f,.45f);Prop("Archivero metálico antiguo","Armario_Oxido",new Vector2(3.65f,.7f),.75f,.45f);Prop("Cuadernos y mapas doblados","Cuadernos",new Vector2(2.8f,-.9f),.7f);
        Prop("Fotografías en el despacho","Fotografias",new Vector2(-1.35f,3.25f),.8f);Prop("Linterna de mantenimiento y herramientas","Mochila_Herramientas",new Vector2(3.0f,-1.6f),.9f,.3f);Rug(new Vector2(0,-1.0f),2.65f);Warm(new Vector2(.5f,1.9f),1.15f,3.6f);Warm(new Vector2(-3.4f,1.0f),.45f,2.2f);
    }
    static void Bedroom()
    {
        Door("Volver al despacho",new Vector2(0,-2.1f),Rooms[4],"despacho_dormitorio","dormitorio_despacho",false);Prop("Cama doméstica con colcha gastada","CamaPersonal",new Vector2(-1.3f,.1f),2.2f,1.45f);Prop("Mesita de noche y lámpara","MesitaLampara",new Vector2(.55f,.95f),.8f,.35f);Warm(new Vector2(.55f,1.7f),.78f,3.1f);
        Prop("Armario con ropa personal","ArmarioRopa",new Vector2(2.65f,1.2f),1.0f,.45f);var trunk=Prop("Baúl de madera privado","Baul",new Vector2(2.3f,-.6f),1.15f,.5f);Document("BaulDormitorio","Baúl antiguo","Un baúl de madera con herrajes gastados. Conserva marcas de muchos años de uso.",trunk);
        Prop("Botas y ropa doblada","BotasRopa",new Vector2(-2.7f,-1.3f),.7f);Prop("Fotografías personales","Fotografias",new Vector2(-1.4f,2.9f),.85f);Prop("Cortinas envejecidas","Cortinas",new Vector2(2.65f,3.12f),1.15f);Rug(new Vector2(.15f,-1.0f),1.8f);
    }
    static void Storage()
    {
        Door("Volver al despacho",new Vector2(0,-1.85f),Rooms[4],"despacho_almacen","almacen_despacho",false);
        Prop("Estante de suministros y mantenimiento","Estanteria_Suministros",new Vector2(-2.15f,1.0f),1.3f,.45f);Prop("Cajas y piezas de maquinaria","CajasPiezas",new Vector2(-2.0f,-.5f),1.2f,.65f);Prop("Bidones y aceite","Bidones",new Vector2(1.9f,1.0f),1.2f,.65f);
        Prop("Cuerdas y herramientas colgadas","HerramientasCuerda",new Vector2(.05f,2.5f),1.45f);Prop("Sacos y frascos de suministros","Sacos",new Vector2(2.1f,-.7f),.95f,.4f);Prop("Madera seca almacenada","Lena",new Vector2(.2f,.5f),1.1f,.4f);Warm(new Vector2(-1.9f,1.7f),.48f,2.4f);
    }
}
