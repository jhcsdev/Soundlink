
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
                TogglePickupPlace();
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

        private void TogglePickupPlace()
        {
            if (currentGrid is not Inventory.Inventory inv) return;

            Vector2Int pos = new Vector2Int(0, 0);

            if (heldPiece == null)
            {
                // pick up
                heldPiece = inv.takePiece(pos);
                Debug.Log(heldPiece != null ? $"Picked up {heldPiece.name} from {pos}" : $"Nothing to pick up at {pos}");
            }
            else
            {
                // place
                bool placed = inv.putPiece(pos, heldPiece);
                Debug.Log(placed ? $"Placed {heldPiece.name} at {pos}" : $"Could not place at {pos} (occupied?)");

                if (placed) heldPiece = null;
            }
        }
    }
}