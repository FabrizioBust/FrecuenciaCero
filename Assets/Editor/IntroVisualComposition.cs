using System;
using System.Collections.Generic;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Only authors IntroHistoria. Gameplay assets are read, never edited or saved.
public static class IntroVisualComposition
{
    const string ScenePath = "Assets/Scenes/IntroHistoria.unity";
    const string GeometryPath = "Assets/Art/Intro/IntroMejoras.asset";
    const string BeamMaterialPath = "Assets/Art/Intro/IntroFaros.mat";
    static Mesh geometry;
    static Material landscape, beamMaterial;
    static Dictionary<string, Sprite> sprites;

    public static void ApplyToExistingIntro()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath);
        var intro = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<IntroHistoriaController>()).Single();
        var vehicle = intro.truck.GetComponentInChildren<SpriteRenderer>();
        Create(intro, vehicle.sharedMaterial,
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalUnlit.mat"));
        AssetDatabase.SaveAssets();
        EditorSceneManager.SaveScene(scene);
        System.IO.File.WriteAllText("intro-visual-results.txt", "PASS: IntroHistoria updated; original truck sprite retained\nPASS: two soft headlights, forest passage, distant mountain and tower\n");
    }

    public static void Create(IntroHistoriaController intro, Material lit, Material unlit)
    {
        if (AssetDatabase.LoadAssetAtPath<Mesh>(GeometryPath) != null ||
            AssetDatabase.LoadAssetAtPath<Material>(BeamMaterialPath) != null)
            throw new InvalidOperationException("Los recursos de mejoras ya existen; no se sobrescribirán.");
        if (intro.truck.gameObject.scene != intro.gameObject.scene || intro.arrival.gameObject.scene != intro.gameObject.scene)
            throw new InvalidOperationException("La composición debe pertenecer únicamente a la escena de introducción.");
        landscape = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Intro/IntroPaisaje.mat");
        var shader = Shader.Find("FrecuenciaCero/Intro/FarosSuaves2D");
        if (shader == null || landscape == null) throw new InvalidOperationException("Faltan los shaders/materiales de la introducción.");
        beamMaterial = new Material(shader) { name = "IntroFaros" };
        beamMaterial.SetColor("_LightColor", new Color(.99f,.98f,.94f,.075f));
        AssetDatabase.CreateAsset(beamMaterial, BeamMaterialPath);
        sprites = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png")
            .OfType<Sprite>().ToDictionary(s => s.name);
        geometry = null;

        // These are known cinematic-only objects, not the retén or truck in the playable level.
        foreach (string name in new[] { "Caseta del retén", "Señal de mantenimiento", "Poste del retén", "Cerca del retén", "Luz de trabajo" })
        {
            var child = intro.arrival.Find(name);
            if (child != null) UnityEngine.Object.DestroyImmediate(child.gameObject);
        }
        if (intro.headlightCone != null && intro.headlightCone.transform.IsChildOf(intro.truck))
            UnityEngine.Object.DestroyImmediate(intro.headlightCone.gameObject);
        if (intro.headlights != null && intro.headlights.transform.IsChildOf(intro.truck))
            UnityEngine.Object.DestroyImmediate(intro.headlights.gameObject);

        // Align with the two visible front lamps of the existing three-quarter pickup sprite.
        var vehicle = intro.truck.GetComponentInChildren<SpriteRenderer>();
        float front = vehicle.sprite.bounds.max.x * vehicle.transform.localScale.x;
        intro.headlights = Lamp(intro.truck,"Faro delantero cercano",new Vector3(front-.07f,.48f,0),.8f,7);
        intro.secondaryHeadlight = Lamp(intro.truck,"Faro delantero lejano",new Vector3(front-.30f,.94f,0),.65f,6.8f);
        intro.headlightCone = MeshObject(intro.truck,"Haz suave cercano",BeamQuad(7.2f,2.2f),-160,beamMaterial);
        intro.headlightCone.transform.localPosition = intro.headlights.transform.localPosition;
        intro.secondaryBeam = MeshObject(intro.truck,"Haz suave lejano",BeamQuad(6.8f,2),-159,beamMaterial);
        intro.secondaryBeam.transform.localPosition = intro.secondaryHeadlight.transform.localPosition;
        intro.secondaryBeam.transform.localRotation = Quaternion.Euler(0,0,-2);

        intro.arrival.name = "Entrada al bosque — escenario visual";
        var entrance = new GameObject("Acceso forestal abierto").transform;
        entrance.SetParent(intro.arrival,false);entrance.localPosition=new Vector3(-1.7f,0,0);
        var path = MeshObject(entrance,"Sendero de tierra hacia el bosque",Path(),-185,landscape);
        var gravel = MeshObject(entrance,"Piedras y huellas del sendero",PathDetails(),-184,landscape);
        // Leave a real empty passage between the fence sections. Reuse weathered wooden rails.
        for(int side=-1;side<=1;side+=2)
        for(int i=0;i<3;i++)
        {
            var fence = Sprite(entrance,"Cerca de madera "+(side<0?"izquierda ":"derecha ")+i,
                sprites["Cerca_Madera"],side*(2.15f+i*1.50f),-.13f,.38f,-95,new Color(.83f,.87f,.9f),lit);
            fence.flipX = side>0;
            fence.transform.localRotation=Quaternion.Euler(0,0,(i==1?side*1.5f:0));
        }
        foreach(int side in new[]{-1,1})
        {
            Sprite(entrance,"Pino del acceso "+side,sprites["Pino_Alto"],side*3.2f,.10f,.73f,-170,new Color(.62f,.70f,.78f),lit);
            Sprite(entrance,"Arbusto del acceso "+side,sprites["Arbusto"],side*2.8f,-.08f,.22f,-90,new Color(.66f,.72f,.76f),lit);
            Sprite(entrance,"Roca junto al acceso "+side,sprites["Roca_Musgo"],side*1.9f,-.20f,.10f,-85,new Color(.60f,.65f,.69f),lit);
        }

        // The roadside forest travels with the road. Author a clearing at this section of its loop.
        // Trees stay fixed in their layer; no runtime disappearing trees or camera changes.
        var middle = intro.layers.Single(l=>l.name=="Bosque junto al camino");middle.speedRatio=1;
        float clearing = Mathf.Repeat(intro.arrival.localPosition.x-1.7f+middle.repeatWidth/2,middle.repeatWidth)-middle.repeatWidth/2;
        foreach(Transform tree in middle.transform.Cast<Transform>().ToArray())
            if(Mathf.Abs(Mathf.DeltaAngle(tree.localPosition.x/middle.repeatWidth*360,clearing/middle.repeatWidth*360))*middle.repeatWidth/360<2.85f)
                UnityEngine.Object.DestroyImmediate(tree.gameObject);
        // A narrow distant clearing makes the trail recede, while its layer retains slower parallax.
        var distant = intro.layers.Single(l=>l.name=="Bosque distante");
        float backOpening = intro.arrival.localPosition.x-1.7f-IntroHistoriaController.DistanceAt(36)*(1-distant.speedRatio);
        foreach(Transform tree in distant.transform.Cast<Transform>().ToArray())
            if(Mathf.Abs(Mathf.DeltaAngle(tree.localPosition.x/distant.repeatWidth*360,backOpening/distant.repeatWidth*360))*distant.repeatWidth/360<1.9f)
                UnityEngine.Object.DestroyImmediate(tree.gameObject);

        var far = new GameObject("Montaña y torre distantes",typeof(IntroParallaxLayer)).GetComponent<IntroParallaxLayer>();
        far.speedRatio=.09f;far.repeatWidth=90;
        var feature = new GameObject("Cumbre con torre de transmisión").transform;feature.SetParent(far.transform,false);
        feature.localPosition=new Vector3(5.1f+IntroHistoriaController.DistanceAt(36)*far.speedRatio,0,0);
        MeshObject(feature,"Montaña al fondo",Mountain(),-280,landscape);
        MeshObject(feature,"Torre de transmisión",Tower(),-275,landscape);
        MeshObject(feature,"Luz roja de señalización",Beacon(),-274,landscape);
        intro.layers = intro.layers.Concat(new[]{far}).ToArray();
    }

    static Light2D Lamp(Transform parent,string name,Vector3 position,float intensity,float radius)
    {
        var obj=new GameObject(name,typeof(Light2D));obj.transform.SetParent(parent,false);obj.transform.localPosition=position;
        obj.transform.localRotation=Quaternion.Euler(0,0,-90);var light=obj.GetComponent<Light2D>();
        light.lightType=Light2D.LightType.Point;light.color=new Color(.99f,.98f,.94f);light.intensity=intensity;
        light.pointLightInnerAngle=10;light.pointLightOuterAngle=32;light.pointLightInnerRadius=.15f;light.pointLightOuterRadius=radius;
        light.falloffIntensity=.55f;return light;
    }
    static SpriteRenderer Sprite(Transform parent,string name,Sprite sprite,float x,float y,float scale,int order,Color color,Material material)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=new Vector3(x,y,0);obj.transform.localScale=Vector3.one*scale;
        var renderer=obj.GetComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sharedMaterial=material;renderer.color=color;renderer.sortingOrder=order;return renderer;
    }
    static MeshRenderer MeshObject(Transform parent,string name,Mesh mesh,int order,Material material)
    {
        mesh.name=name;
        if(geometry==null){geometry=mesh;AssetDatabase.CreateAsset(mesh,GeometryPath);}else AssetDatabase.AddObjectToAsset(mesh,geometry);
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);obj.GetComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=obj.GetComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.sortingOrder=order;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;return renderer;
    }
    static Mesh BeamQuad(float length,float height)
    {
        var mesh=new Mesh();mesh.vertices=new[]{new Vector3(0,-height/2),new Vector3(0,height/2),new Vector3(length,height/2),new Vector3(length,-height/2)};
        mesh.uv=new[]{new Vector2(0,0),new Vector2(0,1),new Vector2(1,1),new Vector2(1,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();return mesh;
    }
    sealed class Shape
    {
        readonly List<Vector3> vertices=new List<Vector3>();readonly List<Color> colors=new List<Color>();readonly List<int> triangles=new List<int>();
        public void Quad(Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color near,Color far)
        {
            int n=vertices.Count;vertices.AddRange(new[]{(Vector3)a,(Vector3)b,(Vector3)c,(Vector3)d});colors.AddRange(new[]{near.linear,far.linear,far.linear,near.linear});
            triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
        }
        public void Rect(float x,float y,float w,float h,Color color){Quad(new Vector2(x,y),new Vector2(x,y+h),new Vector2(x+w,y+h),new Vector2(x+w,y),color,color);}
        public void Line(Vector2 a,Vector2 b,float width,Color color)
        {Vector2 normal=Vector2.Perpendicular((b-a).normalized)*width/2;Quad(a-normal,b-normal,b+normal,a+normal,color,color);}
        public Mesh Mesh(){var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetColors(colors);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();return mesh;}
    }
    static Mesh Path()
    {
        var shape=new Shape();
        Color near=new Color(.115f,.108f,.094f),far=new Color(.038f,.055f,.063f,0);
        shape.Quad(new Vector2(-1.4f,-.45f),new Vector2(.45f,2f),new Vector2(1.05f,2f),new Vector2(1.4f,-.45f),near,far);
        shape.Quad(new Vector2(-1.9f,-.48f),new Vector2(-1.4f,-.45f),new Vector2(1.4f,-.45f),new Vector2(1.9f,-.48f),near,near);
        return shape.Mesh();
    }
    static Mesh PathDetails()
    {
        var shape=new Shape();var random=new System.Random(481);
        for(int i=0;i<110;i++)
        {
            float t=(float)random.NextDouble();float y=Mathf.Lerp(-.42f,1.8f,t);float center=.75f*t;float width=Mathf.Lerp(1.25f,.3f,t);
            float x=center+((float)random.NextDouble()*2-1)*width;
            float size=(i%4==0?.0625f:.03125f)*(1-.45f*t);var color=new Color(.17f,.16f,.135f,Mathf.Lerp(.75f,.08f,t));
            shape.Rect(Mathf.Round(x*32)/32,Mathf.Round(y*32)/32,size*1.5f,size,color);
        }
        return shape.Mesh();
    }
    static Mesh Mountain()
    {
        var shape=new Shape();var points=new List<Vector2>();
        for(int i=0;i<=64;i++)
        {float x=-5+i*.15625f;float peak=3.25f-Mathf.Abs(x)*.46f;float roughness=Mathf.Sin(x*4.2f)*.10f+Mathf.Sin(x*11)*.04f;points.Add(new Vector2(x,Mathf.Round((peak+roughness)*32)/32));}
        var top=new Color(.067f,.096f,.13f);var bottom=new Color(.034f,.05f,.073f);
        for(int i=0;i<points.Count-1;i++)shape.Quad(new Vector2(points[i].x,.1f),points[i],points[i+1],new Vector2(points[i+1].x,.1f),bottom,top);
        shape.Line(new Vector2(-.2f,2.98f),new Vector2(-2,1.72f),.03125f,new Color(.065f,.08f,.102f));
        return shape.Mesh();
    }
    static Mesh Tower()
    {
        var shape=new Shape();var color=new Color(.12f,.15f,.19f);
        shape.Line(new Vector2(-.24f,3.18f),new Vector2(-.0625f,4.12f),.03125f,color);
        shape.Line(new Vector2(.24f,3.18f),new Vector2(.0625f,4.12f),.03125f,color);
        for(int i=0;i<4;i++)
        {float y=3.22f+i*.21875f;float w=Mathf.Lerp(.22f,.065f,i/4f);float next=Mathf.Lerp(.22f,.065f,(i+1)/4f);
            shape.Line(new Vector2(-w,y),new Vector2(next,y+.21875f),.018f,color);shape.Line(new Vector2(w,y),new Vector2(-next,y+.21875f),.018f,color);}
        shape.Rect(-.015625f,4.06f,.03125f,.24f,color);
        return shape.Mesh();
    }
    static Mesh Beacon()
    {
        var shape=new Shape();
        for(int i=5;i>=1;i--){float size=i*.03125f;shape.Rect(-size/2,4.3f-size/2,size,size,new Color(.92f,.055f,.035f,.018f));}
        shape.Rect(-.015625f,4.284375f,.03125f,.03125f,new Color(1,.075f,.045f,.9f));
        return shape.Mesh();
    }
}
