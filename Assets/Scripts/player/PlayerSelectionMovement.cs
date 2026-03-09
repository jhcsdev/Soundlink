
using UnityEngine;

namespace Player
{
    [RequireComponent(typeof(PlayerInputWrapper))]
    public class PlayerSelectionMovement : MonoBehaviour
    {
        [Header("grids")]
        [SerializeField] private PlayerInteractableGrid inventoryGrid;
        [SerializeField] private PlayerInteractableGrid gameGrid;
        private PlayerInteractableGrid currentGrid;

        [Header("input related")]
        [SerializeField, Tooltip("Move input acts like a button. If you hold, it starts rapidly moving.")] 
        private float holdTimeBeforeRapidMove = 0.5f;
        [SerializeField, Tooltip("How quickly focus moves when holding input.")] 
        private float rapidMoveInterval = 0.1f;
        
        private PlayerInputWrapper playerInputWrapper;
        private Vector2 _directionCurrent = Vector2.zero;
        private float _allowNextRapidMoveAt = Mathf.Infinity; 

        private GamePieces.Piece heldPiece;


        private void Awake()
        {
            playerInputWrapper = GetComponent<PlayerInputWrapper>();
        }

        void Start()
        {
            SetCurrentGrid(inventoryGrid);
        }

        private void Update()
        {

            if (playerInputWrapper.INVENTORY_BUTTON.WasPressedThisFrame()) {
                Debug.Log("INVENTORY PRESSED");
                // TODO: add something for the inventory to work
            }

            if (currentGrid == null) return;

            Vector2 inputDirection = playerInputWrapper.MOVE.ReadValue<Vector2>();

            if (inputDirection == Vector2.zero)
            {
                _directionCurrent = Vector2.zero;
                _allowNextRapidMoveAt = Mathf.Infinity;
                return;
            } 

            Vector2 movement = Vector2.zero;
            float curTime = Time.time;

            // if we have just started moving in a new direction, set the time for rapid movement start, and move once
            if (inputDirection != _directionCurrent) 
            {
                _directionCurrent = inputDirection;
                _allowNextRapidMoveAt = curTime + holdTimeBeforeRapidMove;
                movement = currentGrid.ShiftFocusPosition(inputDirection);
            } 
            else if (curTime > _allowNextRapidMoveAt) // otherwise, check if we are rapid moving
            {
                _allowNextRapidMoveAt = curTime + rapidMoveInterval;
                movement = currentGrid.ShiftFocusPosition(inputDirection);
            }

            if (movement != Vector2.zero) SetCurrentGrid(currentGrid == inventoryGrid ? gameGrid : inventoryGrid);
        }

        public void SetCurrentGrid(PlayerInteractableGrid grid)
        {
            currentGrid = grid;
        }
    }
}