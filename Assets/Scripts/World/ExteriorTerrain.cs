using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace FrecuenciaCero
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class ExteriorTerrain : MonoBehaviour
    {
        [SerializeField] private TextAsset layout = null;
        [SerializeField] private Material litMaterial = null;
        [SerializeField] private TileBase[] tiles = null;
        [SerializeField, HideInInspector] private int appliedRevision = 0;
        private const int CurrentRevision = 1;
        public bool IsPrepared => appliedRevision == CurrentRevision && GetComponentInChildren<Tilemap>(true) != null;

        [Serializable] private sealed class Cell { public int x = 0, y = 0, tile = 0, layer = 0; }
        [Serializable] private sealed class Layout { public Cell[] cells = null; }

        private void OnEnable() => BuildIfNeeded();

        public void BuildIfNeeded()
        {
            if (IsPrepared) return;
            if (layout == null || litMaterial == null || tiles == null || tiles.Length == 0) return;
            foreach (TileBase tile in tiles) if (tile == null) return;
            // Wait until Unity has imported all atlas slices before replacing existing ground.
            if (tiles.Length < 14 || !(tiles[13] is ContinuousGroundTile ground) || !ground.IsReady) return;
            Layout data = JsonUtility.FromJson<Layout>(layout.text);
            if (data == null || data.cells == null) return;

            Transform gridTransform = transform.Find("Grid_Exterior");
            if (gridTransform == null)
            {
                var grid = new GameObject("Grid_Exterior", typeof(Grid));
                grid.transform.SetParent(transform, false);
                gridTransform = grid.transform;
            }
            var maps = new Tilemap[4];
            string[] names = { "Suelo_y_sendero", "Arroyo_estatico", "Detalles_suelo", "Vegetacion_baja" };
            for (int i = 0; i < maps.Length; i++)
            {
                Transform existing = gridTransform.Find(names[i]);
                GameObject item = existing != null ? existing.gameObject : new GameObject(names[i], typeof(Tilemap), typeof(TilemapRenderer));
                if (existing == null) item.transform.SetParent(gridTransform, false);
                maps[i] = item.GetComponent<Tilemap>();
                // Migrate only generated ground, water and ground details. Preserve painted vegetation.
                if (i < 3) maps[i].ClearAllTiles();
                maps[i].tileAnchor = i == 0 ? Vector3.zero : new Vector3(.5f, .5f, 0);
                var renderer = item.GetComponent<TilemapRenderer>();
                renderer.sharedMaterial = litMaterial;
                renderer.sortingOrder = -2000 + i * 50;
            }
            foreach (Cell cell in data.cells)
                if (cell.tile >= 0 && cell.tile < tiles.Length && cell.layer >= 0 && cell.layer < maps.Length &&
                    (cell.layer != 3 || !maps[3].HasTile(new Vector3Int(cell.x, cell.y, 0))))
                    maps[cell.layer].SetTile(new Vector3Int(cell.x, cell.y, 0), tiles[cell.tile]);
            foreach (Tilemap map in maps) map.CompressBounds();
            appliedRevision = CurrentRevision;
        }

        public void RebuildGeneratedSurface()
        {
            appliedRevision = 0;
            BuildIfNeeded();
        }
    }
}
