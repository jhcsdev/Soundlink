using GamePieces;
using UnityEngine;

public abstract class PlayerInteractableGrid : MonoBehaviour
{

    protected Vector2Int focusPosition = Vector2Int.zero;
    protected bool isFocused = false;

    /// <summary>
    /// shifts the grid's focus in a given direction
    /// </summary>
    /// <param name="direction">the direction to shift in -- probably unit vector?</param>
    /// <returns>Vector2.zero if you are still within the grid, and a direction of which edge you are leaving from if you are at the edge</returns>
    public abstract Vector2 ShiftFocusPosition(Vector2Int direction);
    /// <summary>
    /// select the object
    /// </summary>
    /// <returns>the piece you select (or nothing if there was nothing there)</returns>
    public abstract Piece TakeAtFocusPosition();
    /// <summary>
    /// places given piece at current focus position
    /// </summary>
    /// <param name="p">the piece</param>
    /// <returns>true if successful, false if not</returns>
    public abstract Vector2Int? PlaceAtFocusPosition(Piece p);


    public virtual void FocusGrid() => isFocused = true;
    public virtual void UnfocusGrid() => isFocused = false;
}