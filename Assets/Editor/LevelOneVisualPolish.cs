using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using FrecuenciaCero;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Explicit, additive authoring pass. It refuses to repeat or overwrite an existing pass.
public static class LevelOneVisualPolish
{
    const string Art = "Assets/Art/Nivel01/VisualPolish/";
    const string RootName = "Pulido forestal — grupos naturales";
    static Dictionary<string,Sprite> sprites;
    static Material lit;
    static Sprite litter, twig;
    static bool forest;
    static readonly Vector2[] GeneratorClusters = {
        new Vector2(-14,6),new Vector2(-11,9),new Vector2(-1,9),new Vector2(2,10),
        new Vector2(12,8),new Vector2(14,4),new Vector2(12,-1),new Vector2(11,-7),
        new Vector2(9,-13),new Vector2(-12,-13),new Vector2(-9,-18),new Vector2(7,-19),
        new Vector2(-15,0),new Vector2(-8,-3),new Vector2(6,-4)
    };
    static readonly Vector2[] ForestClusters = {
        new Vector2(-14,12),new Vector2(-2,13),new Vector2(4,11),new Vector2(10,10),
        new Vector2(14,5),new Vector2(14,-2),new Vector2(14,-13),new Vector2(5,-13),
        new Vector2(-2,-13),new Vector2(-10,-11),new Vector2(-14,-4),new Vector2(-14,2),
        new Vector2(-11,3),new Vector2(1,6)
    };
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Detener Play antes del pulido.");
        foreach (string name in new[]{"N1_Generador","N1_BosqueAbandonado"})
        {
            EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/"+name+".unity");
            if (GameObject.Find(RootName)!=null) throw new InvalidOperationException("El pulido ya existe; no se repetirá.");
        }
        Directory.CreateDirectory(Art);
        sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png").OfType<Sprite>().ToDictionary(s=>s.name);
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        litter=Bake(Art+"Hojarasca.png",32,18,32,(x,y)=>
        {
            if(x<2||x>29||y<2||y>15||ForestFogField.Hash(x/3,y/2)>.34)return Color.clear;
            if(ForestFogField.Hash(x+8,y+31)<.25)return Color.clear;
            return new Color32((byte)(72+x%3*7),(byte)(62+y%3*5),(byte)(37+x%2*6),255);
        });
        twig=Bake(Art+"RamasSecas.png",32,20,32,(x,y)=>
        {
            bool branch=(x>3&&x<28&&Mathf.Abs(y-(4+x/3))<=0)||(x>12&&x<22&&y==18-x/2)||(x>=7&&x<15&&y==x-3);
            return branch?new Color32(68,58,40,255):Color.clear;
        });
        Sprite bottle=Bake(Art+"BotellaRecogible.png",32,64,128,(x,y)=>BottlePixel(x,y));
        Directory.CreateDirectory("Assets/Resources/FrecuenciaCero");
        if(!File.Exists("Assets/Resources/FrecuenciaCero/InventorySettings.asset"))
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<SurvivalInventorySettings>(),"Assets/Resources/FrecuenciaCero/InventorySettings.asset");
        foreach (string name in new[]{"N1_Generador","N1_BosqueAbandonado"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/"+name+".unity");forest=name.Contains("Bosque");
            var root=new GameObject(RootName).transform;
            // Existing trees keep their physics and locations. New understory connects them into groves.
            var clusters=forest?ForestClusters:GeneratorClusters;
            for(int i=0;i<clusters.Length;i++)Grove(root,clusters[i],i);
            GroundEdgeDetails(root);
            if(forest)CampDetails(root);else MaintenanceDetails(root);
            Understory(root);
            foreach(var pickup in UnityEngine.Object.FindObjectsByType<SupplyPickup>().Where(p=>p.Kind==SupplyKind.Bottle))
            {
                pickup.GetComponent<SpriteRenderer>().sprite=bottle;
                pickup.transform.localScale=Vector3.one;
            }
            BakeGround();ConfigureFog();Physics2D.SyncTransforms();
            EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Directory.CreateDirectory("../PulidoVisualPortable");
        File.WriteAllText("../PulidoVisualPortable/Construccion.txt","PASS: two original scenes polished, original interaction/lighting/camera/AI preserved; pickup-only bottle, shared medkit capacity, bank fog and irregular terrain\n");
    }
    public static void RefineUnderstory()
    {
        sprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Nivel01/Exterior/ExteriorReten_ProvisionalAtlas.png").OfType<Sprite>().ToDictionary(s=>s.name);
        lit=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Placeholders/ProvisionalLit.mat");
        litter=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"Hojarasca.png");twig=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"RamasSecas.png");
        foreach(string name in new[]{"N1_Generador","N1_BosqueAbandonado"})
        {
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Nivel01/"+name+".unity");forest=name.Contains("Bosque");
            var root=GameObject.Find(RootName);if(root==null)throw new InvalidOperationException("Primero preparar el pulido.");
            if(root.transform.Find("Sotobosque junto a las zonas de trabajo")!=null)throw new InvalidOperationException("El sotobosque ya está preparado.");
            Understory(root.transform);BakeGround();EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
    }
    public static void RefineBottle()
    {
        if(!File.Exists(Art+"BotellaRecogible.png"))throw new InvalidOperationException("Primero preparar el pulido.");
        Bake(Art+"BotellaRecogible.png",32,64,128,(x,y)=>BottlePixel(x,y));AssetDatabase.SaveAssets();
    }
    static void Understory(Transform root)
    {
        var group=new GameObject("Sotobosque junto a las zonas de trabajo").transform;group.SetParent(root,false);
        foreach(Vector2 p in forest?new[]{new Vector2(-1.4f,8),new Vector2(-13,4),new Vector2(-9,-3)}:
            new[]{new Vector2(-2.3f,6.4f),new Vector2(-8.8f,-.3f)})Grove(group,p,50+Mathf.RoundToInt(p.x));
        var pockets=forest?new[]{new Vector2(-12.2f,7.2f),new Vector2(-4,9.7f),new Vector2(-4.1f,4.5f),new Vector2(-10.5f,6),new Vector2(-8.4f,2.1f),new Vector2(-4.7f,-5.4f),new Vector2(8.5f,-5.4f)}:
            new[]{new Vector2(-9.7f,7.7f),new Vector2(-4.5f,6.8f),new Vector2(2.3f,7.6f),new Vector2(8.7f,1.5f),new Vector2(-5.4f,-1.2f),new Vector2(4.6f,-7.8f)};
        foreach(Vector2 p in pockets)for(int j=0;j<5;j++)
        {
            Vector2 point=p+new Vector2(Mathf.Cos(j*2.4f),Mathf.Sin(j*2.4f))*(.3f+j*.16f);
            float distance=forest?AbandonedForestField.PathDistance(point):(float)GeneratorTerrainField.PathDistance(point.x,point.y);
            if(distance<1.1f||UnityEngine.Object.FindObjectsByType<WorldInteraction>().Any(t=>(t.InteractionPoint-point).sqrMagnitude<1.5f))continue;
            var r=Prop(group,"Brotes al pie de troncos y materiales",sprites[j%3==0?"Raices":"Arbusto"],point,.10f+j%3*.045f);
            r.color=new Color(.73f,.8f,.67f);Ground(r);
            var leaves=Prop(group,"Suelo con hojas agrupadas",litter,point+Vector2.down*.18f,1.6f);Ground(leaves);
        }
    }
    static bool OpenSpace(Vector2 p,float margin)
    {
        if(forest)
        {
            if(AbandonedForestField.PathDistance(p)<margin||(p-new Vector2(2,-4)).magnitude<7)return true;
            if(p.x>-13.8f&&p.x<-2.8f&&p.y>4.5f&&p.y<12.3f)return true;
            return false;
        }
        if(GeneratorTerrainField.PathDistance(p.x,p.y)<margin)return true;
        return UnityEngine.Object.FindObjectsByType<WorldInteraction>().Any(t=>(t.InteractionPoint-p).sqrMagnitude<margin*margin) ||
            p.x>-10&&p.x<-3&&p.y>2&&p.y<7 || p.x>2&&p.x<10&&p.y>2&&p.y<10;
    }
    static void Grove(Transform root,Vector2 center,int seed)
    {
        var group=new GameObject("Arboleda "+seed).transform;group.SetParent(root,false);
        for(int j=0;j<5;j++)
        {
            float angle=(seed*.83f+j*2.39996f),r=.6f+j*.33f;
            Vector2 p=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r;
            if(OpenSpace(p,3.1f))continue;
            if(!Physics2D.OverlapCircle(p+Vector2.up*.15f,.55f))
            {
                var tree=Prop(group,"Pinos agrupados "+j,sprites[(seed+j)%4==0?"Arbol_Frondoso":j%2==0?"Pino_Mediano":"Pino_Alto"],p,.57f+(seed+j)%5*.1f,true);
                tree.flipX=(seed+j)%2==0;tree.color=new Color(.68f+(j%3)*.06f,.78f,.79f);
            }
            Vector2 shrub=p+new Vector2(.8f,-.55f);
            if(!OpenSpace(shrub,2.25f))Prop(group,"Sotobosque",sprites["Arbusto"],shrub,.29f+j%3*.06f);
            var leaves=Prop(group,"Hojarasca del grupo",litter,p+Vector2.down*.3f,1.3f);Ground(leaves);
            if(j==0){var roots=Prop(group,"Raices entre troncos",sprites["Raices"],p+Vector2.left*.5f,.32f);Ground(roots);}
            if(j==2&&!OpenSpace(p+Vector2.down,2.7f))Prop(group,"Roca con musgo",sprites["Roca_Musgo"],p+Vector2.down,.18f,true);
        }
        Vector2 logPoint=center+new Vector2(-1.2f,-1.5f);
        if(seed%3==0&&!OpenSpace(logPoint,3.5f)&&!Physics2D.OverlapCircle(logPoint,.8f))
            Prop(group,"Tronco caido entre la maleza",sprites["Tronco_Caido"],logPoint,.32f,true);
    }
    static void GroundEdgeDetails(Transform root)
    {
        // Litter follows damp margins, with patches rather than uniform scattered props.
        for(int i=0;i<180;i++)
        {
            Vector2 p=new Vector2(Mathf.Lerp(-15,15,(float)ForestFogField.Hash(i,281)),Mathf.Lerp(forest?-14:-20,forest?14:10,(float)ForestFogField.Hash(i,293)));
            float distance=forest?AbandonedForestField.PathDistance(p):(float)GeneratorTerrainField.PathDistance(p.x,p.y);
            if(distance<1.1f||distance>4.4f||Mathf.PerlinNoise(p.x*.25f+10,p.y*.25f+10)<.46f)continue;
            var r=Prop(root,i%4==0?"Ramas en el margen":"Hojas en tierra humeda",i%4==0?twig:litter,p,.8f+i%3*.2f);Ground(r);
            r.flipX=i%2==0;r.color=new Color(.76f,.78f,.64f);
            if(i%7==0&&!OpenSpace(p,2.2f))
            {var bush=Prop(root,"Vegetacion baja del margen",sprites["Arbusto"],p,.18f);bush.color=new Color(.7f,.77f,.62f);}
        }
    }
    static void CampDetails(Transform root)
    {
        foreach(Vector2 p in new[]{new Vector2(-12.6f,10.6f),new Vector2(-11.7f,11.8f),new Vector2(-6.7f,11.9f),new Vector2(-3.8f,9.6f),new Vector2(-12,5.1f)})
            Prop(root,"Maleza junto al refugio y materiales",sprites["Arbusto"],p,.32f);
        foreach(Vector2 p in new[]{new Vector2(-12,7.1f),new Vector2(-10.8f,5.4f),new Vector2(-4.4f,7.5f),new Vector2(-6.1f,10.8f)})
        {var r=Prop(root,"Restos de trabajo forestal",twig,p,1.4f);Ground(r);}
        foreach(Vector2 p in new[]{new Vector2(-3.2f,-7),new Vector2(7.8f,-3.8f),new Vector2(3.5f,-10.2f)})
        {var r=Prop(root,"Raices expuestas del borde del claro",sprites["Raices"],p,.55f);r.color=new Color(.77f,.73f,.6f);Ground(r);}
        foreach(Vector2 p in new[]{new Vector2(2.6f,-3.5f),new Vector2(3.8f,-3.3f),new Vector2(2.7f,-4.2f)})
        {var r=Prop(root,"Tierra excavada y hojas",litter,p,1.8f);r.color=new Color(.82f,.65f,.44f);Ground(r);}
    }
    static void MaintenanceDetails(Transform root)
    {
        foreach(Vector2 p in new[]{new Vector2(-10.6f,9.3f),new Vector2(-6.4f,9.4f),new Vector2(-1.6f,9.6f),new Vector2(11.1f,5.8f),new Vector2(12.7f,3.4f),new Vector2(10.7f,7.8f)})
            Prop(root,"Vegetacion al pie de las cercas",sprites["Arbusto"],p,.31f);
        foreach(Vector2 p in new[]{new Vector2(8.1f,6.4f),new Vector2(-9.4f,6.8f)})
        {var r=Prop(root,"Ramas tras las cajas de mantenimiento",twig,p,1.6f);Ground(r);}
    }
    static SpriteRenderer Prop(Transform parent,string name,Sprite sprite,Vector2 point,float scale,bool solid=false)
    {
        var obj=new GameObject(name,typeof(SpriteRenderer),typeof(YSort));obj.transform.SetParent(parent,false);obj.transform.position=point;obj.transform.localScale=Vector3.one*scale;
        var r=obj.GetComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=lit;
        if(solid){var c=obj.AddComponent<BoxCollider2D>();c.offset=Vector2.up*.14f/scale;c.size=new Vector2(Mathf.Min(.9f,r.bounds.size.x*.64f)/scale,.28f/scale);}
        return r;
    }
    static void Ground(SpriteRenderer r){UnityEngine.Object.DestroyImmediate(r.GetComponent<YSort>());r.sortingOrder=-1785;}
    static Color BottlePixel(int x,int y)
    {
        // Four texels per painted pixel at 128 PPU = the scene's 32 PPU, with no transform scaling.
        x/=4;y/=4;
        bool neck=y>=11&&y<=15&&x>=3&&x<=4;
        bool shoulder=y==9&&x>=2&&x<=5||y==10&&x>=3&&x<=4;
        bool body=y>=1&&y<=8&&x>=1&&x<=6&&!(y==1&&(x==1||x==6));
        if(!neck&&!shoulder&&!body)return Color.clear;
        if(y>=14)return new Color32(109,99,67,255);
        if(x==1||x==6||y==1)return new Color32(30,45,34,255);
        if(y>=4&&y<=7&&x>=2&&x<=5&&!(y==7&&x==5))return new Color32((byte)(119-x*4),108,72,255);
        if(x==2||neck&&x==3)return new Color32(112,125,85,255);
        if(y==2)return new Color32(74,86,53,255);
        return new Color32((byte)(76-x*4),78,47,255);
    }
    static Sprite Bake(string path,int w,int h,int ppu,Func<int,int,Color> pixel,bool point=true)
    {
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);var colors=new Color[w*h];
        for(int y=0;y<h;y++)for(int x=0;x<w;x++)colors[y*w+x]=pixel(x,y);
        tex.SetPixels(colors);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());UnityEngine.Object.DestroyImmediate(tex);
        AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
        var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
        importer.spritePixelsPerUnit=ppu;importer.filterMode=point?FilterMode.Point:FilterMode.Bilinear;importer.textureCompression=TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled=false;importer.maxTextureSize=4096;importer.alphaIsTransparency=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }
    static void BakeGround()
    {
        string path=forest?"Assets/Art/Nivel01/Survival/SueloBosque.png":"Assets/Art/Nivel01/Generator/SueloGenerador.png";
        int w=forest?2048:2304,h=forest?1536:1792;float halfX=forest?32:36,halfY=forest?24:28;
        Bake(path,w,h,32,(x,y)=>
        {
            Vector2 p=new Vector2((x+.5f)/32-halfX,(y+.5f)/32-halfY);
            float grain=(float)ForestFogField.Hash(x,y),broad=Mathf.PerlinNoise(p.x*.15f+50,p.y*.15f+50);
            float pathAmount=Mathf.SmoothStep(0,1,Mathf.Clamp01((1.65f+(Mathf.PerlinNoise(p.x*2+50,p.y*2+50)-.5f)*.65f-AbandonedForestField.PathDistance(p))/.7f));
            pathAmount=Mathf.Max(pathAmount,Mathf.Clamp01(1-Vector2.Distance(p,new Vector2(2,-4))/5)*.8f);
            Color basis;
            if(forest)basis=new Color32((byte)(32+pathAmount*29+broad*7+grain*4),(byte)(42+pathAmount*11+broad*8+grain*4),(byte)(35+pathAmount*4+broad*6+grain*4),255);
            else {uint packed=GeneratorTerrainField.Pixel(x,y);basis=new Color32((byte)packed,(byte)(packed>>8),(byte)(packed>>16),255);}
            float coarse=Mathf.PerlinNoise(p.x*.65f+93,p.y*.65f+28), fine=(float)ForestFogField.Hash(x/2,y/2);
            float wear=Mathf.Lerp(-.024f,.032f,Mathf.Floor(coarse*5)/5);
            basis.r+=wear;basis.g+=wear*.75f;basis.b+=wear*.5f;
            // Moss pockets and tiny snapped leaf groups, bounded contrast so routes stay legible.
            if(coarse>.6f && grain<.09f && fine>.55f)basis=Color.Lerp(basis,new Color(.23f,.23f,.12f),.45f);
            if(fine<.10f&&grain<.65f)basis*=.78f;
            basis.a=1;return basis;
        });
    }
    static void ConfigureFog()
    {
        var fog=UnityEngine.Object.FindAnyObjectByType<ForestFogController>();var s=new SerializedObject(fog);
        s.FindProperty("useAbandonedForestDensity").boolValue=forest;s.FindProperty("useExteriorDensity").boolValue=false;
        s.FindProperty("generalDensity").floatValue=.9f;s.FindProperty("transparency").floatValue=.80f;
        s.FindProperty("movementSpeed").floatValue=.025f;s.FindProperty("ambientStrength").floatValue=.6f;s.FindProperty("bankStrength").floatValue=.9f;
        s.FindProperty("bankCount").intValue=16;s.FindProperty("bankArea").rectValue=forest?new Rect(-16,-15,32,30):new Rect(-16,-20,32,30);
        s.FindProperty("fogColor").colorValue=new Color(.57f,.68f,.76f,1);s.FindProperty("bankTint").colorValue=new Color(.57f,.69f,.76f,1);
        var ambient=(SpriteRenderer)s.FindProperty("ambient").objectReferenceValue;
        var banks=s.FindProperty("bankPool");
        // Low banks sit behind character-scale props; thin upper banks add distant depth.
        ambient.sortingOrder=-1760;
        for(int i=0;i<banks.arraySize;i++)
        {var r=(SpriteRenderer)banks.GetArrayElementAtIndex(i).objectReferenceValue;if(r!=null)r.sortingOrder=i%3==0?700:-900;}
        s.ApplyModifiedPropertiesWithoutUndo();
        string texturePath=Art+(forest?"BrumaBosque":"BrumaGenerador")+".png";
        var sprite=Bake(texturePath,1024,768,16,(x,y)=>
        {
            Vector2 p=new Vector2((x+.5f)/16-32,(y+.5f)/16-24);
            float zone=forest?AbandonedForestField.FogDensity(p):Mathf.Lerp(.3f,1,Mathf.SmoothStep(0,1,((float)GeneratorTerrainField.PathDistance(p.x,p.y)-.5f)/2));
            float cloud=.1f+.5f*Mathf.PerlinNoise(p.x*.22f+7,p.y*.25f+19);
            float edge=Mathf.Clamp01(Mathf.Min((32-Mathf.Abs(p.x))/5,(24-Mathf.Abs(p.y))/5));
            return new Color(1,1,1,cloud*zone*edge);
        },false);
        ambient.sprite=sprite;ambient.transform.localScale=Vector3.one;
        fog.enabled=false;fog.enabled=true;
    }
}
