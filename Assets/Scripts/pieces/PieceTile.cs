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
            int x = relativeOffset.y * 1;
            int y = relativeOffset.x * -1;
            relativeOffset = new(x, y);
        }
        public void RotateRelativeOffsetCounterClockwise()
        {
            int x = relativeOffset.y * -1;
            int y = relativeOffset.x * 1;
            relativeOffset = new(x, y);
        }
    }
}
