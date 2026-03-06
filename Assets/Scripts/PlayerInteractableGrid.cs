using UnityEngine;

public abstract class PlayerInteractableGrid : MonoBehaviour
{

    protected Vector2 position = Vector2.zero;
    protected bool isFocused = false;

    protected abstract void ShiftFocusPosition(Vector2 direction);
}