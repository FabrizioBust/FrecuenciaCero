using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Explicit authoring command, never automatically runs or overwrites an existing forest.
public static class LevelOneFinalBuilder
{
    const string Art="Assets/Art/Nivel01/Survival/", Forest="Assets/Scenes/Nivel01/N1_BosqueAbandonado.unity";
    static Dictionary<string,Sprite> trees,props;
    static Material lit,outline;
    static Transform leo;
    static NarrativeDirector director;
    static Sprite battery,bottle,medkit,wheelbarrow,creature,note;
    static List<WorldInteraction> extra;
    static string Reports => Path.Combine(Application.dataPath,"..","Temp","Nivel1Final");
    static void Report(string name,string content){Directory.CreateDirectory(Reports);File.WriteAllText(Path.Combine(Reports,name),content);}
    public static void Build()
    {
        if(File.Exists(Forest))throw new InvalidOperationException("El bosque ya existe; no se sobrescribirá.");
        Directory.CreateDirectory(Art);
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        outline=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel01/GeneratorPolish/Contorno.mat");
        trees=Sprites("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png");
        props=Sprites("Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png");
        CreateArt();
        var generator=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_Generador.unity");Bind();extra=new List<WorldInteraction>();
        if(Camera.main.GetComponent<ForestCameraBounds>()==null)
        {var bounds=Camera.main.gameObject.AddComponent<ForestCameraBounds>();Set(bounds,s=>s.FindProperty("decoratedBounds").rectValue=new Rect(-36,-28,72,56));}
        Supplies();SouthTrail();Systems(false);UniversalOutlines();AppendTargets();EditorSceneManager.SaveScene(generator);
        var forest=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var originalLeo=leo;
        var system=UnityEngine.Object.Instantiate(leo.root.gameObject);system.name="Sistemas_Leo";SceneManager.MoveGameObjectToScene(system,forest);
        var camera=UnityEngine.Object.Instantiate(Camera.main.transform.root.gameObject);camera.name="Main Camera";SceneManager.MoveGameObjectToScene(camera,forest);
        var originalLight=GameObject.Find("Global Light 2D");originalLight.SetActive(false);
        var light=UnityEngine.Object.Instantiate(originalLight);light.name="Global Light 2D";SceneManager.MoveGameObjectToScene(light,forest);
        light.GetComponent<Light2D>().intensity=.58f;
        var narrative=UnityEngine.Object.Instantiate(director.gameObject);narrative.name="Narrativa_Bosque";SceneManager.MoveGameObjectToScene(narrative,forest);
        var fog=UnityEngine.Object.Instantiate(GameObject.Find("Niebla_Exterior"));fog.name="Niebla_Bosque";SceneManager.MoveGameObjectToScene(fog,forest);
        EditorSceneManager.CloseScene(generator,true);SceneManager.SetActiveScene(forest);
        light.SetActive(true);
        leo=system.GetComponentInChildren<LeoMovement>().transform;leo.position=new Vector3(-8,12,0);
        leo.GetComponent<Rigidbody2D>().position=leo.position;
        director=narrative.GetComponent<NarrativeDirector>();Set(director,s=>{s.FindProperty("movement").objectReferenceValue=leo.GetComponent<LeoMovement>();s.FindProperty("allowGeneratorEvents").boolValue=false;s.FindProperty("allowInteriorEvents").boolValue=false;});
        camera.GetComponent<SmoothCameraFollow>().Configure(leo);camera.transform.position=new Vector3(-8,12,-10);
        Set(camera.GetComponent<ForestCameraBounds>(),s=>s.FindProperty("decoratedBounds").rectValue=new Rect(-32,-24,64,48));
        foreach(var ui in system.GetComponentsInChildren<SurvivalHUD>())Set(ui,s=>s.FindProperty("player").objectReferenceValue=leo.GetComponent<SurvivalPlayer>());
        var oldTargets=narrative.GetComponent<PlayerInteraction>();Set(oldTargets,s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("targets").arraySize=0;});
        Systems(true);ForestBatteryHUD();extra=new List<WorldInteraction>();BuildForest();UniversalOutlines();AppendTargets();
        EditorSceneManager.SaveScene(forest,Forest);
        foreach(string path in new[]{"Assets/Scenes/Nivel01/N1_ExteriorReten.unity","Assets/Scenes/Nivel01/N1_InteriorReten.unity"})
        {
            var scene=EditorSceneManager.OpenScene(path);Bind();Systems(false);UniversalOutlines();EditorSceneManager.SaveScene(scene);
        }
        var scenes=EditorBuildSettings.scenes.ToList();scenes.Add(new EditorBuildSettingsScene(Forest,true));EditorBuildSettings.scenes=scenes.ToArray();
        AssetDatabase.SaveAssets();
        RefineAndInspect();FinishInteractionDetails();
        Report("Construccion.txt","PASS: native scene, assets, survival systems and build integration authored\n");
    }
    static void Bind(){leo=UnityEngine.Object.FindAnyObjectByType<LeoMovement>().transform;director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();}
    static void ForestBatteryHUD()
    {
        var ui=new GameObject("Interfaz_Bateria",typeof(BatteryUI)).GetComponent<BatteryUI>();
        Set(ui,s=>{s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});
    }
    static void Set(UnityEngine.Object obj,Action<SerializedObject> edit){var s=new SerializedObject(obj);edit(s);s.ApplyModifiedPropertiesWithoutUndo();}
    static Dictionary<string,Sprite> Sprites(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
    public static void RefineAndInspect()
    {
        var scene=EditorSceneManager.OpenScene(Forest);Bind();
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        trees=Sprites("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png");
        if(UnityEngine.Object.FindAnyObjectByType<BatteryUI>()==null)ForestBatteryHUD();
        var paper=GameObject.Find("Nota entre materiales");paper.transform.position=new Vector3(-5.4f,8.6f,0);
        Set(paper.GetComponent<ForestDocument>(),s=>s.FindProperty("interactionOffset").vector2Value=Vector2.down*1.1f);
        var enemy=UnityEngine.Object.FindAnyObjectByType<Escarbador>();enemy.GetComponent<CircleCollider2D>().offset=Vector2.zero;
        var ground=GameObject.Find("Suelo continuo del bosque").GetComponent<SpriteRenderer>();
        Report("InspeccionVisual.txt","Ground: "+ground.bounds+" order "+ground.sortingOrder+" sprite "+ground.sprite.name+" material "+ground.sharedMaterial.name+" color "+ground.color+"\n");
        foreach(var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
            if(r.bounds.size.x>20)File.AppendAllText(Path.Combine(Reports,"InspeccionVisual.txt"),r.name+" "+r.bounds+" sort "+r.sortingOrder+" "+r.color+"\n");
        // Keep terrain crisp and readable under the existing night lighting.
        ground.sortingOrder=-1800;
        var fog=GameObject.Find("Niebla_Bosque");
        Set(fog.GetComponent<ForestFogController>(),s=>{s.FindProperty("generalDensity").floatValue=.55f;s.FindProperty("transparency").floatValue=.94f;});
        var cart=GameObject.Find("Carretilla oxidada");cart.transform.position=new Vector3(-5.4f,5.9f,0);
        if(GameObject.Find("Vegetacion del campamento y suelo del claro")==null)GroundDetails();
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Capture("campamento-refinado",new Vector3(-8,8,-10),1920,1080);
        fog.SetActive(false);
        Capture("campamento-sin-niebla",new Vector3(-8,8,-10),1920,1080);
    }
    static void Capture(string name,Vector3 point,int width,int height)
    {
        var camera=Camera.main;var originalTarget=camera.targetTexture;var originalPosition=camera.transform.position;
        var rt=new RenderTexture(width,height,24);rt.Create();camera.targetTexture=rt;camera.transform.position=point;
        camera.GetComponent<PixelPerfectCamera>()?.SendMessage("UpdateCameraProperties");
        RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});
        RenderPipeline.SubmitRenderRequest(camera,new RenderPipeline.StandardRequest{destination=rt});
        var previous=RenderTexture.active;RenderTexture.active=rt;var pixels=new Texture2D(width,height,TextureFormat.RGB24,false);
        pixels.ReadPixels(new Rect(0,0,width,height),0,0);pixels.Apply();Directory.CreateDirectory(Reports);File.WriteAllBytes(Path.Combine(Reports,name+".png"),pixels.EncodeToPNG());
        RenderTexture.active=previous;camera.targetTexture=originalTarget;camera.transform.position=originalPosition;
        rt.Release();UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(pixels);
    }
    static void GroundDetails()
    {
        var root=new GameObject("Vegetacion del campamento y suelo del claro").transform;
        for(int i=0;i<76;i++)
        {
            float x=-13+(float)ForestFogField.Hash(i,73)*19,y=-9+(float)ForestFogField.Hash(i,87)*20;
            if(Distance(new Vector2(x,y))<.8f)continue;
            var r=Prop(root,"Hojas, brotes y raices bajas "+i,trees[i%4==0?"Raices":"Arbusto"],new Vector2(x,y),.10f+(i%3)*.035f);
            r.color=new Color(.6f,.64f,.5f);UnityEngine.Object.DestroyImmediate(r.GetComponent<YSort>());r.sortingOrder=-1790;
        }
        foreach(var point in new[]{new Vector2(-12.4f,8.7f),new Vector2(-8.1f,9.6f),new Vector2(-10.8f,11.2f),new Vector2(-4,8.7f)})
            Prop(root,"Maleza alrededor de materiales",trees["Arbusto"],point,.32f,true);
    }
    public static void FinishInteractionDetails()
    {
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        outline=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel01/GeneratorPolish/Contorno.mat");
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_Generador.unity");Bind();
        var fuse=UnityEngine.Object.FindAnyObjectByType<FusePickup>();
        var sprite=(SpriteRenderer)new SerializedObject(fuse).FindProperty("artwork").objectReferenceValue;
        if(sprite.GetComponent<InteractionOutline>()==null)
        {
            var o=sprite.gameObject.AddComponent<InteractionOutline>();
            Set(o,s=>{s.FindProperty("artwork").objectReferenceValue=sprite;s.FindProperty("target").objectReferenceValue=fuse;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("director").objectReferenceValue=director;s.FindProperty("outlineMaterial").objectReferenceValue=outline;});
        }
        foreach(var r in UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.name.StartsWith("Cerca norte envejecida")||r.name.StartsWith("Cerca este del recinto")))
        {var c=r.GetComponent<BoxCollider2D>();if(c!=null)c.size=new Vector2(r.sprite.bounds.size.x*.94f,c.size.y);}
        EditorSceneManager.SaveScene(scene);
        scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_InteriorReten.unity");Bind();
        var desk=GameObject.Find("Escritorio_Radio80_Bitacora_Lampara").GetComponent<SpriteRenderer>();
        var radio=GameObject.Find("Silueta de radio").transform;
        radio.localPosition=new Vector3((116-desk.sprite.pivot.x)/64,(196-desk.sprite.pivot.y)/64,0);radio.localScale=new Vector3(1.76f,1.39f,1);
        Bake(Art+"RadioMask.png",64,64,(x,y)=>
        {
            bool body=x>=4&&x<=60&&y>=4&&y<=45;
            bool handle=(x>=10&&x<=14||x>=49&&x<=53)&&y>=45&&y<=59||x>=10&&x<=53&&y>=56&&y<=60;
            return new Color32(255,255,255,body||handle?(byte)255:(byte)0);
        },true,64);
        EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Report("Interacciones.txt","PASS: generator fuse silhouette, fence colliders and radio silhouette aligned to original artwork\n");
    }
    static SpriteRenderer Prop(Transform parent,string name,Sprite sprite,Vector2 point,float scale,bool solid=false)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer),typeof(YSort));if(parent!=null)obj.transform.SetParent(parent,false);
        obj.transform.position=point;obj.transform.localScale=Vector3.one*scale;
        var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;r.sortingOrder=Mathf.RoundToInt(-point.y*32);
        if(solid){var c=obj.AddComponent<BoxCollider2D>();c.offset=Vector2.up*.14f/scale;c.size=new Vector2(Mathf.Min(1.8f,r.bounds.size.x*.72f)/scale,.35f/scale);}
        return r;
    }
    static void Systems(bool checkpoint)
    {
        if(leo.GetComponent<MovementNoise>()==null)leo.gameObject.AddComponent<MovementNoise>();
        var survival=leo.GetComponent<SurvivalPlayer>()??leo.gameObject.AddComponent<SurvivalPlayer>();
        Set(survival,s=>{s.FindProperty("forestCheckpoint").boolValue=checkpoint;s.FindProperty("safeCheckpoint").vector2Value=new Vector2(-8,8);});
        var aim=leo.GetComponent<BottleAim>()??leo.gameObject.AddComponent<BottleAim>();
        Set(aim,s=>{s.FindProperty("bottleSprite").objectReferenceValue=bottle;s.FindProperty("bottleMaterial").objectReferenceValue=lit;});
        var hud=leo.root.GetComponentInChildren<SurvivalHUD>();if(hud==null)hud=new GameObject("HUD de supervivencia",typeof(SurvivalHUD)).GetComponent<SurvivalHUD>();
        hud.transform.SetParent(leo.root,false);Set(hud,s=>s.FindProperty("player").objectReferenceValue=survival);
    }
    static SupplyPickup Pickup(Transform root,string id,SupplyKind kind,Vector2 point,Sprite sprite)
    {
        var r=Prop(root,id,sprite,point,1);var p=r.gameObject.AddComponent<SupplyPickup>();
        Set(p,s=>{s.FindProperty("uniqueId").stringValue=id;s.FindProperty("kind").enumValueIndex=(int)kind;s.FindProperty("artwork").objectReferenceValue=r;});extra.Add(p);return p;
    }
    static void Supplies()
    {
        var root=new GameObject("Repuestos de supervivencia junto al fusible").transform;
        Pickup(root,"generador_bateria_01",SupplyKind.Battery,new Vector2(-8.1f,3.8f),battery);
        Pickup(root,"generador_bateria_02",SupplyKind.Battery,new Vector2(-6.7f,3.8f),battery);
        Pickup(root,"generador_botella_01",SupplyKind.Bottle,new Vector2(-5.5f,3.5f),bottle);
    }
    static void AppendTargets()
    {
        Set(director.GetComponent<PlayerInteraction>(),s=>
        {var a=s.FindProperty("targets");int old=a.arraySize;a.arraySize+=extra.Count;for(int i=0;i<extra.Count;i++)a.GetArrayElementAtIndex(old+i).objectReferenceValue=extra[i];});
    }
    static void Boundary(Transform root,string name,Vector2 position,Vector2 size)
    {
        var obj=new GameObject(name,typeof(BoxCollider2D));obj.transform.SetParent(root,false);obj.transform.position=position;obj.GetComponent<BoxCollider2D>().size=size;
    }
    static void Arrival(string id,Vector2 point,Camera camera)
    {
        var obj=new GameObject("Aparicion_"+id,typeof(RetenArrival));obj.transform.position=point;
        Set(obj.GetComponent<RetenArrival>(),s=>{s.FindProperty("spawnId").stringValue=id;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("worldCamera").objectReferenceValue=camera;s.FindProperty("battery").objectReferenceValue=leo.GetComponentInChildren<FlashlightBattery>();s.FindProperty("flashlight").objectReferenceValue=leo.GetComponentInChildren<FlashlightController>();});
    }
    static ForestPassage Passage(string name,Vector2 point,string target,string spawn,bool requirement,Vector2 approach)
    {
        var marker=Prop(null,name,trees["Raices"],point,.45f);var p=marker.gameObject.AddComponent<ForestPassage>();
        Set(p,s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("destination").stringValue=target;s.FindProperty("arrival").stringValue=spawn;s.FindProperty("requiresGenerator").boolValue=requirement;s.FindProperty("approachDirection").vector2Value=approach;});extra.Add(p);return p;
    }
    static void SouthTrail()
    {
        var root=new GameObject("Sendero sur entre raices").transform;
        var south=GameObject.Find("Limite_Sur_Bosque");south.transform.position=new Vector3(0,-21,0);south.GetComponent<BoxCollider2D>().size=new Vector2(36,1);
        foreach(string edge in new[]{"Limite_Este_Bosque","Limite_Oeste_Bosque"}){var c=GameObject.Find(edge).GetComponent<BoxCollider2D>();c.transform.position+=Vector3.down*4.75f;c.size=new Vector2(1,33.5f);}
        foreach(var c in UnityEngine.Object.FindObjectsByType<Collider2D>())
        {
            if(c.isTrigger||c.GetComponent<LeoMovement>()!=null||c.name.StartsWith("Limite")||c.bounds.center.y>-3)continue;
            if(GeneratorTerrainField.SouthPathDistance(c.bounds.center.x,c.bounds.center.y)<2.0)
            {
                var delta=(c.bounds.center.x>.0f?1:-1)*3.4f;
                c.transform.position+=Vector3.right*delta;
            }
        }
        for(int side=-1;side<=1;side+=2)for(int i=0;i<12;i++)
        {
            float y=-5-i*1.4f;float x=(y<-14?-.5f:1)+side*(3.1f+(i%3)*.55f);
            Prop(root,"Pinos del descenso "+side+" "+i,trees[i%2==0?"Pino_Alto":"Pino_Mediano"],new Vector2(x,y),.7f+i%3*.09f,true);
            if(i%2==0)Prop(root,"Maleza del sendero",trees["Arbusto"],new Vector2(x-side*.7f,y-.3f),.48f,true);
            if(i%4==0)Prop(root,"Raices del sendero",trees["Raices"],new Vector2(x-side*.8f,y-.7f),.32f);
        }
        Passage("Acceso natural al bosque",new Vector2(-1,-19),Forest,"bosque_entrada",true,Vector2.up);
        Arrival("generador_sur",new Vector2(-1,-17.3f),Camera.main);
        Bake("Assets/Art/Nivel01/Generator/SueloGenerador.png",GeneratorTerrainField.Width,GeneratorTerrainField.Height,(x,y)=>
        {uint c=GeneratorTerrainField.Pixel(x,y);return new Color32((byte)c,(byte)(c>>8),(byte)(c>>16),255);},false);
    }
    static readonly Vector2[] Route={new Vector2(-8,16),new Vector2(-8,8),new Vector2(-6,4),new Vector2(-3,1),new Vector2(0,-3),new Vector2(5,-6),new Vector2(11,-10)};
    static float Distance(Vector2 p)
    {
        float best=1000;for(int i=1;i<Route.Length;i++){var a=Route[i-1];var d=Route[i]-a;float t=Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(p,a+d*t));}return best;
    }
    static void BuildForest()
    {
        Bake(Art+"SueloBosque.png",2048,1536,(x,y)=>
        {
            Vector2 p=new Vector2((x+.5f)/32-32,(y+.5f)/32-24);float grain=(float)ForestFogField.Hash(x,y),broad=Mathf.PerlinNoise(p.x*.15f+50,p.y*.15f+50);
            float path=Mathf.SmoothStep(0,1,Mathf.Clamp01((1.7f+(Mathf.PerlinNoise(p.x*2+50,p.y*2+50)-.5f)*.5f-Distance(p))/.7f));
            float clear=Mathf.Clamp01(1-Vector2.Distance(p,new Vector2(2,-4))/5);path=Mathf.Max(path,clear*.8f);
            return new Color32((byte)(32+path*29+broad*7+grain*4),(byte)(42+path*11+broad*8+grain*4),(byte)(35+path*4+broad*6+grain*4),255);
        },true,32);
        var ground=Prop(null,"Suelo continuo del bosque",AssetDatabase.LoadAssetAtPath<Sprite>(Art+"SueloBosque.png"),Vector2.zero,1);UnityEngine.Object.DestroyImmediate(ground.GetComponent<YSort>());ground.sortingOrder=-1800;
        var root=new GameObject("Bosque denso de montana").transform;
        for(int row=0;row<14;row++)for(int col=0;col<19;col++)
        {
            float x=-29+col*3.2f+(row%2)*1.2f,y=-21+row*3.1f;
            var p=new Vector2(x,y);bool clearing=Vector2.Distance(p,new Vector2(2,-4))<6;
            bool camp=x>-13&&x<-3&&y>5&&y<12;
            if(Distance(p)<2.6f||clearing||camp)continue;
            int index=row*19+col;
            var r=Prop(root,"Pino antiguo "+index,trees[index%3==0?"Arbol_Frondoso":index%2==0?"Pino_Alto":"Pino_Mediano"],p,.8f+(index%4)*.09f,true);
            r.color=new Color(.74f+(index%3)*.07f,.82f,.82f);
            if(index%3==0)Prop(root,"Maleza del bosque "+index,trees["Arbusto"],p+new Vector2(1.1f,-.6f),.48f,true);
            if(index%7==0)Prop(root,"Hojas y raices "+index,trees["Raices"],p+new Vector2(-.7f,-.5f),.34f);
        }
        Boundary(root,"Bosque impenetrable norte",new Vector2(0,16),new Vector2(34,1));
        Boundary(root,"Bosque impenetrable sur",new Vector2(0,-16),new Vector2(34,1));
        Boundary(root,"Bosque impenetrable oeste",new Vector2(-17,0),new Vector2(1,32));
        Boundary(root,"Bosque impenetrable este",new Vector2(17,0),new Vector2(1,32));
        var campRoot=new GameObject("Campamento forestal abandonado").transform;
        var shed=Prop(campRoot,"Refugio deteriorado",trees["Caseta_Reten"],new Vector2(-10,9),.65f,true);shed.color=new Color(.62f,.7f,.68f);
        Boundary(campRoot,"Pared del refugio",new Vector2(-10,10.4f),new Vector2(3.8f,2.4f));
        var table=Prop(campRoot,"Mesa vieja con nota",props["Mesa_Papeles_Taza"],new Vector2(-5.4f,8),.4f,true);
        Prop(campRoot,"Carretilla oxidada",wheelbarrow,new Vector2(-11.6f,5.7f),1,true);
        for(int i=0;i<3;i++)Prop(campRoot,"Troncos apilados "+i,trees["Tronco_Caido"],new Vector2(-13,8+i*.45f),.46f,true);
        Prop(campRoot,"Materiales abandonados",trees["Caja"],new Vector2(-4.2f,6.4f),.37f,true);
        Prop(campRoot,"Herramientas olvidadas",props["Mochila_Herramientas"],new Vector2(-12.1f,6.5f),.37f);
        Prop(campRoot,"Caja oxidada junto al refugio",trees["Barril"],new Vector2(-7.3f,10.6f),.34f,true);
        Pickup(campRoot,"campamento_botiquin_01",SupplyKind.Medkit,new Vector2(-7,7),medkit);
        var paper=Prop(campRoot,"Nota entre materiales",note,new Vector2(-5.4f,8.6f),.8f);
        Document(paper,"Leer nota","Si escuchas algo excavando, no corras. No importa cuánto te asustes. No corras.");
        Set(paper.GetComponent<ForestDocument>(),s=>s.FindProperty("interactionOffset").vector2Value=Vector2.down*1.1f);
        var inspect=Prop(campRoot,"Herramientas del campamento",props["Mochila_Herramientas"],new Vector2(-4.2f,5.5f),.33f);
        Document(inspect,"Examinar herramientas","Herramientas oxidadas, barro seco y materiales abandonados. Nadie parece haber trabajado aquí en mucho tiempo.");
        var clearingRoot=new GameObject("Claro de las Raices").transform;
        foreach(var p in new[]{new Vector2(-2,-6),new Vector2(5,-2),new Vector2(6,-7),new Vector2(-2,-3),new Vector2(2,-8)})
        {
            var r=Prop(clearingRoot,"Raices antiguas",trees["Raices"],p,.72f);
            if(p.x>4)Prop(clearingRoot,"Arbol del claro",trees["Pino_Alto"],p+Vector2.up*1.2f,1.15f,true);
        }
        var soil=Prop(clearingRoot,"Tierra removida del escondite",trees["Roca_Musgo"],new Vector2(3,-3),.56f);soil.color=new Color(.48f,.4f,.3f);
        foreach(var p in new[]{new Vector2(1,-4),new Vector2(4,-5),new Vector2(-.5f,-6)}){var mound=Prop(clearingRoot,"Monticulo de tierra",trees["Roca_Musgo"],p,.3f);mound.color=new Color(.52f,.41f,.3f);}
        var enemyArt=Prop(clearingRoot,"Escarbador — representacion provisional",creature,new Vector2(3,-3),1);
        UnityEngine.Object.DestroyImmediate(enemyArt.GetComponent<YSort>());
        var body=enemyArt.gameObject.AddComponent<Rigidbody2D>();body.gravityScale=0;body.freezeRotation=true;body.collisionDetectionMode=CollisionDetectionMode2D.Continuous;
        var collider=enemyArt.gameObject.AddComponent<CircleCollider2D>();collider.offset=Vector2.zero;collider.radius=.26f;
        var enemy=enemyArt.gameObject.AddComponent<Escarbador>();Set(enemy,s=>{var frames=s.FindProperty("crawlFrames");frames.arraySize=1;frames.GetArrayElementAtIndex(0).objectReferenceValue=creature;});
        var encounter=new GameObject("Primer encuentro y final del nivel",typeof(RootsEncounter)).GetComponent<RootsEncounter>();
        Set(encounter,s=>{s.FindProperty("leo").objectReferenceValue=leo.GetComponent<SurvivalPlayer>();s.FindProperty("creature").objectReferenceValue=enemy;s.FindProperty("movingSoil").objectReferenceValue=soil.transform;});
        Passage("Sendero de regreso al generador",new Vector2(-8,14),"Assets/Scenes/Nivel01/N1_Generador.unity","generador_sur",false,Vector2.down);
        Arrival("bosque_entrada",new Vector2(-8,12),Camera.main);Arrival("bosque_checkpoint",new Vector2(-8,8),Camera.main);
    }
    static void Document(SpriteRenderer r,string title,string body)
    {
        var doc=ScriptableObject.CreateInstance<DocumentDefinition>();doc.title=title;doc.body=body;
        string path=Art+(title.Contains("nota")?"NotaCampamento":"HerramientasCampamento")+".asset";AssetDatabase.CreateAsset(doc,path);
        var interaction=r.gameObject.AddComponent<ForestDocument>();Set(interaction,s=>s.FindProperty("document").objectReferenceValue=doc);extra.Add(interaction);
    }
    static void UniversalOutlines()
    {
        var targets=UnityEngine.Object.FindObjectsByType<WorldInteraction>(FindObjectsSortMode.None);
        foreach(var target in targets)
        {
            if(UnityEngine.Object.FindObjectsByType<InteractionOutline>(FindObjectsSortMode.None).Any(o=>new SerializedObject(o).FindProperty("target").objectReferenceValue==target))continue;
            SpriteRenderer artwork=target.GetComponent<SpriteRenderer>();MeshRenderer mesh=null;
            if(target is GeneratorAccessGate)
            {
                var hinge=(Transform)new SerializedObject(target).FindProperty("gateLeaf").objectReferenceValue;mesh=hinge.GetComponentInChildren<MeshRenderer>();
            }
            else if(target is NarrativeTarget && target.name.Contains("Radio"))
            {
                var desk=GameObject.Find("Escritorio_Radio80_Bitacora_Lampara").GetComponent<SpriteRenderer>();
                artwork=Mask(desk,"Silueta de radio",new Vector2((116-desk.sprite.pivot.x)/64,(196-desk.sprite.pivot.y)/64),new Vector2(1.76f,1.39f),"RadioMask");
            }
            else if(target is RetenDoor)
            {
                var options=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.name.IndexOf("Puerta",StringComparison.OrdinalIgnoreCase)>=0&&r.sprite!=null).ToArray();
                artwork=options.OrderBy(r=>Vector2.Distance(r.transform.position,target.transform.position)).FirstOrDefault();
                if(artwork==null)
                {
                    var shed=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().First(r=>r.name.Contains("Caseta")&&r.sprite!=null);
                    artwork=Mask(shed,"Silueta de puerta del reten",new Vector2(-.35f,.85f),new Vector2(.68f,1.38f),"DoorMask");
                }
            }
            else if(target is GeneratorInteraction)artwork=GameObject.Find("Generador_Sprite_Visual").GetComponent<SpriteRenderer>();
            if(artwork==null&&mesh==null)
                artwork=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.sprite!=null&&r.gameObject.activeInHierarchy).OrderBy(r=>Vector2.Distance(r.transform.position,target.InteractionPoint)).FirstOrDefault();
            if(artwork==null&&mesh==null)throw new Exception("Interactuable sin silueta: "+target.name);
            var owner=artwork!=null?artwork.gameObject:mesh.gameObject;var o=owner.AddComponent<InteractionOutline>();
            Set(o,s=>{s.FindProperty("artwork").objectReferenceValue=artwork;s.FindProperty("meshArtwork").objectReferenceValue=mesh;s.FindProperty("target").objectReferenceValue=target;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("director").objectReferenceValue=director;s.FindProperty("outlineMaterial").objectReferenceValue=outline;});
        }
    }
    static SpriteRenderer Mask(SpriteRenderer parent,string name,Vector2 local,Vector2 size,string asset)
    {
        string path=Art+asset+".png";
        if(!File.Exists(path))Bake(path,64,64,(x,y)=>
        {
            bool inside=x>3&&x<61&&y>3&&y<61;if(asset=="RadioMask")inside=x>3&&x<61&&y>8&&y<50||x>17&&x<45&&y>=50&&y<56;
            return new Color32(255,255,255,inside?(byte)255:(byte)0);
        },true,64);
        var r=Prop(parent.transform,name,AssetDatabase.LoadAssetAtPath<Sprite>(path),Vector2.zero,1);
        r.transform.localPosition=local;r.transform.localScale=new Vector3(size.x,size.y,1);r.color=Color.clear;r.sortingOrder=parent.sortingOrder+1;return r;
    }
    static void CreateArt()
    {
        battery=PixelArt("Bateria",16,24,(x,y)=>
        {
            if(x>=6&&x<=9&&y>=21&&y<=23)return new Color32(160,162,148,255);
            if(x<3||x>12||y<1||y>21)return Color.clear;
            if(x==3||x==12||y==1||y==21)return new Color32(32,39,35,255);
            return y>13?new Color32(181,150,80,255):new Color32((byte)(83+x*3),113,103,255);
        });
        bottle=PixelArt("Botella",16,30,(x,y)=>
        {
            bool neck=x>=6&&x<=9&&y>=20&&y<=28,body=x>=3&&x<=12&&y>=1&&y<20;
            if(!neck&&!body)return Color.clear;
            if(y>=26)return new Color32(124,117,100,255);
            if(x==3||x==12||x==6&&y>19)return new Color32(32,57,44,255);
            if(y>7&&y<13)return new Color32(164,159,114,255);
            return new Color32((byte)(x<6?116:55),115,77,230);
        });
        medkit=PixelArt("BotiquinRojo",30,22,(x,y)=>
        {
            if(x<2||x>27||y<1||y>20)return Color.clear;
            if(x==2||x==27||y==1||y==20)return new Color32(50,24,26,255);
            if(x>12&&x<17&&y>4&&y<17||y>8&&y<13&&x>8&&x<21)return new Color32(224,224,201,255);
            return new Color32((byte)(y>16?176:132),41,43,255);
        });
        wheelbarrow=PixelArt("CarretillaOxidada",74,46,(x,y)=>
        {
            if((x-49)*(x-49)+(y-8)*(y-8)<55)return new Color32(29,33,31,255);
            if((x-49)*(x-49)+(y-8)*(y-8)<18)return new Color32(103,95,76,255);
            if(y>=19&&y<=34&&x>12+(34-y)/2&&x<58-(34-y)/3)return new Color32((byte)(73+(x+y)%7*5),65,43,255);
            if(y>=31&&y<=35&&x>=9&&x<=61)return new Color32(111,90,52,255);
            if((x>=3&&x<=24&&y>12&&y<16)||(x>23&&x<49&&y>=10&&y<=13)||(x>20&&x<24&&y>=4&&y<23))return new Color32(83,76,60,255);
            return Color.clear;
        });
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Nivel01/Characters/Leo_Exterior.png");
        creature=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Escarbador.asset");
        if(creature==null){creature=UnityEngine.Sprite.Create(texture,new Rect(854,942,265,196),new Vector2(.5f,.06f),128,0,SpriteMeshType.FullRect);creature.name="Escarbador provisional encorvado";AssetDatabase.CreateAsset(creature,Art+"Escarbador.asset");}
        note=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Nota.asset");
        if(note==null){note=UnityEngine.Sprite.Create(texture,new Rect(938,554,78,94),new Vector2(.5f,.1f),128,0,SpriteMeshType.FullRect);note.name="Nota de campamento";AssetDatabase.CreateAsset(note,Art+"Nota.asset");}
    }
    static Sprite PixelArt(string name,int width,int height,Func<int,int,Color> pixel)
    {string path=Art+name+".png";Bake(path,width,height,(x,y)=>(Color32)pixel(x,y),true,48);return AssetDatabase.LoadAssetAtPath<Sprite>(path);}
    static void Bake(string path,int width,int height,Func<int,int,Color32> pixel,bool configure,int ppu=32)
    {
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);var data=new Color32[width*height];
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)data[y*width+x]=pixel(x,y);
        texture.SetPixels32(data);texture.Apply();File.WriteAllBytes(path,texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        if(configure)
        {var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=ppu;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
    }
}
