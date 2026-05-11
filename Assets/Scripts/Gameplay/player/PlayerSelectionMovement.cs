
using System;
using GamePieces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player
{
    [RequireComponent(typeof(PlayerInputWrapper))]
    public class PlayerSelectionMovement : MonoBehaviour
    {
        [Header("grids")]
        [SerializeField] private PlayerInteractableGrid inventoryGrid;
        [SerializeField] private PlayerInteractableGrid puzzleGrid;
        private PlayerInteractableGrid currentGrid;

        private bool IsCurrentGridInventory() => currentGrid == inventoryGrid;
        private bool IsCurrentGridPuzzleGrid() => currentGrid == puzzleGrid;
        private void SwapGrid() => SetCurrentGrid(IsCurrentGridInventory() ? puzzleGrid : inventoryGrid); 

        [Header("input related")]
        [SerializeField, Tooltip("Move input acts like a button. If you hold, it starts rapidly moving.")] 
        private float holdTimeBeforeRapidMove = 0.5f;
        [SerializeField, Tooltip("How quickly focus moves when holding input.")] 
        private float rapidMoveInterval = 0.1f;
        
        private PlayerInputWrapper playerInputWrapper;
        private Vector2 _directionCurrent = Vector2.zero;
        private float _allowNextRapidMoveAt = Mathf.Infinity; 

        private Piece referencedPiece = null;

        private void Awake()
        {
            playerInputWrapper = GetComponent<PlayerInputWrapper>();
        }

        void Start()
        {
            playerInputWrapper.SELECT.performed += OnSelect;
            playerInputWrapper.INVENTORY_BUTTON.performed += OnInventoryButton;
            playerInputWrapper.ESCAPE.performed += OnEscape;
            playerInputWrapper.GRID.performed += OnGrid;
            playerInputWrapper.ROTATE.performed += OnRotate;
            SetCurrentGrid(inventoryGrid);
        }

        private void Update()
        {
            if (currentGrid == null) return;

            Vector2 inputDirectionFloat = playerInputWrapper.MOVE.ReadValue<Vector2>();
            // rn the below just omits diagonal direction
            Vector2Int inputDirection = new(Mathf.RoundToInt(inputDirectionFloat.x), Mathf.RoundToInt(inputDirectionFloat.y));

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
                if (referencedPiece != null) currentGrid.Hover(referencedPiece);
            } 
            else if (curTime > _allowNextRapidMoveAt) // otherwise, check if we are rapid moving
            {
                _allowNextRapidMoveAt = curTime + rapidMoveInterval;
                movement = currentGrid.ShiftFocusPosition(inputDirection);
                if (referencedPiece != null) currentGrid.Hover(referencedPiece);
            }

            if (movement != Vector2.zero) SwapGrid();
        }

        public void SetCurrentGrid(PlayerInteractableGrid grid)
        {
            if (grid == null) return;
            if (currentGrid != null) currentGrid.UnfocusGrid();
            currentGrid = grid;
            currentGrid.FocusGrid(referencedPiece != null);
        }

        /// <summary>
        /// if in inventory - grabs a piece at your focus position
        /// if in grid - 
        ///     if you have a piece selected - place that piece
        ///     if not - try to grab the piece at your current spot
        /// </summary>
        /// <param name="ctx"></param>
        private void OnSelect(InputAction.CallbackContext ctx)
        {   
            if (IsCurrentGridInventory()) // grabbing piece from inventory
            {
                referencedPiece = currentGrid.TakeAtFocusPosition();

                SwapGrid();

                if (referencedPiece != null) currentGrid.Hover(referencedPiece);

                return;
            }

            if (!IsCurrentGridPuzzleGrid()) return;

            if (referencedPiece == null) // grabbing piece from grid; stay in grid, i think?
            {
                referencedPiece = currentGrid.TakeAtFocusPosition();
            }
            else // you are holding a piece and are not in the inventory; place the piece
            {
                Vector2? placedAt = currentGrid.PlaceAtFocusPosition(referencedPiece);
                if (placedAt != null)
                {
                    referencedPiece = null; // reset referenced piece; grid owns that now
                }
            }
        }

        /// <summary>
        /// only works while in the grid - instantly returns the piece you are holding to the grid, or returns the piece at your focus position
        /// </summary>
        /// <param name="ctx"></param>
        private void OnInventoryButton(InputAction.CallbackContext ctx)
        {   
            if (!IsCurrentGridPuzzleGrid()) return;

            if (referencedPiece != null) 
            {
                inventoryGrid.PlaceAtFocusPosition(referencedPiece);
                referencedPiece = null;
            }

            SwapGrid();
        }

        private void OnGrid(InputAction.CallbackContext ctx)
        {
            if (!IsCurrentGridInventory()) return;

            SwapGrid(); 
        }

        private void OnEscape(InputAction.CallbackContext ctx)
        {
            // todo:: pause screen
        }

        private void OnRotate(InputAction.CallbackContext ctx)
        {
            if (referencedPiece == null) return;

            referencedPiece.RotatePieceClockwise();
            currentGrid.Hover(referencedPiece);
        }
    }
}