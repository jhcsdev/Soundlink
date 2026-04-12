using System.Collections.Generic;
using GamePieces;
using UnityEngine;
using PuzzleGrid.Visuals;
using UnityEngine.Tilemaps;
using System.Linq;
using UnityEngine.Events;

namespace PuzzleGrid
{
    /// <summary>
    /// i say that gridTiles can hold representations of up to two piece tiles, if one is passthrough
    /// </summary>
    [RequireComponent(typeof(GridTileVisuals))]
    public class GridTile : MonoBehaviour
    {

        #region notifs
        public UnityAction OnJoinedLink;
        public UnityAction OnRerender;
        #endregion

        [SerializeField] private List<PieceTile> linkedTiles = new(2);
        public GridTileType tileType;

        public GridTileType GetGridTileType()
        {
            return tileType;
        }

        public bool HasPieceTile() => linkedTiles.Count > 0;

        public List<PieceTile> GetPieceTiles()
        {
            return linkedTiles;
        }

        public bool CanSetPieceTile(PieceTile settingTile)
        {
            // either no pieces there, piece is already there and is passthrough, or newly added is passthrough
            return linkedTiles.Count <= 0 
                || (linkedTiles.Count == 1 && linkedTiles[0].GetTileType() == PieceTileType.PASSTHROUGH)
                || (linkedTiles.Count == 1 && settingTile.GetTileType() == PieceTileType.PASSTHROUGH);
        }

        public bool TrySetPieceTile(PieceTile tile)
        {
            if (!CanSetPieceTile(tile)) return false;

            InstantAddPieceTile(tile);
            Rerender();
            return true;
        }
        private bool InstantAddPieceTile(PieceTile tile)
        {
            linkedTiles.Add(tile); return true;
        }

        public bool RemovePieceTile(PieceTile tile)
        {
            for (int i = linkedTiles.Count - 1; i >= 0; i--)
            {
                if (linkedTiles[i] != tile) continue;
                linkedTiles.RemoveAt(i);
                Rerender();
                return true;
            }
            return false;
        }
        public PieceTile TryGrabBottomTile()
        {
            if (linkedTiles.Count == 0) return null;
            PieceTile pieceTile = linkedTiles[0];
            linkedTiles.RemoveAt(0);
            return pieceTile;
        }

        private void Rerender()
        {
            OnRerender?.Invoke();
        }
    }
}
