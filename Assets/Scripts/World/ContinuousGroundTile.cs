using UnityEngine;
using UnityEngine.Tilemaps;

namespace FrecuenciaCero
{
    public sealed class ContinuousGroundTile : TileBase
    {
        [SerializeField] private Sprite[] chunks = null;
        public bool IsReady => chunks != null && chunks.Length == 77 && System.Array.TrueForAll(chunks, s => s != null);

        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData data)
        {
            int x = position.x - OrganicTerrainField.MinX, y = position.y - OrganicTerrainField.MinY;
            if (x < 0 || y < 0 || x % 4 != 0 || y % 4 != 0) return;
            int col = x / 4, row = y / 4;
            if (col >= 11 || row >= 7 || !IsReady) return;
            data.sprite = chunks[row * 11 + col];
            data.color = Color.white;
            data.transform = Matrix4x4.identity;
            data.flags = TileFlags.LockColor | TileFlags.LockTransform;
            data.colliderType = Tile.ColliderType.None;
        }
    }
}
