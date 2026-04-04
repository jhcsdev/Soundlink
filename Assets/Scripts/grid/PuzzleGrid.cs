using GamePieces;
using Unity.Collections;
using UnityEditor.Tilemaps;
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
        [SerializeField] private Sprite GridTileSprite;
        // todo - temporary colors
        private Color gridTileUnfocusedColor = new(0.2f, 0.2f, 0.2f, 0.2f);
        private Color gridTileFocusedColor = new(0.2f, 0.2f, 0.2f, 0.4f);

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
                    tileObj.transform.localPosition = gridBottomLeftPosition + new Vector2(x * tileRealsize, y * tileRealsize);
                    GridTile gridTile = tileObj.AddComponent<GridTile>();
                    SpriteRenderer sr = tileObj.AddComponent<SpriteRenderer>(); // todo: probably temporary visuals
                    sr.sprite = GridTileSprite;
                    sr.color = gridTileUnfocusedColor;
                    sr.sortingOrder = -1; // put it behind everything
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
            tiles[focusPosition.x, focusPosition.y].GetComponent<SpriteRenderer>().color = gridTileUnfocusedColor;

            focusPosition = intended;
            OnMoved?.Invoke(direction);
            OnNewFocusPosition?.Invoke(intended);

            // todo - this is just temporary to show where we are on the grid
            tiles[focusPosition.x, focusPosition.y].GetComponent<SpriteRenderer>().color = gridTileFocusedColor;

            return Vector2Int.zero;
        }

        public override Vector2Int? PlaceAtFocusPosition(Piece p)
        {
            if (!CanPieceBePlaced(p)) return null; // yes, must first CHECK then SET, because we check incrementally - if we bulldozed straight to 
            // setting, then we might have to "unset" which is kinda complicated.

            foreach(PieceTile checkPiece in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + checkPiece.GetUnrotatedRelativeOffset();
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];
                if (!tileAtPosition.TrySetPieceTile(checkPiece)) return null;
            }

            p.GridMode();
            p.transform.parent = transform;

            SetPieceToFocusPosition(p);

            return Vector2Int.zero;
        }

        private void SetPieceToFocusPosition(Piece p)
        {
            p.transform.position = tiles[focusPosition.x, focusPosition.y].transform.position;
        }

        private bool CanPieceBePlaced(Piece p)
        {
            // iterate through piecetiles relative to origin, compare them to gridtiles
            foreach (PieceTile checkPiece in p.GetPieceTiles())
            {
                // todo: getunrotatedrelativeoffset means that this function will not properly check rotated tiles
                Vector2Int checkingPosition = focusPosition + checkPiece.GetUnrotatedRelativeOffset();
                if (checkingPosition.x < 0 || checkingPosition.x >= tiles.GetLength(0) || checkingPosition.y < 0 || checkingPosition.y >= tiles.GetLength(1)) return false;
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

            return atFocus.GetPiece().GridMode();
        }

        public override void Hover(Piece p)
        {
            // todo: lock all PieceTiles in p.GetTiles() to grid offset tiles
            // temporary: just manually set position to focusposition
            SetPieceToFocusPosition(p);
        }
    }
}
