using System;
using System.IO;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class GeneratorPolishBuilder
{
    const string Art="Assets/Art/Nivel01/GeneratorPolish/";
    static Material lit, outline;
    static System.Collections.Generic.Dictionary<string,Sprite> exterior,interior;
    static Transform leo;
    static NarrativeDirector director;
    static UnityEngine.Object parts;

    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode||File.Exists(Art+"CajaPartes.asset"))
            throw new InvalidOperationException("No se sobrescribirá una instalación ya preparada.");
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_Generador.unity");
        exterior=Sprites("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png");
        interior=Sprites("Assets/Art/Nivel01/Interior/InteriorReten_Atlas.png");
        leo=GameObject.Find("Leo").transform;director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        outline=new Material(Shader.Find("FrecuenciaCero/ContornoInteraccion2D")){name="Contorno fino de interaccion"};
        AssetDatabase.CreateAsset(outline,Art+"Contorno.mat");
        var root=new GameObject("Instalacion de mantenimiento — pulido visual").transform;
        BuildReturnGate(root);
        BuildLamp();
        BuildToolbox(root);
        Decorate(root);
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        BuildDocumentOutlines();
        AssetDatabase.SaveAssets();
        File.WriteAllText("../GeneradorPulido/Construccion.txt","PASS: generator, return gate, lamp and toolbox prepared\nPASS: shared silhouette outlines on toolbox and original documents\n");
    }
    static System.Collections.Generic.Dictionary<string,Sprite> Sprites(string path)=>AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().ToDictionary(s=>s.name);
    static void Set(UnityEngine.Object obj,Action<SerializedObject> configure){var s=new SerializedObject(obj);configure(s);s.ApplyModifiedPropertiesWithoutUndo();}
    static SpriteRenderer Sprite(Transform parent,string name,Sprite sprite,Vector2 point,float scale,bool solid=false)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer),typeof(YSort));obj.transform.SetParent(parent,false);obj.transform.position=point;obj.transform.localScale=Vector3.one*scale;
        var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;r.sortingOrder=Mathf.RoundToInt(-point.y*32);
        if(solid){var c=obj.AddComponent<BoxCollider2D>();c.offset=new Vector2(0,.12f/scale);c.size=new Vector2(Mathf.Min(r.bounds.size.x*.72f,1.1f)/scale,.26f/scale);}
        return r;
    }
    static void Outline(SpriteRenderer artwork,WorldInteraction target)
    {
        var component=artwork.gameObject.AddComponent<InteractionOutline>();
        Set(component,s=>{s.FindProperty("artwork").objectReferenceValue=artwork;s.FindProperty("target").objectReferenceValue=target;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("director").objectReferenceValue=director;s.FindProperty("outlineMaterial").objectReferenceValue=outline;});
    }
    static void ReplaceTarget(WorldInteraction from,WorldInteraction to)
    {
        Set(UnityEngine.Object.FindAnyObjectByType<PlayerInteraction>(),s=>
        {var a=s.FindProperty("targets");for(int i=0;i<a.arraySize;i++)if(a.GetArrayElementAtIndex(i).objectReferenceValue==from)a.GetArrayElementAtIndex(i).objectReferenceValue=to;});
    }
    static void BuildReturnGate(Transform parent)
    {
        GameObject.Find("Senal_Entrada_Servicio").SetActive(false);
        var obj=GameObject.Find("Regreso_Exterior_Interactivo_E");var old=obj.GetComponent<RetenDoor>();var data=new SerializedObject(old);
        string destination=data.FindProperty("targetScene").stringValue,spawn=data.FindProperty("targetSpawn").stringValue;
        var gateRoot=new GameObject("Reja de regreso al reten").transform;gateRoot.SetParent(parent,false);gateRoot.position=new Vector3(-14.8f,-7.1f,0);
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/Art/Nivel01/GeneratorAccess/Reja.asset");var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Nivel01/GeneratorAccess/Reja.mat");
        MeshObject(gateRoot,"Cerca de regreso izquierda",mesh,material,new Vector3(-2.5f,1.5f));
        MeshObject(gateRoot,"Cerca de regreso derecha",mesh,material,new Vector3(2.5f,-1.5f));
        var hinge=new GameObject("Bisagra reja de regreso").transform;hinge.SetParent(gateRoot,false);hinge.localPosition=new Vector3(-1.25f,.75f,0);
        MeshObject(hinge,"Puerta de regreso",mesh,material,new Vector3(1.25f,-.75f,0));
        Footprint(gateRoot,"Colision reja izquierda",new Vector2(-3.75f,2.25f),new Vector2(-1.25f,.75f));
        Footprint(gateRoot,"Colision reja puerta",new Vector2(-1.25f,.75f),new Vector2(1.25f,-.75f));
        Footprint(gateRoot,"Colision reja derecha",new Vector2(1.25f,-.75f),new Vector2(3.75f,-2.25f));
        obj.transform.position=new Vector3(-14.15f,-6.48f,0);var gate=obj.AddComponent<GeneratorAccessGate>();
        Set(gate,s=>{s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("gateLeaf").objectReferenceValue=hinge;s.FindProperty("targetScene").stringValue=destination;s.FindProperty("targetSpawn").stringValue=spawn;s.FindProperty("approachDirection").vector2Value=new Vector2(1,.3f);});
        ReplaceTarget(old,gate);UnityEngine.Object.DestroyImmediate(old);
    }
    static MeshRenderer MeshObject(Transform parent,string name,Mesh mesh,Material material,Vector3 local)
    {
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=local;obj.GetComponent<MeshFilter>().sharedMesh=mesh;
        var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.sortingOrder=Mathf.RoundToInt(-obj.transform.position.y*32);r.shadowCastingMode=ShadowCastingMode.Off;return r;
    }
    static void Footprint(Transform parent,string name,Vector2 a,Vector2 b)
    {
        var obj=new GameObject(name,typeof(PolygonCollider2D));obj.transform.SetParent(parent,false);var n=Vector2.Perpendicular((b-a).normalized)*.075f;obj.GetComponent<PolygonCollider2D>().points=new[]{a-n,b-n,b+n,a+n};
    }
    static void BuildLamp()
    {
        var pole=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Single(r=>r.name=="Poste_Cable_Servicio"&&r.transform.position.x<-8);
        pole.transform.position=new Vector3(-10.1f,-5.9f,0);
        var light=GameObject.Find("Farol_Acceso_Existente").GetComponent<Light2D>();light.transform.position=new Vector3(-9.75f,-4.3f,0);light.intensity=.75f;light.pointLightOuterRadius=3.5f;light.pointLightInnerRadius=.4f;light.falloffIntensity=.65f;
        var failure=light.gameObject.AddComponent<GeneratorEntryLamp>();var mask=BuildGlassMask(pole);Set(failure,s=>{s.FindProperty("lamp").objectReferenceValue=light;s.FindProperty("leo").objectReferenceValue=leo;s.FindProperty("triggerPoint").objectReferenceValue=pole.transform;s.FindProperty("pole").objectReferenceValue=pole;s.FindProperty("glassMask").objectReferenceValue=mask;});
        GameObject.Find("Global Light 2D").GetComponent<Light2D>().intensity=.55f;
    }
    static MeshRenderer BuildGlassMask(SpriteRenderer pole)
    {
        var original=new Texture2D(1,1);original.LoadImage(File.ReadAllBytes(AssetDatabase.GetAssetPath(pole.sprite.texture)));
        var vertices=new System.Collections.Generic.List<Vector3>();var colors=new System.Collections.Generic.List<Color>();var triangles=new System.Collections.Generic.List<int>();
        for(int y=500;y<554;y++)for(int x=778;x<824;x++)
        {
            var pixel=original.GetPixel(x,y);if(pixel.a<.5f||pixel.r<.68f||pixel.g<.40f||pixel.b>pixel.g*.86f)continue;
            float lx=(x-pole.sprite.rect.x-pole.sprite.pivot.x)/64,ly=(y-pole.sprite.rect.y-pole.sprite.pivot.y)/64;
            int n=vertices.Count;vertices.AddRange(new[]{new Vector3(lx,ly),new Vector3(lx,ly+1f/64),new Vector3(lx+1f/64,ly+1f/64),new Vector3(lx+1f/64,ly)});
            colors.AddRange(Enumerable.Repeat(new Color(.065f,.073f,.075f).linear,4));triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
        }
        UnityEngine.Object.DestroyImmediate(original);
        if(vertices.Count==0)throw new InvalidOperationException("No se encontró el cristal luminoso del farol original.");
        var mesh=new Mesh{name="Cristal sin emision"};mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();AssetDatabase.CreateAsset(mesh,Art+"FarolaCristal.asset");
        var mask=MeshObject(pole.transform,"Cristal oscurecido al fallar",mesh,AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Intro/IntroPaisaje.mat"),Vector3.zero);mask.sortingOrder=pole.sortingOrder+1;mask.enabled=false;return mask;
    }
    static Sprite Part(string name,Texture2D texture,Rect rect,Vector2 pivot)
    {
        var sprite=UnityEngine.Sprite.Create(texture,rect,pivot,64,0,SpriteMeshType.FullRect);sprite.name=name;
        if(parts==null){parts=sprite;AssetDatabase.CreateAsset(sprite,Art+"CajaPartes.asset");}else AssetDatabase.AddObjectToAsset(sprite,parts);
        return sprite;
    }
    static void BuildToolbox(Transform parent)
    {
        var tex=interior["Mochila_Herramientas"].texture;
        var closedSprite=Part("Caja metalica cerrada",tex,new Rect(217,26,175,126),new Vector2(.5f,.06f));
        var bodySprite=Part("Cuerpo de caja",tex,new Rect(217,26,175,70),new Vector2(.5f,7.56f/70));
        var lidSprite=Part("Tapa de caja",tex,new Rect(217,96,175,56),new Vector2(.5f,0));
        var backpack=Part("Mochila sin caja duplicada",tex,new Rect(50,22,166,201),new Vector2(.5f,.08f));
        GameObject.Find("Almacen_Herramientas").GetComponent<SpriteRenderer>().sprite=backpack;
        var box=new GameObject("Caja de herramientas con fusible").transform;box.SetParent(parent,false);box.position=new Vector3(-7.2f,4.6f,0);box.localScale=Vector3.one*.457f;
        var closed=BoxSprite(box,"Caja cerrada",closedSprite,Vector3.zero,-147);
        var body=BoxSprite(box,"Cuerpo visible de caja abierta",bodySprite,Vector3.zero,-147);body.enabled=false;
        var lid=BoxSprite(box,"Tapa animada",lidSprite,new Vector3(0,(70-7.56f)/64,0),-146);lid.enabled=false;
        var collider=box.gameObject.AddComponent<BoxCollider2D>();collider.offset=new Vector2(0,.12f/.457f);collider.size=new Vector2(.9f/.457f,.22f/.457f);
        var cavityMesh=new Mesh{name="Interior oscuro de la caja"};
        cavityMesh.vertices=new[]{new Vector3(-1.19f,.97f),new Vector3(-1.08f,1.36f),new Vector3(1.08f,1.36f),new Vector3(1.19f,.97f)};
        cavityMesh.colors=Enumerable.Repeat(new Color(.027f,.037f,.041f).linear,4).ToArray();cavityMesh.triangles=new[]{0,1,2,0,2,3};cavityMesh.RecalculateBounds();AssetDatabase.AddObjectToAsset(cavityMesh,parts);
        var cavity=MeshObject(box,"Hueco de la caja",cavityMesh,AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Intro/IntroPaisaje.mat"),Vector3.zero);cavity.sortingOrder=-145;cavity.gameObject.SetActive(false);
        var target=GameObject.Find("Fusible_Repuesto_Recogible_E").GetComponent<FusePickup>();var oldArt=target.GetComponent<SpriteRenderer>();oldArt.enabled=false;
        var fuse=BoxSprite(box,"Fusible visible dentro de la caja",oldArt.sprite,new Vector3(0,1.16f,0),-143);fuse.transform.localScale=Vector3.one*.32f;fuse.enabled=false;
        target.transform.localScale=Vector3.one;target.transform.position=new Vector3(-7.2f,4.4f,0);
        var visual=box.gameObject.AddComponent<ToolboxVisual>();Set(visual,s=>{s.FindProperty("closed").objectReferenceValue=closed;s.FindProperty("body").objectReferenceValue=body;s.FindProperty("lid").objectReferenceValue=lid;s.FindProperty("fuse").objectReferenceValue=fuse;s.FindProperty("cavity").objectReferenceValue=cavity.gameObject;});
        var examine=ScriptableObject.CreateInstance<DialogueSequence>();examine.lines=new[]{new DialogueLine{speaker="LEO",text="Una caja de herramientas. Parece que todavía queda algo dentro.",soundCue=""}};
        AssetDatabase.CreateAsset(examine,"Assets/Narrative/Nivel01/CajaHerramientas_Examinar.asset");
        Set(target,s=>{s.FindProperty("artwork").objectReferenceValue=fuse;s.FindProperty("toolbox").objectReferenceValue=visual;s.FindProperty("examineToolbox").objectReferenceValue=examine;});
        Outline(closed,target);Outline(body,target);Outline(lid,target);
    }
    static SpriteRenderer BoxSprite(Transform root,string name,Sprite sprite,Vector3 local,int order)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.SetParent(root,false);obj.transform.localPosition=local;var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;r.sortingOrder=order;return r;
    }
    static void Decorate(Transform root)
    {
        for(int i=0;i<9;i++)Sprite(root,"Cerca norte envejecida "+i,exterior["Cerca_Metal"],new Vector2(-11+i*2.4f,10),.58f,true);
        for(int i=0;i<4;i++)Sprite(root,"Cerca este del recinto "+i,exterior["Cerca_Metal"],new Vector2(10.4f+i*1.8f,8.5f-i*1.4f),.55f,true);
        foreach(var p in new[]{new Vector2(8.9f,4),new Vector2(9.4f,5.3f),new Vector2(-9.6f,6.7f),new Vector2(-5.1f,6.9f)})Sprite(root,"Materiales de mantenimiento",exterior["Caja"],p,.34f,true);
        Sprite(root,"Barril de reserva junto al taller",exterior["Barril"],new Vector2(8.1f,2.5f),.34f,true);
        Sprite(root,"Mesa de trabajo y documentos",interior["Mesa_Papeles_Taza"],new Vector2(8.8f,2.9f),.3f,true);
        var floor=Sprite(root,"Tablones de apoyo del generador",interior["Tablones_Envejecidos"],new Vector2(5,4.1f),.70f);UnityEngine.Object.DestroyImmediate(floor.GetComponent<YSort>());floor.sortingOrder=-1800;
        int count=0;
        for(int ring=0;ring<2;ring++)
        {
            float xMin=ring==0?-28:-18,xMax=ring==0?28:17,yMin=ring==0?-20:-13,yMax=ring==0?20:14;
            for(float x=xMin;x<=xMax;x+=3.6f){Tree(root,new Vector2(x,yMin),count++);Tree(root,new Vector2(x+.6f,yMax),count++);}
            for(float y=yMin+3;y<yMax;y+=3.8f){Tree(root,new Vector2(xMin,y),count++);Tree(root,new Vector2(xMax,y+.8f),count++);}
        }
        foreach(var p in new[]{new Vector2(-11,3),new Vector2(-10,8.5f),new Vector2(-5.5f,8.7f),new Vector2(1.5f,8.8f),new Vector2(8.8f,8.6f),new Vector2(10,-1),new Vector2(7,-1.6f),new Vector2(-5,-6),new Vector2(-13,-9.5f),new Vector2(-9,-7.8f)})
        {Sprite(root,"Maleza del borde de mantenimiento",exterior["Arbusto"],p,.27f);}
        foreach(var p in new[]{new Vector2(-11.5f,-1.8f),new Vector2(-4,1.5f),new Vector2(9.5f,-3),new Vector2(8.7f,9)})Sprite(root,"Piedras entre la vegetacion",exterior["Roca_Musgo"],p,.20f);
        GameObject.Find("Cables_Postes_Mantenimiento").SetActive(false);
        var poles=UnityEngine.Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.name=="Poste_Cable_Servicio").OrderBy(r=>r.transform.position.x).ToArray();
        for(int i=1;i<poles.Length;i++)
        {
            var obj=new GameObject("Cable suspendido de mantenimiento "+i,typeof(LineRenderer));obj.transform.SetParent(root,false);var line=obj.GetComponent<LineRenderer>();
            line.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Intro/IntroPaisaje.mat");line.positionCount=17;line.startWidth=line.endWidth=.035f;line.startColor=line.endColor=new Color(.055f,.065f,.075f).linear;line.sortingOrder=180;line.shadowCastingMode=ShadowCastingMode.Off;
            Vector3 a=poles[i-1].transform.position+Vector3.up*1.75f,b=poles[i].transform.position+Vector3.up*1.75f;
            for(int k=0;k<17;k++){float t=k/16f;line.SetPosition(k,Vector3.Lerp(a,b,t)+Vector3.down*(Mathf.Sin(t*Mathf.PI)*.32f));}
        }
    }
    static void Tree(Transform root,Vector2 point,int index)
    {
        if(GeneratorTerrainField.PathDistance(point.x,point.y)<2.1)return;
        Sprite(root,"Bosque del recinto "+index,exterior[index%3==0?"Arbol_Frondoso":index%3==1?"Pino_Alto":"Pino_Mediano"],point,.8f+index%3*.04f,true);
    }
    static void BuildDocumentOutlines()
    {
        var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_InteriorReten.unity");leo=GameObject.Find("Leo").transform;director=UnityEngine.Object.FindAnyObjectByType<NarrativeDirector>();
        var map=GameObject.Find("Mapa_CimaDelAguila_EnPared").GetComponent<SpriteRenderer>();Outline(map,GameObject.Find("Mapa_Pared_LecturaLateral").GetComponent<NarrativeTarget>());
        var desk=GameObject.Find("Escritorio_Radio80_Bitacora_Lampara").GetComponent<SpriteRenderer>();
        // Invisible procedural silhouette aligned to the existing painted book; no duplicate book artwork.
        var texture=new Texture2D(116,62,TextureFormat.RGBA32,false);var pixels=new Color32[116*62];
        var polygon=new[]{new Vector2(7,4),new Vector2(0,48),new Vector2(48,61),new Vector2(58,54),new Vector2(109,60),new Vector2(115,11),new Vector2(58,0)};
        for(int y=0;y<62;y++)for(int x=0;x<116;x++)
        {
            bool inside=false;for(int i=0,j=polygon.Length-1;i<polygon.Length;j=i++)if((polygon[i].y>y)!=(polygon[j].y>y)&&x<(polygon[j].x-polygon[i].x)*(y-polygon[i].y)/(polygon[j].y-polygon[i].y)+polygon[i].x)inside=!inside;
            pixels[y*116+x]=new Color32(255,255,255,inside?(byte)255:(byte)0);
        }
        texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes(Art+"SiluetaBitacora.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(Art+"SiluetaBitacora.png",ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(Art+"SiluetaBitacora.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=64;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        var book=new GameObject("Silueta de la bitacora existente",typeof(SpriteRenderer));book.transform.SetParent(desk.transform,false);
        book.transform.localPosition=new Vector3((125+58-desk.sprite.pivot.x)/64,(90+31-desk.sprite.pivot.y)/64,0);
        var r=book.GetComponent<SpriteRenderer>();r.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"SiluetaBitacora.png");r.color=Color.clear;r.sharedMaterial=lit;r.sortingOrder=desk.sortingOrder+1;
        Outline(r,GameObject.Find("Bitacora_Escritorio").GetComponent<NarrativeTarget>());
        EditorSceneManager.SaveScene(scene);
    }
}
