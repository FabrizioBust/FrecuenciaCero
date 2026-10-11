using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero.Editor
{
    public static class BosquePrototypeBuilder
    {
        private const string ScenePath = "Assets/Scenes/Nivel01_Bosque.unity";
        private const string ArtPath = "Assets/Art/Placeholders";
        private const string RootName = "Prototipo_ElSenderoDelReten";

        [MenuItem("Frecuencia Cero/Preparar Nivel01 Bosque")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Sal del modo Play antes de preparar la escena.");

            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            foreach (GameObject existing in scene.GetRootGameObjects())
                if (existing.name == RootName)
                {
                    if (openedHere) EditorSceneManager.CloseScene(scene, true);
                    return;
                }
            if (scene.isDirty)
            {
                Debug.LogWarning("Guarda Nivel01_Bosque y vuelve a ejecutar Frecuencia Cero > Preparar Nivel01 Bosque.");
                return;
            }

            Scene previous = SceneManager.GetActiveScene();
            SceneManager.SetActiveScene(scene);
            GameObject root = new GameObject(RootName);
            try
            {
                Directory.CreateDirectory(ArtPath);
                Sprite ground = MakeSprite("Suelo", Ground(), new Vector2(0.5f, 0.5f));
                Sprite path = MakeSprite("Sendero", PathPixels(), new Vector2(0.5f, 0.5f));
                Sprite leoSprite = MakeSprite("Leo", Leo(), new Vector2(0.5f, 0.125f));
                Sprite tree = MakeSprite("Arbol", Tree(), new Vector2(0.5f, 0.125f));
                Sprite rock = MakeSprite("Roca", Rock(), new Vector2(0.5f, 0.25f));
                Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (shader == null) throw new InvalidOperationException("No se encontró el shader Sprite Unlit de URP 2D.");
                string materialPath = ArtPath + "/ProvisionalUnlit.mat";
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, materialPath);
                }

                Transform floor = Group("Suelo", root.transform);
                for (int y = -12; y <= 12; y++)
                    for (int x = -16; x <= 16; x++)
                    {
                        bool onPath = Mathf.Abs(x - Mathf.RoundToInt(2f * Mathf.Sin(y * 0.22f))) <= 1;
                        SpriteObject(onPath ? "Sendero" : "Suelo", onPath ? path : ground,
                            new Vector2(x, y), floor, material, -2000);
                    }

                Transform obstacles = Group("Obstaculos", root.transform);
                for (int y = -10; y <= 10; y += 3)
                    for (int x = -14; x <= 14; x += 3)
                    {
                        float px = x + ((y + 10) % 2 == 0 ? 0.25f : -0.25f);
                        if (Mathf.Abs(px - 2f * Mathf.Sin(y * 0.22f)) < 3.5f) continue;
                        GameObject item = SpriteObject("Arbol", tree, new Vector2(px, y), obstacles, material);
                        var collider = item.AddComponent<BoxCollider2D>();
                        collider.size = new Vector2(0.5f, 0.4f);
                        collider.offset = new Vector2(0f, 0.12f);
                        item.AddComponent<YSort>();
                    }
                foreach (Vector2 position in new[] { new Vector2(1.4f, -4f), new Vector2(-1.8f, 2.5f),
                    new Vector2(3f, 6f), new Vector2(-4f, -7f) })
                {
                    GameObject item = SpriteObject("Roca", rock, position, obstacles, material);
                    var collider = item.AddComponent<BoxCollider2D>();
                    collider.size = new Vector2(0.8f, 0.5f);
                    collider.offset = new Vector2(0f, 0.12f);
                    item.AddComponent<YSort>();
                }
                Transform limits = Group("Limites", root.transform);
                Boundary("Norte", new Vector2(0f, 12.5f), new Vector2(34f, 1f), limits);
                Boundary("Sur", new Vector2(0f, -12.5f), new Vector2(34f, 1f), limits);
                Boundary("Oeste", new Vector2(-16.5f, 0f), new Vector2(1f, 26f), limits);
                Boundary("Este", new Vector2(16.5f, 0f), new Vector2(1f, 26f), limits);

                GameObject leo = SpriteObject("Leo", leoSprite, new Vector2(0f, -8f), root.transform, material);
                var body = leo.AddComponent<Rigidbody2D>();
                body.gravityScale = 0f;
                body.constraints = RigidbodyConstraints2D.FreezeRotation;
                body.interpolation = RigidbodyInterpolation2D.Interpolate;
                body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                var feet = leo.AddComponent<CapsuleCollider2D>();
                feet.direction = CapsuleDirection2D.Horizontal;
                feet.size = new Vector2(0.42f, 0.25f);
                feet.offset = new Vector2(0f, 0.1f);
                string physicsPath = ArtPath + "/SinFriccion.physicsMaterial2D";
                PhysicsMaterial2D physics = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(physicsPath);
                if (physics == null)
                {
                    physics = new PhysicsMaterial2D("SinFriccion") { friction = 0f, bounciness = 0f };
                    AssetDatabase.CreateAsset(physics, physicsPath);
                }
                feet.sharedMaterial = physics;
                var actions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Settings/InputSystem_Actions.inputactions");
                if (actions == null || actions.FindAction("Player/Move") == null)
                    throw new InvalidOperationException("Falta la acción existente Player/Move.");
                leo.AddComponent<LeoMovement>().Configure(actions);
                leo.AddComponent<YSort>();

                Camera camera = null;
                foreach (GameObject existing in scene.GetRootGameObjects())
                    if (existing.TryGetComponent(out Camera found)) { camera = found; break; }
                if (camera == null) camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
                camera.tag = "MainCamera";
                camera.orthographic = true;
                camera.orthographicSize = 5.625f;
                camera.transform.SetPositionAndRotation(new Vector3(0f, -8f, -10f), Quaternion.identity);
                camera.backgroundColor = new Color32(15, 21, 27, 255);
                camera.allowMSAA = false;
                camera.allowHDR = false;
                camera.gameObject.AddComponent<SmoothCameraFollow>().Configure(leo.transform);
                var pixel = camera.GetComponent<PixelPerfectCamera>();
                if (pixel == null) pixel = camera.gameObject.AddComponent<PixelPerfectCamera>();
                pixel.assetsPPU = 32;
                pixel.refResolutionX = 640;
                pixel.refResolutionY = 360;
                pixel.gridSnapping = PixelPerfectCamera.GridSnapping.UpscaleRenderTexture;
                var pixelSettings = new SerializedObject(pixel);
                pixelSettings.FindProperty("m_FilterMode").enumValueIndex = (int)PixelPerfectCamera.PixelPerfectFilterMode.Point;
                pixelSettings.ApplyModifiedPropertiesWithoutUndo();

                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Debug.Log("Frecuencia Cero: Nivel01_Bosque preparado. Abre la escena y pulsa Play.");
            }
            catch
            {
                // Roll back only objects created by this run; never delete existing scene objects.
                UnityEngine.Object.DestroyImmediate(root);
                throw;
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform Group(string name, Transform parent)
        {
            var group = new GameObject(name).transform;
            group.SetParent(parent, false);
            return group;
        }

        private static GameObject SpriteObject(string name, Sprite sprite, Vector2 position,
            Transform parent, Material material, int order = 0)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            var renderer = item.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sharedMaterial = material;
            renderer.sortingOrder = order;
            renderer.spriteSortPoint = SpriteSortPoint.Pivot;
            return item;
        }

        private static void Boundary(string name, Vector2 position, Vector2 size, Transform parent)
        {
            var item = new GameObject(name);
            item.transform.SetParent(parent, false);
            item.transform.localPosition = position;
            item.AddComponent<BoxCollider2D>().size = size;
        }

        private static Sprite MakeSprite(string name, string[] rows, Vector2 pivot)
        {
            string assetPath = ArtPath + "/" + name + ".png";
            if (!File.Exists(assetPath))
            {
                int width = rows[0].Length, height = rows.Length;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                        texture.SetPixel(x, height - 1 - y, Palette(rows[y][x]));
                File.WriteAllBytes(assetPath, texture.EncodeToPNG());
                UnityEngine.Object.DestroyImmediate(texture);
                AssetDatabase.ImportAsset(assetPath);
            }
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 32;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.npotScale = TextureImporterNPOTScale.None;
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteAlignment = (int)SpriteAlignment.Custom;
            settings.spritePivot = pivot;
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
        }

        private static Color32 Palette(char c)
        {
            switch (c)
            {
                case 'g': return new Color32(29, 43, 40, 255);
                case 'G': return new Color32(35, 51, 45, 255);
                case 'p': return new Color32(66, 62, 54, 255);
                case 'P': return new Color32(76, 72, 62, 255);
                case 't': return new Color32(24, 35, 33, 255);
                case 'T': return new Color32(43, 62, 51, 255);
                case 'b': return new Color32(69, 51, 40, 255);
                case 'r': return new Color32(75, 82, 83, 255);
                case 'R': return new Color32(101, 108, 106, 255);
                case 'h': return new Color32(58, 41, 34, 255);
                case 's': return new Color32(186, 150, 117, 255);
                case 'j': return new Color32(153, 123, 65, 255);
                case 'J': return new Color32(184, 151, 83, 255);
                case 'd': return new Color32(42, 47, 61, 255);
                default: return new Color32(0, 0, 0, 0);
            }
        }

        private static string[] Tile(char basis, char accent)
        {
            var rows = new string[32];
            for (int y = 0; y < 32; y++)
            {
                var row = new char[32];
                for (int x = 0; x < 32; x++) row[x] = (x * 17 + y * 31) % 37 < 3 ? accent : basis;
                rows[y] = new string(row);
            }
            return rows;
        }

        private static string[] Ground() => Tile('g', 'G');
        private static string[] PathPixels() => Tile('p', 'P');
        private static string[] Leo() => new[]
        {
            "................", "......hhhh......", ".....hhhhhh.....", ".....hhhhhh.....",
            ".....hssssh.....", "......ssss......", ".....jjjjjj.....", "....jjJJJJjj....",
            "...ssjJJJJjss...", "...ssjJJJJjss...", ".....jjjjjj.....", ".....dddddd.....",
            ".....dd..dd.....", ".....dd..dd.....", ".....hh..hh.....", "................"
        };
        private static string[] Tree()
        {
            var rows = new string[64];
            for (int y = 0; y < 64; y++)
            {
                var row = new char[48];
                for (int x = 0; x < 48; x++)
                {
                    int halfWidth = y < 45 ? 3 + y / 2 : 0;
                    row[x] = y < 45 && Mathf.Abs(x - 24) < halfWidth ?
                        ((x + y * 3) % 11 < 3 ? 'T' : 't') :
                        (y >= 38 && y < 59 && x >= 21 && x <= 26 ? 'b' : '.');
                }
                rows[y] = new string(row);
            }
            return rows;
        }
        private static string[] Rock()
        {
            var rows = new string[24];
            for (int y = 0; y < 24; y++)
            {
                var row = new char[32];
                for (int x = 0; x < 32; x++)
                    row[x] = Mathf.Pow((x - 16) / 14f, 2) + Mathf.Pow((y - 11) / 9f, 2) < 1f ?
                        (y < 10 ? 'R' : 'r') : '.';
                rows[y] = new string(row);
            }
            return rows;
        }
    }
}
