using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    /// The inventory data is entirely controlled by the Inventory class. Inventory Visuals assumes that whatever it is sent is
    /// the absolute source of truth, and it also assumes that following those instructions will not cause visual conflicts 
    /// (which imo is fine). 
    /// 
    /// It is a canvas object, and it is arranged into row objects, which each get piece canvas objects placed as children beneath them. 
    /// When moving left / right, you move between rows; when moving up & down, the entire grid may have to shift in order to bring pieces 
    /// into the screen.
    /// </summary>
    [RequireComponent(typeof(Inventory))]
    public class InventoryVisuals : MonoBehaviour
    {
        #region settable in inspector
        [Header("Visual Configuration")]
        [SerializeField] private int maxRowsDisplayedAtOnce;
        [SerializeField] private int columnsDisplayedAtOnce;
        [Header("Objects")]
        [SerializeField] private GameObject inventoryCanvas;
        [SerializeField] private GameObject focusObject; 
        #endregion

        #region state management 
        private int _currentFocusedRow;
        private int _totalNumberOfRows;
        private List<GameObject> _rowObjects;
        #endregion

        private Inventory inventory;

        public void Awake()
        {
            if (!TryGetComponent(out inventory)) Debug.LogWarning("Missing required Inventory component on " + name);
        }

        void OnEnable()
        {
            inventory.OnInitializingGrid += InitializeWithEmptyRows;
            inventory.OnFocusMovementSuccess += FocusLocationChanged;
            inventory.OnFocusMovementFailure += FocusLocationFailedChange;
            inventory.OnInventoryFocused += FocusGrid;
            inventory.OnInventoryUnfocused += UnfocusGrid;
            inventory.OnPiecePutIntoInventory += PlacePieceInNewSpot;
            inventory.OnPieceTakenOutOfInventory += PieceTakenOut;
        }

        void OnDisable()
        {
            
        }

        #region initialization

        public void InitializeWithEmptyRows(int numPieces)
        {
            
        }

        #endregion

        #region actual visual implementations

        void FocusGrid()
        {
            // Todo - pull to the left and do some sort of visuals
        }

        void UnfocusGrid()
        {
            // todo - fade visuals slightly, push back to right
        }

        void FocusLocationChanged(Vector2Int direction, Vector2Int newFocus)
        {
            // Todo - fucking insane mega chungus code block
            // Thought
        }

        void FocusLocationFailedChange(Vector2Int directionFailed)
        {
            // Thought: Stretch the focus shape in a given direction, color it red for now
        }

        void PlacePieceInNewSpot(Piece piece, Vector2Int spot)
        {
            // Thought: Maximize the piece's canvas representation, and position it in "spot" relative to the inventory spacig
        }

        void PieceTakenOut(Piece piece)
        {
            // Thought: Shrink the piece's canvas representation.
        }

        #endregion
    }
}