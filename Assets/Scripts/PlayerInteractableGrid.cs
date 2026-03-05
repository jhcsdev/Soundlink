using UnityEngine;

public abstract class PlayerInteractableGrid : MonoBehaviour
{

    private Vector2 position = Vector2.zero;
    private bool isFocused = false;

    protected virtual void ShiftFocusPosition(Vector2 direction)
    {
        
    }
}