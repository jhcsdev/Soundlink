using UnityEngine;

public abstract class PlayerInteractableGrid : MonoBehaviour
{

    protected Vector2 position = Vector2.zero;
    protected bool isFocused = false;

    /// <summary>
    /// shifts the grid's focus in a given direction
    /// </summary>
    /// <param name="direction">the direction to shift in -- probably unit vector?</param>
    /// <returns>Vector2.zero if you are still within the grid, and a direction of which edge you are leaving from if you are at the edge</returns>
    public abstract Vector2 ShiftFocusPosition(Vector2 direction);
}