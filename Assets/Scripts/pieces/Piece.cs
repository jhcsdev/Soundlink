using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        private static float inventoryScale = 0.6f;
        private static float tileScale = 1;
        private static float gridTileScale = 1f;

        [SerializeField] private PieceData data;

        private readonly List<PieceTile> tileObjects = new();
        private bool initialized = false;

        // this is a canvas object that is exclusively used for visuals / the inventory. it is not a child object. 
        private GameObject canvasPiece;
        public GameObject GetCanvasPiece() => canvasPiece;

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
            canvasPiece = new GameObject("CanvasPiece", typeof(RectTransform));

            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = TileManager.Instance.CreatePieceAndCanvasTile(ptd, out GameObject canvasVisual)
                    .Initialize(ptd, this)
                    .SetParent(transform)
                    .SetLocalPositionAndScaleByTileSize(tileScale);
                tileObjects.Add(t);
                
                canvasVisual.transform.SetParent(canvasPiece.transform, false);

                RectTransform rt = canvasVisual.GetComponent<RectTransform>();
                float tileSize = rt.sizeDelta.x; 
                rt.anchoredPosition = new Vector2(ptd.relativeOffset.x * tileSize, ptd.relativeOffset.y * tileSize);
            }
        }

        public Piece InventoryMode()
        {
            transform.localScale = Vector2.one * inventoryScale;

            return this;
        }
        public Piece LimboMode()
        {
            transform.localScale = Vector2.one * gridTileScale; 
            // todo (temporary): separate into own visual class; for now just makes tiles slightly transparent
            foreach (PieceTile pt in tileObjects)
            {
                SpriteRenderer temp = pt.gameObject.GetComponent<SpriteRenderer>();
                temp.color = new(temp.color.r, temp.color.g, temp.color.b, 0.5f);
            }

            return this;
        }
        public Piece GridMode()
        {
            transform.localScale = Vector2.one * gridTileScale;
            // todo (temporary): separate into own visual class; for now just makes tiles fully visible
            foreach (PieceTile pt in tileObjects)
            {
                SpriteRenderer temp = pt.gameObject.GetComponent<SpriteRenderer>();
                temp.color = new(temp.color.r, temp.color.g, temp.color.b, 1f);
            }

            return this;
        }
        public List<PieceTile> GetPieceTiles()
        {
            return tileObjects;
        }

        public void RotatePieceClockwise()
        {
            foreach (PieceTile pt in tileObjects)
            {
                pt.RotateRelativeOffsetClockwise();
            }
        }
        public void RotatePieceCounterClockwise()
        {
            foreach (PieceTile pt in tileObjects)
            {
                pt.RotateRelativeOffsetCounterClockwise();
            }
        }
    }
}
