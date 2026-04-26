using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        private static float tileScale = 1;
        private static float gridTileScale = 1f;

        private PieceState currentState;

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
                    .SetLocalPositionAndScaleByTileSize(tileScale)
                    .CreateGlueSprites();
                tileObjects.Add(t);
                
                canvasVisual.transform.SetParent(canvasPiece.transform, false);

                RectTransform rt = canvasVisual.GetComponent<RectTransform>();
                float tileSize = rt.sizeDelta.x; 
                rt.anchoredPosition = new Vector2(ptd.relativeOffset.x * tileSize, ptd.relativeOffset.y * tileSize);
            }
        }

        public Piece InventoryMode()
        {
            currentState = PieceState.INVENTORY;
            return this;
        }
        public Piece LimboMode()
        {
            if (currentState == PieceState.HOVER_GRID) { 
                foreach(PieceTile pt in tileObjects) pt.Hovered();
                return this;
            } 
            currentState = PieceState.HOVER_GRID;

            transform.localScale = Vector2.one * gridTileScale; 

            foreach(PieceTile pt in tileObjects) { pt.PickedUp(); pt.Hovered(); }// these are visuals

            return this;
        }
        public Piece GridMode()
        {
            if (currentState == PieceState.PLACED_GRID) return this;
            currentState = PieceState.PLACED_GRID;
                        
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

        public void FailedPlace()
        {
            // todo:: visuals
            foreach(PieceTile pt in tileObjects) pt.ColorPulse(Color.red, 0.5f);
        }
        public void LinkPulse(Color color)
        {
            foreach(PieceTile pt in tileObjects) pt.ColorPulse(color);
        }
        public void SetColorPermanent(Color color, float overTime=0.2f)
        {
            foreach(PieceTile pt in tileObjects) pt.SetColor(color, overTime);
        }

        public bool IsSilentPiece() => data.isSilentPiece;
    }

    public enum PieceState
    {
        INVENTORY,
        PLACED_GRID, 
        HOVER_GRID
    }
}
