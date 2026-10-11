using System.IO;
using UnityEditor;
using UnityEngine;

namespace FrecuenciaCero.Editor
{
    public static class OrganicTerrainBaker
    {
        public const string AtlasPath = "Assets/Art/Nivel01/Terrain/SueloOrganico.png";
        public static void Bake()
        {
            var texture = new Texture2D(OrganicTerrainField.Width, OrganicTerrainField.Height, TextureFormat.RGBA32, false);
            try
            {
                var pixels = new Color32[texture.width * texture.height];
                for (int y=0;y<texture.height;y++)
                    for(int x=0;x<texture.width;x++)
                    {
                        uint c=OrganicTerrainField.Pixel(x,y);
                        pixels[y*texture.width+x]=new Color32((byte)(c>>16),(byte)(c>>8),(byte)c,255);
                    }
                texture.SetPixels32(pixels);
                texture.Apply();
                File.WriteAllBytes(AtlasPath,texture.EncodeToPNG());
                AssetDatabase.ImportAsset(AtlasPath,ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
            }
            finally { Object.DestroyImmediate(texture); }
        }
    }
}
