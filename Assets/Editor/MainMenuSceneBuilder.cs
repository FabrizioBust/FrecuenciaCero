using System;
using System.Linq;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public static class MainMenuSceneBuilder
{
    const string Path = "Assets/Scenes/MainMenu.unity";
    static Font font;
    static readonly Color Red = new Color(.94f,.12f,.15f);

    [MenuItem("Frecuencia Cero/Crear MainMenu (solo si no existe)")]
    public static void Create()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(Path) != null)
            throw new InvalidOperationException("MainMenu ya existe. Edita la escena; no se sobrescribirá.");
        AssetDatabase.Refresh();
        var importer=AssetImporter.GetAtPath("Assets/Art/Menu/Menu_FondoLimpio.png") as TextureImporter;
        if(importer!=null)
        {
            importer.filterMode=FilterMode.Point;importer.mipmapEnabled=false;
            importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
        }
        font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));
        camera.tag="MainCamera";
        camera.transform.position=new Vector3(0,0,-10);
        var cam=camera.GetComponent<Camera>();
        cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.cullingMask=0;
        var events=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        var canvas=new GameObject("Menu Canvas",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        canvas.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;
        var scale=canvas.GetComponent<CanvasScaler>();
        scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scale.referenceResolution=new Vector2(1920,1080);scale.matchWidthOrHeight=.5f;
        var letterbox=Image(canvas.transform,"Letterbox",0,0,1920,1080,Color.black);Stretch(letterbox.rectTransform);
        var composition=Rect(canvas.transform,"Composition 16x9",0,0,1920,1080);
        Stretch(composition);var aspect=composition.gameObject.AddComponent<AspectRatioFitter>();
        aspect.aspectMode=AspectRatioFitter.AspectMode.FitInParent;aspect.aspectRatio=16f/9;
        // Fixed design coordinates are uniformly scaled within this aspect-fitted parent.
        var design=Rect(composition,"Design 1920x1080",0,0,1920,1080);
        design.anchorMin=design.anchorMax=new Vector2(.5f,.5f);design.pivot=new Vector2(.5f,.5f);
        design.anchoredPosition=Vector2.zero;
        design.gameObject.AddComponent<MenuDesignScale>();
        var background=Rect(design,"Forest Background",0,0,1920,1080).gameObject.AddComponent<RawImage>();
        background.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Menu/Menu_FondoLimpio.png");background.raycastTarget=false;
        Rect(design,"Ambient Fog and Beacon",0,0,1920,1080).gameObject.AddComponent<MenuAtmosphereGraphic>();
        var host=new GameObject("Main Menu Controller",typeof(AudioSource),typeof(MainMenuController));
        var menu=host.GetComponent<MainMenuController>();
        host.GetComponent<AudioSource>().playOnAwake=false;
        menu.menuMusic=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/MusicaMenu.mp3");
        if(background.texture==null || menu.menuMusic==null) throw new InvalidOperationException("Faltan fondo o MusicaMenu.mp3.");
        menu.mainPage=Page(design,"Main");
        var title=Rect(menu.mainPage.transform,"FRECUENCIA CERO Title",110,720,760,253).gameObject.AddComponent<RawImage>();
        title.texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Menu/Menu_Titulo.png");title.raycastTarget=false;
        title.gameObject.AddComponent<MenuTitleInterference>();
        menu.newGame=Button(menu.mainPage.transform,"NUEVA PARTIDA",160,575,490,64);
        menu.options=Button(menu.mainPage.transform,"OPCIONES",160,482,360,64);
        menu.credits=Button(menu.mainPage.transform,"CRÉDITOS",160,389,360,64);
        menu.quit=Button(menu.mainPage.transform,"SALIR",160,296,360,64);
        Link(menu.newGame,menu.options,menu.credits,menu.quit);

        menu.optionsPage=Page(design,"Options");
        Image(menu.optionsPage.transform,"Options Panel",120,125,1080,830,new Color(.015f,.025f,.035f,.94f));
        Text(menu.optionsPage.transform,"OPCIONES",175,845,900,65,49,Red,FontStyle.Bold);
        Text(menu.optionsPage.transform,"AUDIO",175,750,600,50,27,Color.white,FontStyle.Bold);
        Text(menu.optionsPage.transform,"Volumen de música",175,673,450,45,27,Color.white);
        menu.musicVolume=Slider(menu.optionsPage.transform,"Music Volume",610,680);
        menu.musicPercent=Text(menu.optionsPage.transform,"Music Percent",1010,673,140,45,25,Red);
        Text(menu.optionsPage.transform,"Volumen de efectos",175,576,450,45,27,Color.white);
        menu.effectsVolume=Slider(menu.optionsPage.transform,"Effects Volume",610,583);
        menu.effectsPercent=Text(menu.optionsPage.transform,"Effects Percent",1010,576,140,45,25,Red);
        Text(menu.optionsPage.transform,"PANTALLA",175,460,600,50,27,Color.white,FontStyle.Bold);
        var toggle=DefaultControls.CreateToggle(new DefaultControls.Resources());toggle.name="Fullscreen";
        toggle.transform.SetParent(menu.optionsPage.transform,false);Place(toggle.GetComponent<RectTransform>(),610,385,450,45);
        menu.fullscreen=toggle.GetComponent<Toggle>();
        var toggleLabel=toggle.GetComponentInChildren<Text>();Style(toggleLabel,27);toggleLabel.text="Pantalla completa";
        toggleLabel.color=Color.white;
        toggle.transform.Find("Background").GetComponent<Image>().color=new Color(.2f,.25f,.29f);
        menu.fullscreen.graphic.color=Red;
        Text(menu.optionsPage.transform,"Resolución",175,285,400,50,27,Color.white);
        var dropdown=DefaultControls.CreateDropdown(new DefaultControls.Resources());dropdown.name="Resolution";
        dropdown.transform.SetParent(menu.optionsPage.transform,false);Place(dropdown.GetComponent<RectTransform>(),610,285,450,52);
        menu.resolution=dropdown.GetComponent<Dropdown>();
        foreach(var text in dropdown.GetComponentsInChildren<Text>(true))Style(text,25);
        menu.resolution.captionText.color=Color.white;
        dropdown.GetComponent<Image>().color=new Color(.11f,.13f,.16f);
        var arrow=dropdown.transform.Find("Arrow");arrow.GetComponent<Image>().color=Color.clear;
        var arrowText=Text(arrow,"▼",0,0,20,20,18,Red);Stretch(arrowText.rectTransform);
        menu.optionsBack=Button(menu.optionsPage.transform,"VOLVER",175,175,280,60);
        Link(menu.musicVolume,menu.effectsVolume,menu.fullscreen,menu.resolution,menu.optionsBack);

        menu.creditsPage=Page(design,"Credits");
        Image(menu.creditsPage.transform,"Credits Panel",260,110,1400,860,new Color(.008f,.017f,.026f,.96f));
        Text(menu.creditsPage.transform,"CRÉDITOS",325,865,1250,65,49,Red,FontStyle.Bold);
        Text(menu.creditsPage.transform,"EQUIPO DE DESARROLLO",325,775,1250,45,27,Red,FontStyle.Bold);
        Text(menu.creditsPage.transform,"Team",325,552,1250,205,29,new Color(.86f,.87f,.87f)).text=
            "Bustios Rodriguez, Fabrizio Imanol\nSanchez Valdivieso, Alexander Benito\nCotrina Quiroz, Jorge Alexander\nSandoval Lopez, Rood Van";
        Text(menu.creditsPage.transform,"MÚSICA DEL MENÚ",325,482,1250,45,27,Red,FontStyle.Bold);
        Text(menu.creditsPage.transform,"Music Attribution",325,312,1250,157,24,new Color(.83f,.85f,.87f)).text=
            "SCP-x5x (Outer Thoughts) — Kevin MacLeod\n"+
            "Origen: https://incompetech.com/music/royalty-free/index.html?isrc=USUAN2000011\n"+
            "Creative Commons Attribution 4.0 International (CC BY 4.0)\n"+
            "https://creativecommons.org/licenses/by/4.0/\nPista original; reproducción en bucle y ajustes de volumen.";
        Text(menu.creditsPage.transform,"MOTOR DE DESARROLLO",325,252,1250,45,27,Red,FontStyle.Bold);
        Text(menu.creditsPage.transform,"Unity",325,207,1250,40,27,Color.white);
        menu.creditsBack=Button(menu.creditsPage.transform,"VOLVER",325,135,280,55);
        var fadeImage=Image(canvas.transform,"Fade",0,0,1920,1080,Color.black);Stretch(fadeImage.rectTransform);
        fadeImage.raycastTarget=true;menu.fade=fadeImage.gameObject.AddComponent<CanvasGroup>();
        // Keep the scene directly editable; runtime handles actions, not UI construction.
        menu.fade.alpha=0;
        menu.optionsPage.gameObject.SetActive(false);menu.creditsPage.gameObject.SetActive(false);
        EditorSceneManager.SaveScene(scene,Path);
        var existing=EditorBuildSettings.scenes.Where(s=>s.path!=Path).ToList();
        existing.Insert(0,new EditorBuildSettingsScene(Path,true));EditorBuildSettings.scenes=existing.ToArray();
        AssetDatabase.SaveAssets();
    }
    static RectTransform Rect(Transform parent,string name,float x,float y,float w,float h)
    {
        var obj=new GameObject(name,typeof(RectTransform));obj.transform.SetParent(parent,false);
        var rect=obj.GetComponent<RectTransform>();Place(rect,x,y,w,h);return rect;
    }
    static void Place(RectTransform r,float x,float y,float w,float h)
    {r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=new Vector2(x,y);r.sizeDelta=new Vector2(w,h);}
    static void Stretch(RectTransform r)
    {r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.pivot=new Vector2(.5f,.5f);}
    static Image Image(Transform p,string n,float x,float y,float w,float h,Color c)
    {var i=Rect(p,n,x,y,w,h).gameObject.AddComponent<Image>();i.color=c;i.raycastTarget=false;return i;}
    static Text Text(Transform p,string value,float x,float y,float w,float h,int size,Color color,FontStyle style=FontStyle.Normal)
    {
        var text=Rect(p,value,x,y,w,h).gameObject.AddComponent<Text>();Style(text,size);
        text.text=value;text.color=color;text.fontStyle=style;text.alignment=TextAnchor.MiddleLeft;return text;
    }
    static void Style(Text text,int size)
    {text.font=font;text.fontSize=size;text.raycastTarget=false;text.supportRichText=false;}
    static CanvasGroup Page(Transform parent,string name)
    {return Rect(parent,name,0,0,1920,1080).gameObject.AddComponent<CanvasGroup>();}
    static Button Button(Transform parent,string label,float x,float y,float w,float h)
    {
        var image=Image(parent,label,x,y,w,h,new Color(0,0,0,.6f));image.raycastTarget=true;
        var button=image.gameObject.AddComponent<Button>();button.transition=Selectable.Transition.None;
        var borderRect=Rect(image.transform,"Border",0,0,w,h);
        var border=borderRect.gameObject.AddComponent<MenuBorderGraphic>();border.raycastTarget=false;
        var highlight=image.gameObject.AddComponent<MenuButtonHighlight>();
        highlight.label=Text(image.transform,label,66,0,w-72,h,29,new Color(.8f,.79f,.75f));
        highlight.marker=Text(image.transform,"▶",24,0,40,h,21,Color.clear);
        // A thin separate line is used by the animated highlight; the full outline uses its color.
        highlight.border=Image(image.transform,"Selection Edge",0,0,3,h,Red);
        border.edge=highlight.border;
        return button;
    }
    static Slider Slider(Transform parent,string name,float x,float y)
    {
        var obj=DefaultControls.CreateSlider(new DefaultControls.Resources());obj.name=name;
        obj.transform.SetParent(parent,false);Place(obj.GetComponent<RectTransform>(),x,y,370,32);
        var slider=obj.GetComponent<Slider>();slider.minValue=0;slider.maxValue=1;
        slider.fillRect.GetComponent<Image>().color=Red;slider.handleRect.GetComponent<Image>().color=Color.white;
        obj.transform.Find("Background").GetComponent<Image>().color=new Color(.2f,.25f,.29f);
        return slider;
    }
    static void Link(params Selectable[] values)
    {
        for(int i=0;i<values.Length;i++)
        {
            var nav=values[i].navigation;nav.mode=Navigation.Mode.Explicit;
            nav.selectOnUp=values[(i+values.Length-1)%values.Length];nav.selectOnDown=values[(i+1)%values.Length];
            values[i].navigation=nav;
        }
    }
}
