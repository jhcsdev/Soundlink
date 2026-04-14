using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    public class PieceTile : MonoBehaviour
    {
        [SerializeField] private PieceTileType type;
        private Sprite tileSprite; // todo - only supports tile right now, no overlay
        private TileSpriteDirection spriteDirection;
        private Piece piece;
        private Vector2Int relativeOffset;

        private PieceTileData ptd;
        private bool initialized = false;

        #region chaining functions
        public PieceTile Initialize(PieceTileData data, Piece parent) // returns self for chaining purposes
        {
            ptd = data;
            relativeOffset = ptd.relativeOffset;
            piece = parent;
            type = ptd.type;
            initialized = true;
            return this;
        }
        private bool IsInitialized() { if (initialized) return true; Debug.Log(name + " not initialized!"); return false; }
        public PieceTile SetParent(Transform to)
        {
            if (IsInitialized()) transform.parent = to;
            return this;
        }
        public PieceTile SetLocalPosition(Vector2 to)
        {
            if (IsInitialized()) transform.localPosition = to; 
            return this;
        }
        public PieceTile SetLocalPositionAndScaleByTileSize(float tileSize)
        {
            if (IsInitialized()) {
                transform.localScale = Vector2.one * tileSize;
                transform.localPosition = new Vector2(relativeOffset.x, relativeOffset.y) * tileSize;
            }
            return this;
        }

        public PieceTile SetPiece(Piece p) { piece = p; return this; }
        public PieceTile SetRelativeOffset(Vector2Int vint) { relativeOffset = vint; return this; }
        public PieceTile SetSpriteVariable(Sprite sprite) { tileSprite = sprite; return this; }
        public PieceTile SetTileDirection(TileSpriteDirection direction)  { spriteDirection = direction; return this; }
        #endregion

        #region getters
        public List<GlueCardinality> GetGlue => ptd.glue;
        public PieceTileType GetTileType() => type;
        public Piece GetPiece() => piece;
        public Vector2Int GetUnrotatedRelativeOffset() => ptd.relativeOffset;
        public Vector2Int GetRotatedRelativeOffset() => relativeOffset;
        public Sprite GetSprite() => tileSprite;
        public TileSpriteDirection GetSpriteDirection() => spriteDirection;
        #endregion

        public void RotateRelativeOffsetClockwise()
        {
            int x = relativeOffset.y * 1;
            int y = relativeOffset.x * -1;
            relativeOffset = new(x, y);
            SetLocalPosition(relativeOffset);
            spriteDirection = TileManager.RotateCW90(spriteDirection);
        }
        public void RotateRelativeOffsetCounterClockwise()
        {
            int x = relativeOffset.y * -1;
            int y = relativeOffset.x * 1;
            relativeOffset = new(x, y);
            SetLocalPosition(relativeOffset);
            spriteDirection = TileManager.RotateCCW90(spriteDirection);
        }
    }
}
