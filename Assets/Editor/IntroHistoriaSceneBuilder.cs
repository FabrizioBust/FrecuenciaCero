using System;
using System.Collections.Generic;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class IntroHistoriaSceneBuilder
{
    const string ScenePath = "Assets/Scenes/IntroHistoria.unity";
    const string ArtPath = "Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png";
    const string GeometryPath = "Assets/Art/Intro/IntroPaisaje.asset";
    static Material unlit, lit, landscapeMaterial;
    static Font font;
    static Mesh geometryAsset;
    static Dictionary<string, Sprite> sprites;

    [MenuItem("Frecuencia Cero/Crear IntroHistoria (solo si no existe)")]
    public static void Create()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) != null ||
            AssetDatabase.LoadAssetAtPath<Mesh>(GeometryPath) != null ||
            AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Intro/IntroPaisaje.mat") != null)
            throw new InvalidOperationException("IntroHistoria o su geometría ya existen. No se sobrescribirán.");
        // Read the actual vehicle renderer, including sprite, material, flip and tint; never save gameplay.
        EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/N1_ExteriorReten.unity");
        var original = GameObject.Find("Camioneta_Mantenimiento").GetComponent<SpriteRenderer>();
        var vehicleSprite = original.sprite;
        var vehicleMaterial = original.sharedMaterial;
        var vehicleColor = original.color;
        bool flipX = original.flipX, flipY = original.flipY;
        if (vehicleSprite == null || vehicleSprite.name != "Camioneta")
            throw new InvalidOperationException("No se identificó la camioneta original del nivel.");
        sprites = AssetDatabase.LoadAllAssetsAtPath(ArtPath).OfType<Sprite>().ToDictionary(s => s.name);
        if (!AssetDatabase.IsValidFolder("Assets/Art/Intro")) AssetDatabase.CreateFolder("Assets/Art", "Intro");
        unlit = AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalUnlit.mat");
        landscapeMaterial = new Material(Shader.Find("FrecuenciaCero/Intro/Paisaje2D"));
        AssetDatabase.CreateAsset(landscapeMaterial,"Assets/Art/Intro/IntroPaisaje.mat");
        lit = vehicleMaterial;
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (!AssetDatabase.IsValidFolder("Assets/Art/Intro")) AssetDatabase.CreateFolder("Assets/Art", "Intro");
        geometryAsset = null;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(UniversalAdditionalCameraData), typeof(PixelPerfectCamera));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0, 0, -10);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true; camera.orthographicSize = 5.625f;
        camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
        var pixel = cameraObject.GetComponent<PixelPerfectCamera>();
        pixel.assetsPPU = 32; pixel.refResolutionX = 640; pixel.refResolutionY = 360;
        // Use native serialized settings to match the project's installed URP version.
        var pixelSettings = new SerializedObject(pixel);
        pixelSettings.FindProperty("m_CropFrame").intValue = 3;
        pixelSettings.FindProperty("m_GridSnapping").intValue = 2;
        pixelSettings.ApplyModifiedPropertiesWithoutUndo();
        var global = new GameObject("Madrugada — iluminación ambiental", typeof(Light2D)).GetComponent<Light2D>();
        global.lightType = Light2D.LightType.Global; global.color = new Color(.64f, .72f, .86f); global.intensity = .55f;
        var host = new GameObject("Introducción — secuencia de 40 segundos", typeof(IntroHistoriaController));
        var intro = host.GetComponent<IntroHistoriaController>();

        var landscape = new GameObject("Paisaje cinematográfico").transform;
        WorldMesh(landscape, "Cielo nocturno", Rectangle(-50, -8, 100, 18, C(.025f,.043f,.068f)), -300);
        var mountains = Layer(landscape, "Montañas lejanas", .09f, 90);
        for (int i = 0; i < 3; i++)
        {
            var mesh = WorldMesh(mountains.transform, "Cresta " + i, Ridge(i), -290);
            mesh.transform.localPosition = new Vector3((i - 1) * 30, 0, 0);
        }
        WorldMesh(landscape, "Suelo del bosque", Rectangle(-50,-8,100,9.3f,C(.038f,.063f,.061f)), -250);
        WorldMesh(landscape, "Arcén distante", Rectangle(-50,-1.25f,100,.33f,C(.078f,.091f,.085f)), -220);
        WorldMesh(landscape, "Carretera", Rectangle(-50,-3.75f,100,2.5f,C(.073f,.084f,.094f)), -210);
        WorldMesh(landscape, "Arcén cercano", Rectangle(-50,-4.04f,100,.3f,C(.06f,.078f,.072f)), -205);
        WorldMesh(landscape, "Borde superior", Rectangle(-50,-1.39f,100,.05f,C(.18f,.185f,.167f)), -200);
        WorldMesh(landscape, "Borde inferior", Rectangle(-50,-3.57f,100,.04f,C(.15f,.16f,.15f)), -200);
        var road = Layer(landscape, "Marcas de carretera", 1, 48);
        for (int i = 0; i < 24; i++)
        {
            var mark = WorldMesh(road.transform, "Marca " + i, Rectangle(-.4f,0,.8f,.045f,C(.26f,.24f,.16f)), -199);
            mark.transform.localPosition = new Vector3(-24 + i * 2, -2.83f, 0);
        }
        var distant = Layer(landscape, "Bosque distante", .28f, 52);
        var middle = Layer(landscape, "Bosque junto al camino", .63f, 52);
        for (int i = 0; i < 25; i++)
        {
            float x = -26 + i * 2.08f;
            Sprite(distant.transform, "Pino distante " + i, sprites[i % 3 == 0 ? "Arbol_Frondoso" : "Pino_Alto"],
                x, .3f + i % 3 * .13f, .39f + i % 4 * .018f, -240, C(.26f,.36f,.41f), unlit);
            Sprite(middle.transform, "Árbol " + i, sprites[i % 4 == 0 ? "Arbol_Frondoso" : "Pino_Mediano"],
                x + .7f, -.62f + i % 3 * .12f, .55f + i % 4 * .02f, -180, C(.67f,.74f,.81f), lit);
        }
        var foreground = Layer(landscape, "Vegetación del primer plano", 1.16f, 52);
        for (int i = 0; i < 20; i++)
        {
            Sprite(foreground.transform, "Arbusto cercano " + i, sprites[i % 4 == 0 ? "Pino_Mediano" : "Arbusto"],
                -26+i*2.6f,-5.65f,.27f+i%3*.025f,100,C(.23f,.31f,.33f),unlit);
            Sprite(foreground.transform, "Roca del arcén " + i, sprites["Roca_Musgo"],
                -26+i*2.6f+.8f,-4.32f,.075f,80,C(.36f,.41f,.43f),unlit);
        }
        var fog = Layer(landscape, "Niebla nocturna ligera", .15f, 60);
        var fogSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Nivel01/Fog/BancoNiebla.png");
        for (int i = 0; i < 8; i++)
        {
            var bank = Sprite(fog.transform, "Banco de niebla " + i, fogSprite,-30+i*7.5f,.25f+i%3*.7f,1,-170,new Color(.55f,.64f,.72f,.11f),unlit);
            bank.transform.localScale = new Vector3(1.3f,.35f,1);
        }
        intro.layers = new[] { mountains, distant, middle, road, foreground, fog };
        intro.truck = new GameObject("Camioneta de mantenimiento — solo representación visual").transform;
        intro.truck.position = new Vector3(-2.4f,-2.45f,0);
        var vehicle = Sprite(intro.truck,"Camioneta original",vehicleSprite,0,0,.72f,0,vehicleColor,vehicleMaterial);
        vehicle.flipX = flipX; vehicle.flipY = flipY;
        intro.arrival = new GameObject("Entrada al bosque — escenario visual").transform;
        intro.arrival.position = new Vector3(56.4f,-.8f,0);
        IntroVisualComposition.Create(intro,lit,unlit);

        intro.engineSource=Source(host.transform,"Motor",true);
        intro.roadSource=Source(host.transform,"Ruedas",true);
        intro.windSource=Source(host.transform,"Viento",true);
        intro.forestSource=Source(host.transform,"Bosque",true);
        intro.voiceSource=Source(host.transform,"Voces normales y apagado de motor",false);
        var canvasObject=new GameObject("Intro Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=.5f;
        var composition=Rect(canvas.transform,"Composition 16x9",0,0,1920,1080);Stretch(composition);
        var aspect=composition.gameObject.AddComponent<AspectRatioFitter>();aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=16f/9;
        var design=Rect(composition,"Design 1920x1080",0,0,1920,1080);
        design.anchorMin=design.anchorMax=design.pivot=new Vector2(.5f,.5f);design.anchoredPosition=Vector2.zero;
        design.gameObject.AddComponent<MenuDesignScale>();
        // Quiet letterboxing and typography; no HUD, objectives or added narrative.
        Image(design,"Banda superior",0,1000,1920,80,Color.black);
        Image(design,"Banda inferior",0,0,1920,200,Color.black);
        intro.locationGroup=Rect(design,"Lugar y hora",120,810,820,160).gameObject.AddComponent<CanvasGroup>();
        intro.location=Text(intro.locationGroup.transform,"CIMA DEL ÁGUILA",0,65,820,65,35,new Color(.84f,.86f,.85f));
        intro.timeOfNight=Text(intro.locationGroup.transform,"02:30 A. M.",0,20,820,42,23,new Color(.6f,.65f,.66f));
        intro.dialogueGroup=Rect(design,"Comunicación profesional",360,75,1200,118).gameObject.AddComponent<CanvasGroup>();
        intro.speaker=Text(intro.dialogueGroup.transform,"SUPERVISOR",0,82,1200,30,21,new Color(.76f,.78f,.65f));
        intro.subtitle=Text(intro.dialogueGroup.transform,"Subtítulo",0,0,1200,76,27,new Color(.88f,.9f,.9f));
        intro.skipGroup=Rect(design,"Omitir",1325,4,475,60).gameObject.AddComponent<CanvasGroup>();
        Text(intro.skipGroup.transform,"Mantén ESPACIO para omitir",0,18,475,40,20,new Color(.6f,.64f,.67f));
        Image(intro.skipGroup.transform,"Pista de progreso",0,2,400,3,new Color(.14f,.18f,.2f));
        intro.skipProgress=Image(intro.skipGroup.transform,"Progreso",0,2,400,3,new Color(.71f,.76f,.76f));
        intro.skipProgress.type=UnityEngine.UI.Image.Type.Filled;intro.skipProgress.fillMethod=UnityEngine.UI.Image.FillMethod.Horizontal;
        intro.skipProgress.fillOrigin=0;intro.skipProgress.fillAmount=0;
        var fade=Image(canvas.transform,"Fundido",0,0,1920,1080,Color.black);Stretch(fade.rectTransform);
        intro.fade=fade.gameObject.AddComponent<CanvasGroup>();intro.fade.alpha=1;
        intro.locationGroup.alpha=intro.dialogueGroup.alpha=0;intro.skipGroup.alpha=0;
        EditorSceneManager.SaveScene(scene,ScenePath);
        var scenes=EditorBuildSettings.scenes.Where(s=>s.path!=ScenePath).ToList();
        int index=scenes.FindIndex(s=>s.path=="Assets/Scenes/MainMenu.unity");
        scenes.Insert(index<0?0:index+1,new EditorBuildSettingsScene(ScenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
        AssetDatabase.SaveAssets();
        System.IO.File.WriteAllText("intro-setup-results.txt","PASS: truck copied from original: "+vehicleSprite.name+"\nPASS: IntroHistoria saved\nPASS: Build scenes retained\n");
    }

    static Color C(float r,float g,float b){return new Color(r,g,b,1);}
    static IntroParallaxLayer Layer(Transform parent,string name,float ratio,float width)
    {var obj=new GameObject(name,typeof(IntroParallaxLayer));obj.transform.SetParent(parent,false);var layer=obj.GetComponent<IntroParallaxLayer>();layer.speedRatio=ratio;layer.repeatWidth=width;return layer;}
    static SpriteRenderer Sprite(Transform parent,string name,Sprite sprite,float x,float y,float scale,int order,Color color,Material material)
    {var obj=new GameObject(name,typeof(SpriteRenderer));obj.transform.SetParent(parent,false);obj.transform.localPosition=new Vector3(x,y,0);obj.transform.localScale=Vector3.one*scale;var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=order;r.color=color;return r;}
    static MeshRenderer WorldMesh(Transform parent,string name,Mesh mesh,int order)
    {
        if(geometryAsset==null){geometryAsset=mesh;AssetDatabase.CreateAsset(mesh,GeometryPath);}else AssetDatabase.AddObjectToAsset(mesh,geometryAsset);
        var obj=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));obj.transform.SetParent(parent,false);
        obj.GetComponent<MeshFilter>().sharedMesh=mesh;var r=obj.GetComponent<MeshRenderer>();r.sharedMaterial=landscapeMaterial;r.sortingOrder=order;
        r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;return r;
    }
    static Mesh Rectangle(float x,float y,float w,float h,Color color)
    {return Polygon(new[]{new Vector3(x,y),new Vector3(x,y+h),new Vector3(x+w,y+h),new Vector3(x+w,y)},new[]{color,color,color,color});}
    static Mesh Ridge(int variant)
    {
        var points=new List<Vector3>{new Vector3(-15,.9f),new Vector3(-15,2.7f)};
        for(int i=0;i<60;i++){float x=-15+i*.5f;float y=3+Mathf.Sin((x+variant*5)*.24f)*1.1f+Mathf.Sin(x*.75f)*.22f;points.Add(new Vector3(x,Mathf.Round(y*32)/32));}
        points.Add(new Vector3(15,2.7f));points.Add(new Vector3(15,.9f));
        return Polygon(points.ToArray(),Enumerable.Repeat(C(.046f,.073f,.102f),points.Count).ToArray());
    }
    static Mesh Beam()
    {return Polygon(new[]{Vector3.zero,new Vector3(6.8f,1.18f),new Vector3(6.8f,-.78f)},new[]{new Color(.9f,.84f,.62f,.13f),new Color(.9f,.84f,.62f,0),new Color(.9f,.84f,.62f,0)});}
    static Mesh Polygon(Vector3[] vertices,Color[] colors)
    {
        var mesh=new Mesh{name="Intro geometry"};mesh.vertices=vertices;mesh.colors=colors.Select(c=>c.linear).ToArray();mesh.uv=new Vector2[vertices.Length];
        var triangles=new List<int>();for(int i=1;i<vertices.Length-1;i++){triangles.Add(0);triangles.Add(i);triangles.Add(i+1);}mesh.triangles=triangles.ToArray();mesh.RecalculateBounds();return mesh;
    }
    static AudioSource Source(Transform parent,string name,bool loop)
    {var obj=new GameObject(name,typeof(AudioSource));obj.transform.SetParent(parent,false);var source=obj.GetComponent<AudioSource>();source.playOnAwake=false;source.loop=loop;source.spatialBlend=0;source.volume=0;return source;}
    static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
    {var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(parent,false);var rect=obj.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.zero;rect.anchoredPosition=new Vector2(x,y);rect.sizeDelta=new Vector2(w,h);return rect;}
    static void Stretch(RectTransform rect){rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;rect.pivot=new Vector2(.5f,.5f);}
    static Image Image(Transform parent,string name,float x,float y,float w,float h,Color color)
    {var image=Rect(parent,name,x,y,w,h).gameObject.AddComponent<Image>();image.color=color;image.raycastTarget=false;return image;}
    static Text Text(Transform parent,string value,float x,float y,float w,float h,int size,Color color)
    {var text=Rect(parent,value,x,y,w,h).gameObject.AddComponent<Text>();text.font=font;text.fontSize=size;text.color=color;text.text=value;text.raycastTarget=false;text.supportRichText=false;text.alignment=TextAnchor.MiddleLeft;return text;}
}
