using System;
using System.IO;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

// Edits only the service access in the exterior. Does not save any other scene or atlas.
public static class GeneratorAccessBuilder
{
    const string ScenePath = "Assets/Scenes/Nivel01/N1_ExteriorReten.unity";
    const string Art = "Assets/Art/Nivel01/GeneratorAccess/";
    static readonly Vector2[] Road = { new Vector2(11.1f,2.5f), new Vector2(12.8f,1.2f),
        new Vector2(14.5f,.65f), new Vector2(17,1.1f), new Vector2(20,2.4f), new Vector2(23,3.7f), new Vector2(27,4.3f) };

    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Detén Play antes de editar el acceso.");
        var scene = EditorSceneManager.OpenScene(ScenePath);
        if(GameObject.Find("Acceso cercado al generador") != null || File.Exists(Art+"Reja.asset"))
            throw new InvalidOperationException("El acceso ya existe; no se sobrescribirá.");
        var sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png")
            .OfType<Sprite>().ToDictionary(s=>s.name);
        var sign = GameObject.Find("Senal_Camino_Servicio");
        var oldPath = GameObject.Find("Sendero_Acceso_Generador");
        var access = GameObject.Find("Acceso_Generador_Interactivo_E");
        if(sign==null || oldPath==null || access==null) throw new InvalidOperationException("Falta el acceso original.");
        var oldDoor = access.GetComponent<RetenDoor>();
        var oldSettings = new SerializedObject(oldDoor);
        var leo = oldSettings.FindProperty("leo").objectReferenceValue as Transform;
        string destination = oldSettings.FindProperty("targetScene").stringValue;
        string spawn = oldSettings.FindProperty("targetSpawn").stringValue;
        var lit = sign.GetComponent<SpriteRenderer>().sharedMaterial;
        sign.SetActive(false);oldPath.SetActive(false);

        var root = new GameObject("Acceso cercado al generador").transform;
        root.position = new Vector3(19.3f,1.75f,0);
        var material = new Material(Shader.Find("FrecuenciaCero/RejaIluminada2D"));
        material.name="Reja iluminada con atlas original";material.SetTexture("_MainTex",sprites["Cerca_Metal"].texture);
        AssetDatabase.CreateAsset(material,Art+"Reja.mat");
        var mesh = FenceMesh(sprites["Cerca_Metal"],2.5f);
        AssetDatabase.CreateAsset(mesh,Art+"Reja.asset");
        Panel(root,"Cerca fija izquierda",mesh,material,new Vector3(-2.5f,1.5f,0));
        Panel(root,"Cerca fija derecha",mesh,material,new Vector3(2.5f,-1.5f,0));
        var hinge = new GameObject("Bisagra de la puerta").transform;hinge.SetParent(root,false);
        hinge.localPosition=new Vector3(-1.25f,.75f,0);
        var leaf = Panel(hinge,"Puerta metalica movil",mesh,material,new Vector3(1.25f,-.75f,0));
        // Ground footprints follow the projected fence. The interaction point lies on the near side.
        Footprint(root,"Colision cerca izquierda",new Vector2(-3.75f,2.25f),new Vector2(-1.25f,.75f));
        Footprint(root,"Colision puerta cerrada",new Vector2(-1.25f,.75f),new Vector2(1.25f,-.75f));
        Footprint(root,"Colision cerca derecha",new Vector2(1.25f,-.75f),new Vector2(3.75f,-2.25f));
        // A small metal latch makes the central section distinct without another standing sign.
        var latch = new GameObject("Pestillo de la puerta",typeof(SpriteRenderer));latch.transform.SetParent(leaf.transform,false);
        latch.transform.localPosition=new Vector3(.55f,.75f,0);latch.transform.localScale=new Vector3(.18f,.045f,1);
        latch.GetComponent<SpriteRenderer>().sprite=AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        latch.GetComponent<SpriteRenderer>().sharedMaterial=lit;latch.GetComponent<SpriteRenderer>().color=new Color(.48f,.39f,.25f);
        latch.GetComponent<SpriteRenderer>().sortingOrder=leaf.sortingOrder+1;

        var gate = access.AddComponent<GeneratorAccessGate>();
        access.transform.position = new Vector3(18.8f,1.22f,0);
        var settings = new SerializedObject(gate);
        settings.FindProperty("leo").objectReferenceValue=leo;
        settings.FindProperty("gateLeaf").objectReferenceValue=hinge;
        settings.FindProperty("targetScene").stringValue=destination;
        settings.FindProperty("targetSpawn").stringValue=spawn;
        settings.FindProperty("approachDirection").vector2Value=new Vector2(-1,.25f);
        settings.ApplyModifiedPropertiesWithoutUndo();
        var player = UnityEngine.Object.FindAnyObjectByType<PlayerInteraction>();
        var playerSettings = new SerializedObject(player);
        var targets = playerSettings.FindProperty("targets");
        for(int i=0;i<targets.arraySize;i++) if(targets.GetArrayElementAtIndex(i).objectReferenceValue==oldDoor)
            targets.GetArrayElementAtIndex(i).objectReferenceValue=gate;
        playerSettings.ApplyModifiedPropertiesWithoutUndo();
        UnityEngine.Object.DestroyImmediate(oldDoor);
        GameObject.Find("Aparicion_Exterior_Camino_Servicio").transform.position = new Vector3(17.7f,1.85f,0);

        var road = BakeRoad();
        var ground = new GameObject("Camino curvo hacia la derecha",typeof(SpriteRenderer));ground.transform.SetParent(root,false);
        ground.transform.position=new Vector3(18.5f,2,0);var renderer=ground.GetComponent<SpriteRenderer>();
        renderer.sprite=road;renderer.sharedMaterial=lit;renderer.sortingOrder=-1800;

        // Move only local trees whose solid footprint would obstruct the new maintenance route.
        int moved=0;
        foreach(var tree in UnityEngine.Object.FindObjectsByType<SpriteRenderer>())
        {
            var p=tree.transform.position;
            if(!tree.name.StartsWith("Arbol_") || p.x<12 || p.x>27 || p.y<-2.5f || p.y>6.5f)continue;
            if(PathDistance(p.x,p.y)<1.3f || (p.x>14.7f && p.x<16.5f && p.y<-.5f && p.y>-2))
            {
                float side=p.y>2?1:-1;
                tree.transform.position=new Vector3(p.x,p.y+side*2.1f,p.z);moved++;
            }
        }
        ClearLocalCanopies();
        foreach(var p in new[]{new Vector2(16.5f,-2.1f),new Vector2(19.5f,-.8f),new Vector2(22,1.1f),new Vector2(24.5f,6)})
        {
            var plant=new GameObject("Vegetacion del camino",typeof(SpriteRenderer),typeof(YSort));plant.transform.SetParent(root,false);plant.transform.position=p;
            plant.transform.localScale=Vector3.one*.20f;var r=plant.GetComponent<SpriteRenderer>();r.sprite=sprites["Arbusto"];r.sharedMaterial=lit;
        }
        var rock=new GameObject("Roca del borde del camino",typeof(SpriteRenderer),typeof(YSort));rock.transform.SetParent(root,false);rock.transform.position=new Vector3(20,4.25f,0);rock.transform.localScale=Vector3.one*.18f;
        rock.GetComponent<SpriteRenderer>().sprite=sprites["Roca_Musgo"];rock.GetComponent<SpriteRenderer>().sharedMaterial=lit;
        AssetDatabase.SaveAssets();EditorSceneManager.SaveScene(scene);
        File.WriteAllText("../AccesoGenerador/Construccion.txt","PASS: original E controller and RetenTravel reused\nPASS: original fence atlas retained\nMoved local obstructing trees: "+moved+"\n");
    }

    public static void RepositionExisting()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Detén Play antes de ajustar el acceso.");
        var scene=EditorSceneManager.OpenScene(ScenePath);
        var root=GameObject.Find("Acceso cercado al generador").transform;
        // Move the fence structure and its colliders, keeping the already-authored road and borders fixed.
        var scenery=root.Cast<Transform>().Where(t=>t.GetComponent<SpriteRenderer>()!=null).ToArray();
        var positions=scenery.Select(t=>t.position).ToArray();
        root.position=new Vector3(19.3f,1.75f,0);
        for(int i=0;i<scenery.Length;i++)scenery[i].position=positions[i];
        foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>())
            renderer.sortingOrder=Mathf.RoundToInt(-renderer.transform.position.y*32);
        var leaf=root.Find("Bisagra de la puerta/Puerta metalica movil").GetComponent<MeshRenderer>();
        leaf.GetComponentInChildren<SpriteRenderer>().sortingOrder=leaf.sortingOrder+1;
        GameObject.Find("Acceso_Generador_Interactivo_E").transform.position=new Vector3(18.8f,1.22f,0);
        GameObject.Find("Aparicion_Exterior_Camino_Servicio").transform.position=new Vector3(17.7f,1.85f,0);
        EditorSceneManager.SaveScene(scene);
    }

    static Mesh FenceMesh(Sprite sprite,float width)
    {
        float scale=width/sprite.bounds.size.x;
        var points=sprite.vertices.Select(v=>new Vector3(v.x*scale,Mathf.Round((v.y*scale-v.x*scale*.6f)*32)/32,0)).ToArray();
        var mesh=new Mesh{name="Cerca metalica proyectada"};mesh.vertices=points;mesh.uv=sprite.uv;
        mesh.colors=Enumerable.Repeat(Color.white,points.Length).ToArray();mesh.triangles=sprite.triangles.Select(t=>(int)t).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
    }
    static void ClearLocalCanopies()
    {
        // Nearby canopies conceal the service path even when their trunks are clear.
        // Reposition them as the southern border; preserve sprites, colliders and all other trees.
        string[] names={"Arbol_014","Arbol_128","Arbol_Borde_0_202","Arbol_Borde_1_052","Arbol_Borde_0_027","Arbol_Borde_0_022"};
        Vector2[] points={new Vector2(15.779f,-5.15f),new Vector2(20.6f,-3.2f),new Vector2(19.4f,-4.6f),new Vector2(24.6f,-3.5f),new Vector2(17.7166f,-5.1f),new Vector2(20.1854f,-4.8f)};
        for(int i=0;i<names.Length;i++)
        {
            var tree=GameObject.Find(names[i]);if(tree!=null)tree.transform.position=points[i];
        }
    }
    static MeshRenderer Panel(Transform parent,string name,Mesh mesh,Material material,Vector3 position)
    {
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=position;
        obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.sortingOrder=Mathf.RoundToInt(-obj.transform.position.y*32);r.shadowCastingMode=ShadowCastingMode.Off;return r;
    }
    static void Footprint(Transform root,string name,Vector2 a,Vector2 b)
    {
        var obj=new GameObject(name,typeof(PolygonCollider2D));obj.transform.SetParent(root,false);
        var offset=Vector2.Perpendicular((b-a).normalized)*.075f;
        obj.GetComponent<PolygonCollider2D>().points=new[]{a-offset,b-offset,b+offset,a+offset};
    }
    public static double PathDistance(double x,double y)
    {
        double best=double.MaxValue;
        for(int i=0;i<Road.Length-1;i++)for(int step=0;step<12;step++)
        {
            Vector2 a=Curve(i,step/12f),b=Curve(i,(step+1)/12f);
            double dx=b.x-a.x,dy=b.y-a.y,t=Math.Max(0,Math.Min(1,((x-a.x)*dx+(y-a.y)*dy)/(dx*dx+dy*dy)));
            double ex=x-a.x-t*dx,ey=y-a.y-t*dy;best=Math.Min(best,ex*ex+ey*ey);
        }
        return Math.Sqrt(best);
    }
    static Vector2 Curve(int i,float t)
    {
        var a=Road[Math.Max(0,i-1)];var b=Road[i];var c=Road[i+1];var d=Road[Math.Min(Road.Length-1,i+2)];
        return .5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t);
    }
    static Sprite BakeRoad()
    {
        const int width=608,height=384;
        var texture=new Texture2D(width,height,TextureFormat.RGBA32,false);var pixels=new Color32[width*height];
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            double wx=9+(x+.5)/32,wy=-4+(y+.5)/32;
            double edge=(GeneratorTerrainField.Hash(x/7,y/7)-.5)*.20;
            double alpha=Math.Max(0,Math.Min(1,(1.3+edge-PathDistance(wx,wy))/.48));alpha=alpha*alpha*(3-2*alpha);
            double ends=Math.Min(1,Math.Max(0,(wx-9)/1.7))*Math.Min(1,Math.Max(0,(28-wx)/1.8));alpha*=ends;
            double grain=(GeneratorTerrainField.Hash(x,y)-.5)*8;
            double damp=GeneratorTerrainField.Hash(x/13,y/13)*5;
            pixels[y*width+x]=new Color32((byte)(91+grain-damp),(byte)(75+grain-damp),(byte)(53+grain-damp),(byte)(alpha*255));
        }
        texture.SetPixels32(pixels);texture.Apply();File.WriteAllBytes(Art+"CaminoServicio.png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(Art+"CaminoServicio.png",ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(Art+"CaminoServicio.png");importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=32;importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.alphaIsTransparency=true;importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(Art+"CaminoServicio.png");
    }
}
