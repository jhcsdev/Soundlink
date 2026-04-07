using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    public class PieceTile : MonoBehaviour
    {
        [SerializeField] private PieceTileType type;
        private Piece piece;
        private Vector2 relativeOffset;

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
                transform.localPosition = relativeOffset * tileSize;
            }
            return this;
        }
        #endregion

        public List<GlueCardinality> GetGlue => ptd.glue;
        public PieceTileType GetTileType() => type;
        public Piece GetPiece() => piece;
        public void SetPiece(Piece p) => piece = p;
        public Vector2Int GetUnrotatedRelativeOffset() => ptd.relativeOffset;
        public void SetRelativeOffset(Vector2Int vint) => relativeOffset = vint;
        public void RotateRelativeOffsetClockwise()
        {
            float x = relativeOffset.y * 1;
            float y = relativeOffset.x * -1;
            relativeOffset = new(x, y);
        }
        public void RotateRelativeOffsetCounterClockwise()
        {
            float x = relativeOffset.y * -1;
            float y = relativeOffset.x * 1;
            relativeOffset = new(x, y);
        }
    }
}
