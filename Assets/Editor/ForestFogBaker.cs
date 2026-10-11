using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrecuenciaCero.Editor
{
    public static class ForestFogBaker
    {
        public const string AmbientPath = "Assets/Art/Nivel01/Fog/NieblaAmbiental.png";
        public const string BankPath = "Assets/Art/Nivel01/Fog/BancoNiebla.png";
        [MenuItem("Frecuencia Cero/Niebla exterior/Regenerar texturas")]
        public static void Bake()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("Deten Play antes de regenerar las texturas de niebla.");
                return;
            }
            Save(AmbientPath, ForestFogField.AmbientWidth, ForestFogField.AmbientHeight, true);
            Save(BankPath, ForestFogField.BankWidth, ForestFogField.BankHeight, false);
            Debug.Log("Texturas de niebla regeneradas; escenas y fisicas conservadas.");
        }
        private static void Save(string path, int width, int height, bool ambient)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[width * height];
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        double alpha = ambient ? ForestFogField.AmbientAlpha(x, y) : ForestFogField.BankAlpha(x, y);
                        pixels[y * width + x] = new Color32(255, 255, 255, (byte)System.Math.Round(alpha * 255));
                    }
                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(path, texture.EncodeToPNG());
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);
            }
            finally { Object.DestroyImmediate(texture); }
        }
    }
}
