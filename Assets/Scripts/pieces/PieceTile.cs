using UnityEngine;

namespace GamePieces
{
    public class PieceTile : MonoBehaviour
    {
        [SerializeField] private PieceTileType type;
        private Piece piece;
        private Vector2Int relativeOffset;

        public PieceTileType GetTileType() => type;
        public Piece GetPiece() => piece;
        public void SetPiece(Piece p) => piece = p;
        public Vector2Int GetRelativeOffset() => relativeOffset;
        public void SetRelativeOffset(Vector2Int vint) => relativeOffset = vint;
        public void RotateRelativeOffsetClockwise()
        {
            // todo
        }
        public void RotateRelativeOffsetCounterClockwise()
        {
            // todo
        }
        
    }
}
