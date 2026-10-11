using UnityEngine;
using UnityEngine.Tilemaps;

namespace FrecuenciaCero
{
    [CreateAssetMenu(menuName = "Frecuencia Cero/Tile exterior")]
    public sealed class ExteriorTile : TileBase
    {
        [SerializeField] private Sprite sprite = null;
        [SerializeField, Min(0.1f)] private float spriteScale = 1f;
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData data)
        {
            data.sprite = sprite;
            data.color = Color.white;
            data.transform = Matrix4x4.Scale(Vector3.one * spriteScale);
            data.flags = TileFlags.LockTransform | TileFlags.LockColor;
            data.colliderType = Tile.ColliderType.None;
        }
    }
}
