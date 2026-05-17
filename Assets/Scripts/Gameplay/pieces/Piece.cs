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
        private Material instance;

        #region
        public UnityAction OnPiecePlacedOnGrid;
        public UnityAction OnPieceFailedToPlaceOnGrid;
        public UnityAction OnPieceStartedHover;
        public UnityAction OnPiecePickedUp;
        public UnityAction<Transform /*new focus*/> OnHoverPieceMoved;
        public UnityAction OnPieceReturnedToInventory;
        public UnityAction<Color, PieceTile, bool, float> OnLinkPulse;
        public UnityAction<LinkPlacementData, PieceTile> OnJoinedLink;
        public UnityAction OnRotateNeedsSync;
        public UnityAction OnLeftLink;
        
        public UnityAction<Material> OnSetMaterial;
        public UnityAction<Vector2> OnMaterialFillOriginChange;
        public UnityAction<float> OnMaterialFillAmountChange;
        public UnityAction<Color> OnMaterialBaseColorChange;
        public UnityAction<Color> OnMaterialBorderColorChange;
        public UnityAction<Color> OnMaterialFillFadeColorChange;
        public UnityAction<Color> OnMaterialFillBaseColorChange;
        public UnityAction<float> OnMaterialFillFadeDistanceChange;
        public UnityAction<float> OnMaterialBorderThicknessChange;
        #endregion

        private readonly List<PieceTile> tileObjects = new();
        private bool initialized = false;

        // this is a canvas object that is exclusively used for the inventory, and is not really within the "bounds" of being controlled by the Piece.
        // it is not a child object. 
        private GameObject canvasPiece;
        public GameObject GetCanvasPiece() => canvasPiece;

        #region initialization
        public void Initialize(PieceData pd)
        {
            if (initialized == true) { Debug.Log("Duplicate intiialization on piece: " + name); return; }

            initialized = true;
            data = pd;

            CreateTiles();
            gameObject.AddComponent<PieceVisuals>();

            EmitMaterialUpdate(
                materialSet: instance, 
                fillAmount: -0.2f, 
                fillOrigin: new(0,0), 
                fillFadeColor: Color.white, 
                fillBaseColor: Color.black, 
                borderColor: Color.black, 
                baseColor: Color.white,
                borderThickness: 0.15f,
                fillFadeDistance: 5f
            );
        }
        private void CreateTiles()
        {
            canvasPiece = new GameObject("CanvasPiece", typeof(RectTransform));
            instance = new(TileManager.Instance.GetPieceMaterial());

            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = TileManager.Instance.CreatePieceAndCanvasTile(ptd, out GameObject canvasVisual, isSilent: data.isSilentPiece)
                    .Initialize(ptd, this)
                    .SetParent(transform)
                    .SetLocalPositionAndScaleByTileSize(tileScale)
                    .CreateGlueSprites()
                    .SetRendererMaterialData(instance);
                tileObjects.Add(t);
                
                canvasVisual.transform.SetParent(canvasPiece.transform, false);

                RectTransform rt = canvasVisual.GetComponent<RectTransform>();
                float tileSize = rt.sizeDelta.x; 
                rt.anchoredPosition = new Vector2(ptd.relativeOffset.x * tileSize, ptd.relativeOffset.y * tileSize);
            }
        }
        private void EmitMaterialUpdate(Material materialSet = null, float? fillAmount = null, Vector2? fillOrigin = null, 
            Color? fillFadeColor = null, Color? borderColor = null, Color? baseColor = null, Color? fillBaseColor = null, 
            float? borderThickness = null, float? fillFadeDistance = null)
        {
            if (materialSet != null) OnSetMaterial?.Invoke(materialSet);
            if (fillAmount != null) OnMaterialFillAmountChange?.Invoke(fillAmount.Value);
            if (fillOrigin != null) OnMaterialFillOriginChange?.Invoke(fillOrigin.Value);
            if (fillFadeColor != null) OnMaterialFillFadeColorChange?.Invoke(fillFadeColor.Value);
            if (borderColor != null) OnMaterialBorderColorChange?.Invoke(borderColor.Value);
            if (baseColor != null) OnMaterialBaseColorChange?.Invoke(baseColor.Value);
            if (fillBaseColor != null) OnMaterialFillBaseColorChange?.Invoke(fillBaseColor.Value);
            if (borderThickness != null) OnMaterialBorderThicknessChange?.Invoke(borderThickness.Value);
            if (fillFadeDistance != null) OnMaterialFillFadeDistanceChange?.Invoke(fillFadeDistance.Value);
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
            foreach(var pieceTile in GetPieceTiles()) pieceTile.SetRendererIndex(GetRendererIndexByTileType(pieceTile.GetTileType(), false));

            transform.localScale = Vector2.one * gridTileScale; 
            transform.position = focusedTile.transform.position;

            return this;
        }
        public Piece PlacedGrid()
        {
            if (currentState == PieceState.PLACED_GRID) return this;
            currentState = PieceState.PLACED_GRID;

            OnPiecePlacedOnGrid?.Invoke();
            foreach(var pieceTile in GetPieceTiles()) pieceTile.SetRendererIndex(GetRendererIndexByTileType(pieceTile.GetTileType(), true));
                        
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
        private int GetRendererIndexByTileType(PieceTileType t, bool placedDown) => t switch
        {
            PieceTileType.PASSTHROUGH => (int)(placedDown ? SPRITE_ORDER.PIECE_TILE_PASSTHROUGH_SPRITE_INDEX_PLACED_GRID : SPRITE_ORDER.PIECE_TILE_PASSTHROUGH_SPRITE_INDEX_HOVER_GRID),
            _ => (int)(placedDown ? SPRITE_ORDER.PIECE_TILE_SPRITE_INDEX_PLACED_GRID : SPRITE_ORDER.PIECE_TILE_SPRITE_INDEX_HOVER_GRID) 
        };
        #endregion

        #region link stuff
        public void JoinLink(LinkPlacementData link, PieceTile joinTile)
        {
            OnJoinedLink?.Invoke(link, joinTile);
        }
        public void LeaveLink(PieceTile tile)
        {
            OnLeftLink?.Invoke();
        }
        public void LinkPulse(Color c, PieceTile startTile, bool isFirstInLink, float secondsPerBeat)
        {
            OnLinkPulse?.Invoke(c, startTile, isFirstInLink, secondsPerBeat);
        }
        public void UpdateFillLocation(Vector2 where)
        {
            EmitMaterialUpdate(fillOrigin: where);
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
