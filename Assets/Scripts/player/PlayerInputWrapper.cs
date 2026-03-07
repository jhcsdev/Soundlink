

using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    public class PlayerInputWrapper : MonoBehaviour
    {
        private PlayerInputActions playerInput;
        public InputAction SELECT, INVENTORY_BUTTON, MAP, LEFT_CLICK, MOVE, ROTATE;

        private void Awake()
        {
            playerInput = new();
            SELECT = playerInput.Player.Select;
            INVENTORY_BUTTON = playerInput.Player.ReturnToinventory;
            MAP = playerInput.Player.Map;
            LEFT_CLICK = playerInput.Player.LeftClick;
            MOVE = playerInput.Player.Move;
            ROTATE = playerInput.Player.Rotate;
        }

        private void OnEnable()
        {
            playerInput.Enable();
        }
        private void OnDisable()
        {
            playerInput.Disable();
        }
    }
}