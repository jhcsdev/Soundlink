using GamePieces;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleGrid
{
    /// <summary>
    /// models a grid with pieces. grid coordinates go 0,0 up from the bottom left. of the grid.
    /// </summary>
    public class PuzzleGrid : PlayerInteractableGrid
    {
        [SerializeField] private Vector2 gridBottomLeftPosition = Vector2.zero;
        [SerializeField] private GridData data; 
        [SerializeField] private float tileRealsize = 1f;

        private GridTile[,] tiles; // index via [x,y]

        #region notifications
        public UnityAction<Vector2> OnFailedLeavingGrid;
        public UnityAction<Vector2> OnMoved;
        public UnityAction<Vector2Int> OnNewFocusPosition;
        #endregion

        void Awake()
        {
            tiles = new GridTile[data.width, data.height];
            for (int x = 0; x < data.width; x++)
            {
                for (int y = 0; y < data.height; y++)
                {
                    GameObject tileObj = new($"Tile {x}, {y}");
                    tileObj.transform.parent = transform;
                    tileObj.transform.position = gridBottomLeftPosition + new Vector2(x * tileRealsize, y * tileRealsize);
                    GridTile gridTile = tileObj.AddComponent<GridTile>();
                    tiles[x,y] = gridTile;
                }
            }
            Debug.Log("grid setup");
        }

        public override Vector2Int ShiftFocusPosition(Vector2Int direction)
        {
            if (direction == Vector2.zero) return Vector2Int.zero;

            Vector2Int intended = focusPosition + direction;

            // check x pos, y up
            if (intended.x < 0 || intended.x >= data.width || intended.y >= data.height) { OnFailedLeavingGrid?.Invoke(direction); return Vector2Int.zero; }
            // check y down - are we going back to the inventory?
            if (intended.y < 0) { OnMoved?.Invoke(direction); return Vector2Int.down; }

            // otherwise movement is ok
            focusPosition = intended;
            OnMoved?.Invoke(direction);
            OnNewFocusPosition?.Invoke(intended);

            return Vector2Int.zero;
        }

        public override Vector2Int? PlaceAtFocusPosition(Piece p)
        {
            if (!CanPieceBePlaced(p)) return null;

            foreach(PieceTile checkPiece in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + checkPiece.GetUnrotatedRelativeOffset();
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];
                if (!tileAtPosition.TrySetPieceTile(checkPiece)) return null;
            }

            return Vector2Int.zero;
        }

        private bool CanPieceBePlaced(Piece p)
        {
            // iterate through piecetiles relative to origin, compare them to gridtiles
            foreach (PieceTile checkPiece in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + checkPiece.GetUnrotatedRelativeOffset();
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];
                if (!tileAtPosition.CanSetPieceTile(checkPiece)) return false;
            }

            return true;
        }

        public override Piece TakeAtFocusPosition()
        {
            GridTile focus = tiles[focusPosition.x, focusPosition.y];
            PieceTile atFocus = focus.TryGrabBottomTile();
            if (atFocus == null) return null;

            Vector2Int atFocusOffset = atFocus.GetUnrotatedRelativeOffset(); // might not be starting from 0,0 this time

            // need to remove the rest of the piece tiles
            foreach (PieceTile checkTile in atFocus.GetPiece().GetPieceTiles())
            {
                if (checkTile == atFocus) continue;
                Vector2Int checkingPosition = focusPosition - atFocusOffset + checkTile.GetUnrotatedRelativeOffset();
                GridTile testing = tiles[checkingPosition.x, checkingPosition.y];
                if (!testing.RemovePieceTile(checkTile))
                {
                    // if this failed, we are probably looking at the wrong coordinates?
                    Debug.LogWarning($"Failed to remove a piece from the grid, tile: {checkTile.name}");
                }
            }

            return atFocus.GetPiece();
        }
    }
}
