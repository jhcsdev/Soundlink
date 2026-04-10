using System.Collections.Generic;
using System.Xml.Serialization;
using GamePieces;
using Unity.Collections;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

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

        // TODO: create a list of links ..
        #endregion

        void Awake()
        {
            Debug.Log("width: " + data.width);
            Debug.Log("height: " + data.height);
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

                    // TODO: at this point will have the tile type, then can set the color based on that ? 
                    GridTileType tileType = data.GetTileInfo(x, y, out int soundID);
                    gridTile.tileType = tileType;
                    

                    // set color based on tile type
                    sr.color = GetTileColor(tileType);
                    sr.sprite = GridTileSprite;
                    sr.sortingOrder = -1; // put it behind everything
                    tiles[x,y] = gridTile;
                }
            }
            Debug.Log("grid setup");
        }

        // get color based on tile type
        private Color GetTileColor(GridTileType tileType)
        {

            if (tileType == GridTileType.START)
            {
                return Color.green;
            }

            if (tileType == GridTileType.END)
            {
                return Color.red;
            }

            return gridTileUnfocusedColor;
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
            // TODO: some error here when I am visiting a start tile I am making it a start tile
            GridTileType tileType = tiles[focusPosition.x, focusPosition.y].GetGridTileType();
            tiles[focusPosition.x, focusPosition.y].GetComponent<SpriteRenderer>().color = GetTileColor(tileType);

            focusPosition = intended;
            OnMoved?.Invoke(direction);
            OnNewFocusPosition?.Invoke(intended);

            // todo - this is just temporary to show where we are on the grid
            // TODO: we are not showin
            tiles[focusPosition.x, focusPosition.y].GetComponent<SpriteRenderer>().color = gridTileFocusedColor;

            return Vector2Int.zero;
        }

        // get grid tile that is adjacent to current tile, based on direction of focus (ie North, South, East, or West)
        public GridTile GetNeighborTile(GlueCardinality gc,  GridTile[,] tiles, Vector2Int checkingPosition)
        {
            if (gc == GlueCardinality.NORTH)
            {
                if (checkingPosition.y + 1 >= tiles.GetLength(1)) return null;
                return tiles[checkingPosition.x, checkingPosition.y + 1];
            }
            if (gc == GlueCardinality.SOUTH)
            {
                if (checkingPosition.y -1 < 0) return null;
                return tiles[checkingPosition.x, checkingPosition.y - 1];
            }
            if (gc == GlueCardinality.WEST)
            {
                if (checkingPosition.x -1 < 0) return null;
                return tiles[checkingPosition.x - 1, checkingPosition.y];
            }

            // EAST
            if (checkingPosition.y + 1 >= tiles.GetLength(0)) return null;
            return tiles[checkingPosition.x + 1, checkingPosition.y];
        }

        public bool GlueCardinalitiesCompatabile(GlueCardinality glueOne, GlueCardinality glueTwo)
        {
            if (glueOne == GlueCardinality.NORTH && glueTwo == GlueCardinality.SOUTH)
            {
                return true;
            }
            if (glueOne == GlueCardinality.SOUTH && glueTwo == GlueCardinality.NORTH)
            {
                return true;
            }
            if (glueOne == GlueCardinality.WEST && glueTwo == GlueCardinality.EAST)
            {
                return true;
            }
            if (glueOne == GlueCardinality.EAST && glueTwo == GlueCardinality.WEST)
            {
                return true;
            }
            return false;
        }

        // TODO: when successful, should add to a link ... 
        public override Vector2Int? PlaceAtFocusPosition(Piece p)
        {
            // yes, must first CHECK then SET, because we check incrementally - if we bulldozed straight to 
            // setting, then we might have to "unset" which is kinda complicated.
            if (!CanPieceBePlaced(p)) return null; 

            // int connectedTracks = 0;
            int connectedTracks = 0;

            foreach(PieceTile checkPiece in p.GetPieceTiles())
            {
                // limit of 2 connected tracks
                if (connectedTracks >= 2)
                {
                    Debug.Log("Too many tracks connected to this piece already");
                    continue;
                }

                // get the glue at this piecetile
                List<GlueCardinality> pieceTileGlue = checkPiece.GetGlue;
                if (pieceTileGlue == null || pieceTileGlue.Count == 0)
                {
                    Debug.Log("Piece tile did not have any glue");
                    continue;
                }

                // get the relative position of this pieceTile
                Vector2Int checkingPosition = focusPosition + checkPiece.GetUnrotatedRelativeOffset();

                // otherwise, check all tiles adjacent to glue
                foreach (GlueCardinality gc in pieceTileGlue)
                {
                    Debug.Log("This piece tile has some glue!");

                    // grab the neighboring tile
                    GridTile neighborGridTile = GetNeighborTile(gc, tiles, checkingPosition);

                    // make sure it exists
                    if (neighborGridTile == null) {
                        Debug.Log("no neighbor tiles!");
                        continue;
                    }

                    // if that grid tile has a piece on it, check all those glues
                    List<PieceTile> neighborPieceTiles = neighborGridTile.GetPieceTiles();

                    // NOTE: it appears that grids can have multiple pieces on them? should confer ... 
                    foreach (PieceTile neighborPiece in neighborPieceTiles) {
                        // make sure piece exists
                        if (neighborPiece == null) continue;  

                        // grab the glues of this neighboring piece!
                        List<GlueCardinality> neighborGC = neighborPiece.GetGlue;     

                        // check the glues .. if cardinality matches, then create a link
                        foreach (GlueCardinality someGC in neighborGC)
                        {
                            if (GlueCardinalitiesCompatabile(gc, someGC))
                            {
                                // TODO: create a link! (actually)
                                Debug.Log("A link would be greated here!");

                                // at this point, create a link ... what does that mean though? should check if there is an existing link, right?
                                connectedTracks++;
                                continue;
                            }
                            
                        }             
                    }
                }

                // TODO: something about checking that what i am connecting with is not already a link, or something like that ...
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

        // TODO: when successful, remove piece from any links that it is a part of .. 
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
