using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        private static float inventoryScale = 0.5f;
        private static float tileScale = 1;
        private static float gridTileScale = 1f;

        [SerializeField] private PieceData data;

        private List<PieceTile> tileObjects = new();
        private bool initialized = false;

        void Awake()
        {
            if (data == null) return;

            initialized = true;
            CreateTiles();
        }

        public void Initialize(PieceData pd)
        {
            if (initialized == true) { Debug.Log("Duplicate intiialization on piece: " + name); return; }

            initialized = true;
            data = pd;
            CreateTiles();
        }

        private void CreateTiles()
        {
            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = TileManager.Instance.CreateTile(ptd)
                    .Initialize(ptd, this)
                    .SetParent(transform)
                    .SetLocalPositionAndScaleByTileSize(tileScale);
                tileObjects.Add(t);
            }
        }

        public void InventoryMode()
        {
            transform.localScale = Vector2.one * inventoryScale;
        }
        public void GridMode()
        {
            transform.localScale = Vector2.one * gridTileScale;
        }
        public List<PieceTile> GetPieceTiles()
        {
            return tileObjects;
        }

        public void RotatePieceClockwise()
        {
            foreach (PieceTile pt in tileObjects)
            {
                // change the local position of the object based on where it is currently; need to just change local x/y
            }
        }
        public void RotatePieceCounterClockwise()
        {
            
        }
    }
}
