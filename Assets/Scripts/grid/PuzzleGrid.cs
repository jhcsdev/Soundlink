using UnityEngine;

namespace PuzzleGrid
{
    public class PuzzleGrid : PlayerInteractableGrid
    {
        

        public override Vector2 ShiftFocusPosition(Vector2 direction)
        {
            Debug.Log("Movement: " + direction);
            return Vector2.zero;
        }
    }
}
