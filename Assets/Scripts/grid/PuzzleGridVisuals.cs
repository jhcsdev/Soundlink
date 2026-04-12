

using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    [RequireComponent(typeof(PuzzleGrid))]
    public class PuzzleGridVisuals : MonoBehaviour
    {
        #region variables

        #region state

        #endregion
        
        PuzzleGrid grid;

        #endregion

        void Awake()
        {
            grid = GetComponent<PuzzleGrid>();
        }

        #region initialize
        void OnEnable()
        {
            grid.OnFailedLeavingGrid += FocusPositionFailedChange;
            grid.OnNewFocusedTile += FocusPositionSuccessfullyChanged;
            grid.OnGridFocused += GridFocused;
            grid.OnGridUnfocused += GridUnfocused;
            grid.OnGridInitialize += InitializeValues;

            grid.OnTileUpdated += TileUpdated;
            grid.OnHoveringTile += HoverTile;
            grid.OnGridTileInitialize += InitializeTile;
            grid.OnPieceYoinked += PickupPiece;
            grid.OnPiecePlacementSuccess += 
            grid.OnPiecePlacementFailure += 
            grid.OnPieceSentBackToInventory += SendPieceToInventory;
            grid.OnNewHover += ResetHovers;
        }
        void OnDisable()
        {
            grid.OnFailedLeavingGrid -= FocusPositionFailedChange;
            grid.OnNewFocusedTile -= FocusPositionSuccessfullyChanged;
            grid.OnGridFocused -= GridFocused;
            grid.OnGridUnfocused -= GridUnfocused;
            grid.OnGridInitialize -= InitializeValues;
        }

        void InitializeValues(int width, int height)
        {
            
        }
        #endregion

        #region actual visual implementations
        
        #region focusing grid / pointer
        void GridFocused()
        {
            
        }

        void GridUnfocused()
        {
            
        }

        void FocusPositionSuccessfullyChanged(Vector2Int direction, GridTile toTile)
        {
            
        }

        void FocusPositionFailedChange(Vector2Int direction)
        {
            
        }
        #endregion

        #region links
        void LinkFormed(GridLink link)
        {
            
        }
        #endregion

        #region tile updates
        void TileUpdated(GridTile tile)
        {
            
        }

        void HoverTile(GridTile tile)
        {
            
        }
        
        void InitializeTile(GridTile tile, GridTileType type, Vector2 position)
        {
            
        }

        void ResetHovers()
        {
            
        }
        #endregion

        #region piece updates
        void PickupPiece(Piece p)
        {
            p.gameObject.SetActive(true);
        }
        void SendPieceToInventory(Piece p)
        {
            p.gameObject.SetActive(true);
            // todo: cool visuals? maybe it flies back to inventory...who knows
        }

        void PlacePieceFailure(Piece p)
        {
            // todo: flash piece red, scale it slightly?
        }
        void PlacePieceSuccess(Piece p)
        {
            p.gameObject.SetActive(false);
        }
        #endregion
        #endregion 
    }
}