using System.Collections.Generic;
using PuzzleGrid;
using UnityEngine;
using UnityEngine.Events;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private PieceData data;
        private PieceState currentState;
        private static float tileScale = 1;
        private static float gridTileScale = 1f;

        #region
        public UnityAction OnPiecePlacedOnGrid;
        public UnityAction OnPieceFailedToPlaceOnGrid;
        public UnityAction OnPieceStartedHover;
        public UnityAction OnPiecePickedUp;
        public UnityAction<Transform /*new focus*/> OnHoverPieceMoved;
        public UnityAction OnPieceReturnedToInventory;
        public UnityAction<Color, PieceTile, bool> OnLinkPulse;
        public UnityAction<LinkPlacementData> OnJoinedLink;
        public UnityAction OnLeftLink;
        public UnityAction OnLinkBroken; // for when two conflicting links are "merged" with one another? todo:: unsure if will use
        #endregion

        private readonly List<PieceTile> tileObjects = new();
        private bool initialized = false;

        // this is a canvas object that is exclusively used for the inventory, and is not really within the "bounds" of being controlled by the Piece.
        // it is not a child object. 
        private GameObject canvasPiece;
        public GameObject GetCanvasPiece() => canvasPiece;

        #region initialization
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

            gameObject.AddComponent<PieceVisuals>();
        }
        private void CreateTiles()
        {
            canvasPiece = new GameObject("CanvasPiece", typeof(RectTransform));

            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = TileManager.Instance.CreatePieceAndCanvasTile(ptd, out GameObject canvasVisual, isSilent: data.isSilentPiece)
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
        #endregion 

        #region mode context
        public Piece InventoryMode()
        {
            if (currentState == PieceState.INVENTORY) return this;
            currentState = PieceState.INVENTORY;
            OnPieceReturnedToInventory?.Invoke();
            return this;
        }
        public Piece LimboMode(GridTile focusedTile)
        {
            // already in hover mode, which suggests we had a focused tile before this one; tile has now (potentially?) been changed
            if (currentState == PieceState.HOVER_GRID) 
            {
                OnHoverPieceMoved?.Invoke(focusedTile.transform);
                return this;
            }
            if (currentState == PieceState.PLACED_GRID) OnPiecePickedUp?.Invoke();
            else OnPieceStartedHover?.Invoke();

            currentState = PieceState.HOVER_GRID;

            transform.localScale = Vector2.one * gridTileScale; 
            transform.position = focusedTile.transform.position;

            return this;
        }
        public Piece PlacedGrid()
        {
            if (currentState == PieceState.PLACED_GRID) return this;
            currentState = PieceState.PLACED_GRID;

            OnPiecePlacedOnGrid?.Invoke();
                        
            return this;
        }
        public void FailedGridPlace()
        {
            OnPieceFailedToPlaceOnGrid?.Invoke();
        }
        #endregion
        
        #region other
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
        public bool IsSilentPiece() => data.isSilentPiece;
        #endregion

        #region link stuff
        public void JoinLink(LinkPlacementData link)
        {
            Debug.Log("Joined link!");
            OnJoinedLink?.Invoke(link);
        }
        public void LeaveLink()
        {
            OnLeftLink?.Invoke();
        }
        public void BreakLink()
        {
            OnLinkBroken?.Invoke();
        }
        public void LinkPulse(Color c, PieceTile startTile, bool isFirstInLink)
        {
            OnLinkPulse?.Invoke(c, startTile, isFirstInLink);
        }
        #endregion 
    }

    public enum PieceState
    {
        INVENTORY,
        PLACED_GRID, 
        HOVER_GRID
    }
}
