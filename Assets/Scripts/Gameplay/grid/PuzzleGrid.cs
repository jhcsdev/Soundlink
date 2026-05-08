using System.Collections.Generic;
using System.Linq;
using GamePieces;
using GridLinks;
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
        [SerializeField] private float tileRealsize = 1f;

        private GridData gridData; 

        private GridTile[,] tiles; // index via [x,y]
        public List<GridLink> gridLinks = new();

        private int totalTracksInGrid = 0;

        #region notifications
        private bool pointerActive = true;
        public UnityAction<GridTile> OnActivatePointer;
        public UnityAction<GridTile> OnDeactivatePointer;
        public UnityAction<Vector2Int /*direction */> OnFailedLeavingGrid;
        
        public UnityAction<Vector2Int /*direction*/, GridTile /*FocusedTile*/, bool /*pointerActive*/> OnNewFocusedTile;
        public UnityAction<Vector2Int> OnNewHover;
        public UnityAction<GridTile> OnHoveringTile;
        public UnityAction<GridTile, GridTileType, Vector2> OnGridTileInitialize;
        public UnityAction OnGridFinishedInitialize;

        public UnityAction<Piece> OnPiecePlacementFailure;
        public UnityAction<Piece> OnPiecePlacementSuccess;
        public UnityAction<Piece> OnPieceYoinked;
        public UnityAction<Piece> OnPieceSentBackToInventory;

        public UnityAction GameWon;
        public UnityAction<GridLink> OnAStartLinkUpdated;
        public UnityAction<int> OnAStartLinkFullyDestroyed;
        #endregion

        #region unity functions
        void OnEnable()
        {
            if (LevelLoader.instance == null) Debug.LogError("Warning: A LevelLoader needs to exist in the scene for grid to build.");
            if (LevelLoader.instance.GetGridData() == null) Debug.LogError("Warning: LevelLoader exists, but grid failed to retrieve GridData.");

            gridData = LevelLoader.instance.GetGridData();
            tiles = new GridTile[gridData.width, gridData.height];
        }

        void Start()
        {
            for (int x = 0; x < gridData.width; x++)
            {
                for (int y = 0; y < gridData.height; y++)
                {
                    GameObject tileObj = new($"Tile {x}, {y}");
                    tileObj.transform.parent = transform;
                    Vector2 pos = new(x, y);
                    tileObj.transform.localPosition = gridBottomLeftPosition + pos * tileRealsize;
                    GridTile gridTile = tileObj.AddComponent<GridTile>();

                    // grab and set color based on tiletype
                    GridTileType tileType = gridData.GetTileInfo(x, y, out LinkPlacementData placementData);
                    gridTile.SetTileType(tileType);
                    if (tileType == GridTileType.START || tileType == GridTileType.END)
                    {
                        totalTracksInGrid += tileType == GridTileType.START ? 1 : 0;
                        gridTile.SetLinkPlacementData(placementData);
                    }

                    tiles[x,y] = gridTile;

                    OnGridTileInitialize?.Invoke(gridTile, tileType, pos);
                }
            }

            OnGridFinishedInitialize?.Invoke();
        }
        #endregion

        #region overriden (implemented) functions
        public override Vector2Int ShiftFocusPosition(Vector2Int direction)
        {
            if (direction == Vector2.zero) return Vector2Int.zero;

            Vector2Int intended = focusPosition + direction;

            // check x pos, y up
            if (intended.x < 0 || intended.x >= gridData.width || intended.y < 0 || intended.y >= gridData.height) { 
                if (pointerActive) { OnFailedLeavingGrid?.Invoke(direction); } 
                return Vector2Int.zero; 
            }

            // otherwise movement is ok
            focusPosition = intended;
            OnNewFocusedTile?.Invoke(direction, tiles[focusPosition.x, focusPosition.y], pointerActive);

            return Vector2Int.zero;
        }
        public override Vector2Int? PlaceAtFocusPosition(Piece p)
        {
            // first check, then set, to avoid having to "unset"
            if (!CanPieceBePlaced(p)) {
                OnPiecePlacementFailure?.Invoke(p);
                return null; 
            }

            // place the tiles down
            foreach(PieceTile pt in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + pt.GetRotatedRelativeOffset();
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];

                if (tileAtPosition.IsStartTile()) AddLinkToKnownLinks(CreateStartLink(p, pt, tileAtPosition));
                else if (tileAtPosition.IsEndTile()) AddLinkToKnownLinks(CreateEndLink(p, pt, tileAtPosition));

                if (!tileAtPosition.TrySetPieceTile(pt)) return null;
            }

            // update links
            int connectedTracks = 0;

            foreach (PieceTile checkPiece in p.GetPieceTiles())
            {
                if (connectedTracks >= 2) break; // break, not continue - we're fully done

                List<GlueCardinality> pieceTileGlue = checkPiece.GetGlue;
                if (pieceTileGlue == null || pieceTileGlue.Count == 0) continue;

                Vector2Int checkingPosition = focusPosition + checkPiece.GetRotatedRelativeOffset();

                foreach (GlueCardinality gc in pieceTileGlue)
                {
                    GridTile neighborTile = GetNeighborTile(gc, tiles, checkingPosition);
                    if (neighborTile == null) continue;

                    foreach (PieceTile neighborPieceTile in neighborTile.GetPieceTiles())
                    {
                        if (neighborPieceTile == null) continue;
                        TryConnectPieces(p, checkPiece, neighborPieceTile, gc, ref connectedTracks);
                    }
                }
            }  

            p.PlacedGrid();
            p.transform.parent = transform;
            OnPiecePlacementSuccess?.Invoke(p);
            LogLinks();

            // TODO: add game winning scene!
            if (CheckIfGameWon()) {
                Debug.Log("YOU HAVE WON THE GAME!");
            }
            else
            {
                Debug.Log("you have not yet won the game ... but good luck :D");
            }

            ActivatePointerIfDeactivated();

            return Vector2Int.zero;
        }
        public override Piece TakeAtFocusPosition()
        {
            GridTile focus = tiles[focusPosition.x, focusPosition.y];
            PieceTile atFocus = focus.TryGrabBottomTile();
            if (atFocus == null) return null;

            Vector2Int atFocusOffset = atFocus.GetRotatedRelativeOffset(); // might not be starting from 0,0 this time

            // need to remove the rest of the piece tiles
            foreach (PieceTile checkTile in atFocus.GetPiece().GetPieceTiles())
            {
                if (checkTile == atFocus) continue; // skipped because TryGrabBottomTile removes it already
                Vector2Int checkingPosition = focusPosition - atFocusOffset + checkTile.GetRotatedRelativeOffset();
                GridTile testing = tiles[checkingPosition.x, checkingPosition.y];
                if (!testing.RemovePieceTile(checkTile))
                {
                    // if this failed, we are probably looking at the wrong coordinates?
                    Debug.LogWarning($"Failed to remove a piece from the grid, tile: {checkTile.name}");
                }
            }

            // remove piece from all links
            Piece toBeRemoved = atFocus.GetPiece();
            RemoveFromGridLinks(toBeRemoved);

            LogLinks();

            OnPieceYoinked?.Invoke(toBeRemoved);
            toBeRemoved.transform.position = GetFocusedGridTile().transform.position; // todo: should not be manually setting position

            DeactivatePointerIfActivated();

            // return
            return toBeRemoved.LimboMode(GetFocusedGridTile());
        }
        public override void Hover(Piece p)
        {
            OnNewHover?.Invoke(focusPosition);

            foreach (PieceTile checkPiece in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + checkPiece.GetRotatedRelativeOffset();
                if (checkingPosition.x < 0 || checkingPosition.x >= tiles.GetLength(0) || checkingPosition.y < 0 || checkingPosition.y >= tiles.GetLength(1)) continue;
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];
                if (tileAtPosition.CanSetPieceTile(checkPiece)) OnHoveringTile?.Invoke(tileAtPosition);
            }
            
            p.LimboMode(GetFocusedGridTile());
        }
        public override void FocusGrid(bool isHoldingPiece)
        {
            base.FocusGrid();
            if (!isHoldingPiece) ActivatePointerIfDeactivated(overrideCheck: true);
            else DeactivatePointerIfActivated();
        }
        public override void UnfocusGrid()
        {
            base.UnfocusGrid();
            DeactivatePointerIfActivated();
        }

        private void ActivatePointerIfDeactivated(bool overrideCheck = false)
        {
            if (!overrideCheck && pointerActive) return;
            pointerActive = true;
            OnActivatePointer?.Invoke(GetFocusedGridTile());
        }
        private void DeactivatePointerIfActivated()
        {
            if (!pointerActive) return;
            pointerActive = false;
            OnDeactivatePointer?.Invoke(GetFocusedGridTile());
        }
        
        #endregion
        
        #region link creation / manipulation
        private void TryConnectPieces(Piece p, PieceTile pieceTile, PieceTile neighborPieceTile, GlueCardinality gc, ref int connectedTracks)
        {
            bool hasCompatibleGlue = neighborPieceTile.GetGlue
                .Any(neighborGC => GlueCardinalitiesCompatabile(gc, neighborGC));
            List<GridLink> linksForBasePiece = GetGridLinks(p);

            if (!hasCompatibleGlue) return;

            Piece neighborPiece = neighborPieceTile.GetPiece();
            List<GridLink> existingLinks = GetGridLinks(neighborPiece);

            if (existingLinks.Count == 0)
            {
                if (linksForBasePiece.Count > 0)
                {
                    UpdateLink(linksForBasePiece[0], neighborPiece, neighborPieceTile, p, pieceTile);
                    connectedTracks++;
                }
                else
                {
                    Debug.Log("No existing link — creating new one.");
                    AddLinkToKnownLinks(CreateNewLink(p, pieceTile, neighborPiece, neighborPieceTile));
                    connectedTracks++;
                }
                return;
            }

            foreach (GridLink existingLink in existingLinks)
            {
                if (linksForBasePiece.Contains(existingLink))
                {
                    Debug.Log("Current piece already belongs to this link — skipping.");
                    continue;
                }

                Debug.Log("Adding current piece to neighbor's existing link.");
                if (linksForBasePiece.Count > 0)
                {
                    Debug.Log("\t Merging links");
                    MergeLinks(linksForBasePiece[0], existingLink, neighborPiece, p);
                }
                else
                {
                    Debug.Log("\t Updating existing link");
                    UpdateLink(existingLink, p, pieceTile, neighborPiece, neighborPieceTile);
                }
            }
        }

        /// <summary>
        /// remove piece from all gridlinks that contain it
        /// </summary>
        /// <param name="piece">
        ///     the piece to be removed
        /// </param>
        public void RemoveFromGridLinks(Piece piece)
        {
            List<GridLink> gridsContainedIn = GetGridLinks(piece);

            foreach (GridLink link in gridsContainedIn)
            {
                Debug.Log("Removing Piece from Link");
                if (!link.ContainsPiece(piece)) continue;
                int startIdIfExists = link.GetStartSoundIDIfExists(); // because we might lose the start data, we have to store before splitting, then reset (OnAStartLinkFullyDestroyed) if necessary

                if (!link.SplitLink(piece, out GridLink startLink, out GridLink endLink))
                {
                    Debug.LogWarning($"Tried to split link {link}, but it failed!");
                    return;
                }

                if (endLink != null && endLink.IsLinkLive()) 
                { 
                    Debug.Log($"Created new endLink {endLink}; adding to links."); gridLinks.Add(endLink);
                    if (endLink.HasStartData()) { Debug.LogWarning("Warning: 'end' Link created after splitting has start data."); }
                }

                if (startLink.HasStartData())
                {
                    OnAStartLinkUpdated?.Invoke(startLink);
                } 
                else
                {
                    Debug.Log($"Start link {startLink} died after split merge.");
                    if (!startLink.IsLinkLive()) gridLinks.Remove(link); // remove link from known links if necessary.
                    if (startIdIfExists >= 0) OnAStartLinkFullyDestroyed?.Invoke(startIdIfExists);
                }
            }
        }
        public GridLink CreateNewLink(Piece pieceOne, PieceTile pieceOneTile, Piece pieceTwo, PieceTile pieceTwoTile)
        {
            GridLink newLink = new();
            newLink.AddPiece(pieceOne, null, null, null);
            newLink.AddPiece(pieceTwo, pieceTwoTile, pieceOne, pieceOneTile);
            return newLink;
        }
        /// <summary>
        /// Creates a "Start Link" -- a link with just one piece and StartPlacementData.
        /// </summary>
        /// <param name="piece">The piece in the link.</param>
        /// <param name="startTile">The PieceTile of piece that is connected to gridTile.</param>
        /// <param name="gridTile">The gridTile with the StartPlacementData.</param>
        /// <returns>Newly-created link</returns>
        public GridLink CreateStartLink(Piece piece, PieceTile startTile, GridTile gridTile)
        {
            GridLink newLink = new(); 
            return newLink.SetStartPlacementData(gridTile.GetLinkPlacementData()).AddPiece(piece, startTile, null, null, true);
        }
        /// <summary>
        /// Creates an "End Link" -- a link with just one piece and EndPlacementData.
        /// </summary>
        /// <param name="piece">The piece in the link.</param>
        /// <param name="endTile">The PieceTile above gridTile.</param>
        /// <param name="gridTile">The GridTile with the StartPlacementData.</param>
        /// <returns>Newly-created link</returns>
        public GridLink CreateEndLink(Piece piece, PieceTile endTile, GridTile gridTile)
        {
            GridLink newLink = new(); 
            return newLink.SetEndPlacementData(gridTile.GetLinkPlacementData()).AddPiece(piece, endTile, null, null, false);
        }

        public void AddLinkToKnownLinks(GridLink what)
        {
            gridLinks.Add(what);
            if (what.HasStartData()) OnAStartLinkUpdated?.Invoke(what);
        }
        /// <summary>
        /// Adds a new piece to a given link.
        /// </summary>
        /// <param name="what">The link to be updated.</param>
        /// <param name="with">The piece being added.</param>
        /// <param name="withTile">The tile of with connected to neighborPiece.</param>
        /// <param name="neighborPiece">The piece (already in what) that will be used to base insertion of 'with'.</param>
        /// <param name="neighborTile">The tile of neighbor connected to withTile. </param>
        public void UpdateLink(GridLink what, Piece with, PieceTile withTile, Piece neighborPiece, PieceTile neighborTile)
        {
            what.AddPiece(with, withTile, neighborPiece, neighborTile);
            if (what.HasStartData()) OnAStartLinkUpdated?.Invoke(what);
        }
        public void MergeLinks(GridLink baseLink, GridLink mergeTo, Piece mergePivot, Piece basePivot)
        {
            if (baseLink.MergeLink(mergeTo, mergePivot, basePivot))
            {
                Debug.Log("Merge success.");
                gridLinks.Remove(mergeTo);
                if (baseLink.HasStartData()) OnAStartLinkUpdated?.Invoke(baseLink);
            } else { Debug.LogWarning("Merge failed."); }
        }
        #endregion
        
        #region link helpers
        // determine if Piece exists in GridLinks, return that link if true
        public List<GridLink> GetGridLinks(Piece piece)
        {
            List<GridLink> gridsContainedIn = new();

            foreach (GridLink link in gridLinks)
            {
                if (link.ContainsPiece(piece))
                {
                    gridsContainedIn.Add(link);
                }
            }
            return gridsContainedIn;
        }
        public List<GridLink> GetAllLinks() => gridLinks;
        #endregion

        #region helpers
        private Vector2Int GetPositionOfPieceTile(PieceTile pt)
        {
            for (int x = 0; x < gridData.width; x++)
                for (int y = 0; y < gridData.height; y++)
                    if (tiles[x, y].GetPieceTiles().Contains(pt))
                        return new Vector2Int(x, y);
            return new Vector2Int(-1, -1); // not found
        }
        public bool GlueCardinalitiesCompatabile(GlueCardinality glueOne, GlueCardinality glueTwo)
        {
            return (glueOne == GlueCardinality.NORTH && glueTwo == GlueCardinality.SOUTH)
                || (glueOne == GlueCardinality.SOUTH && glueTwo == GlueCardinality.NORTH)
                || (glueOne == GlueCardinality.WEST && glueTwo == GlueCardinality.EAST)
                || (glueOne == GlueCardinality.EAST && glueTwo == GlueCardinality.WEST);
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
            if (checkingPosition.x + 1 >= tiles.GetLength(0)) return null;
            return tiles[checkingPosition.x + 1, checkingPosition.y];
        }
        public bool CheckIfGameWon()
        {
            int numCompletedTracks = 0;
            foreach(GridLink link in gridLinks)
            {
                numCompletedTracks += link.IsLinkComplete() ? 1 : 0;
            }

            Debug.Log($"-- checking game state! -- \n\t completed tracks: {numCompletedTracks} \n\t total in grid: {totalTracksInGrid}");
            return numCompletedTracks == totalTracksInGrid;
        }
        private bool CanPieceBePlaced(Piece p)
        {
            // iterate through piecetiles relative to origin, compare them to gridtiles
            foreach (PieceTile checkPiece in p.GetPieceTiles())
            {
                Vector2Int checkingPosition = focusPosition + checkPiece.GetRotatedRelativeOffset();
                if (checkingPosition.x < 0 || checkingPosition.x >= tiles.GetLength(0) || checkingPosition.y < 0 || checkingPosition.y >= tiles.GetLength(1)) return false;
                GridTile tileAtPosition = tiles[checkingPosition.x, checkingPosition.y];
                if (!tileAtPosition.CanSetPieceTile(checkPiece)) return false;
            }

            return true;
        }
        private GridTile GetFocusedGridTile()
        {
            return tiles[focusPosition.x, focusPosition.y];
        }
        #endregion
    
        #region other
        // TODO: delete the visuals and just do logging stuff 
        public void LogLinks()
        {
            // Debug.Log($"{gridLinks.Count} links exist");

            for (int i = 0; i < gridLinks.Count; i++)
            {
                // Debug.Log($"Link {i} has {gridLinks[i].GetPieces().Count} pieces");

                foreach (var pd in gridLinks[i].GetPieces())
                {
                    // Debug.Log($"  Piece {piece.name} has {piece.GetPieceTiles().Count} piecetiles");
                    foreach (PieceTile pt in pd.piece.GetPieceTiles())
                    {
                        Vector2Int pos = GetPositionOfPieceTile(pt);
                        // Debug.Log($"    PieceTile {pt.name} found at pos {pos}");
                    }
                }
            }
        }
        #endregion
    }
}
