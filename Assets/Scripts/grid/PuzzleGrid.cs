using UnityEngine;
using GamePieces;

namespace PuzzleGrid
{
    public class PuzzleGrid : PlayerInteractableGrid
    {
        

        public override Vector2 ShiftFocusPosition(Vector2 direction)
        {
            Debug.Log("Movement: " + direction);
            return Vector2.zero;
        }

        public override Piece TakeAtFocusPosition() {
            // filler for now
            Piece p = new Piece();
            return p;
        }

        public override Vector2Int PlaceAtFocusPosition(Piece piece) {
            // filler for now
            return new Vector2Int(-1, -1);
        }
    }
}
