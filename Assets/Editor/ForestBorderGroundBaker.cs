using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrecuenciaCero.Editor
{
    public static class ForestBorderGroundBaker
    {
        public const string AtlasPath = "Assets/Art/Nivel01/Exterior/SueloBosqueExtendido.png";

        [MenuItem("Frecuencia Cero/Exterior Reten/Regenerar suelo de bordes")]
        public static void Bake()
        {
            var texture = new Texture2D(ForestBorderField.Width, ForestBorderField.Height, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[texture.width * texture.height];
                for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                    {
                        uint c = ForestBorderField.Pixel(x, y);
                        pixels[y * texture.width + x] = new Color32((byte)(c >> 16), (byte)(c >> 8), (byte)c, 255);
                    }
                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(AtlasPath, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(AtlasPath, ImportAssetOptions.ForceUpdate);
            }
            finally { Object.DestroyImmediate(texture); }
        }
    }
}
