using System.Collections.Generic;
using GamePieces;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace PuzzleGrid
{
    /// <summary>
    /// i say that gridTiles can hold representations of up to two piece tiles, if one is passthrough
    /// </summary>
    public class GridTile : MonoBehaviour
    {

        #region notifs
        public UnityAction OnJoinedLink;
        public UnityAction OnRerender;
        #endregion

        [SerializeField] private List<PieceTile> linkedTiles = new(2);
        private GridTileType tileType;
        private LinkPlacementData goalLinkData;
        private SpriteRenderer spriteRenderer;

        public GridTileType GetGridTileType()
        {
            return tileType;
        }
        public void SetTileType(GridTileType t) 
        {
            tileType = t;
        }
        public bool IsStartTile() => tileType == GridTileType.START;
        public bool IsEndTile() => tileType == GridTileType.END;
        public LinkPlacementData GetLinkPlacementData()
        {
            if (!IsStartTile() && !IsEndTile()) return null;
            return goalLinkData;
        }
        public bool HasLinkPlacementData() => goalLinkData != null;
        public void SetLinkPlacementData(LinkPlacementData data)
        {
            if (!IsStartTile() && !IsEndTile()) { Debug.LogWarning($"Warning: Tried to set link placement data on {name} but this tile is of type: {tileType}"); return; }
            goalLinkData = data;
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
            
            tile.Placed(this);
            
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

        public SpriteRenderer ResetChildRenderer(string childName)
        {  
            if (spriteRenderer != null)
            {
                Destroy(spriteRenderer.gameObject);
                spriteRenderer = null;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            spriteRenderer = child.AddComponent<SpriteRenderer>();
            return spriteRenderer;
        }

        public void SetSprite(Sprite spr)
        {
            spriteRenderer.sprite = spr;
        }

        public void SetLinkColorByPlacementData()
        {
            if (spriteRenderer == null) {Debug.LogError("Called SetLinkColorByPlacementData but spriteRenderer is null"); return; }
            if (goalLinkData == null) {Debug.LogError("Called SetLinkColorByPlacementData but has no placement data."); return;}

            spriteRenderer.color = tileType == GridTileType.START ? goalLinkData.GetPulseColor() : goalLinkData.GetBaseColor();
        }
        public void MultColor(Color with)
        {
            spriteRenderer.color *= with;
        }
    }
}
