using GamePieces;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

namespace Inventory
{
    [RequireComponent(typeof(InventoryVisuals))]
    public class Inventory : PlayerInteractableGrid
    {
        #region visual events
        public UnityAction<int /*number of pieces that are going to be put into the inventory*/> OnInitializingGrid;
        public UnityAction<Vector2Int /*focusPosition*/> OnInventoryFocused;
        public UnityAction OnInventoryUnfocused;
        public UnityAction<Vector2Int /*direction*/, Vector2Int /*new focus position*/> OnFocusMovementSuccess;
        public UnityAction<Vector2Int /*direction*/> OnFocusMovementFailure;
        public UnityAction<Piece> OnPieceTakenOutOfInventory; 
        public UnityAction<Piece, Vector2Int /*position of placement*/> OnPiecePutIntoInventory;
        #endregion

        private InventoryData inventoryData;
        private Dictionary<Vector2Int, Piece> piecesLookup;

        void OnEnable()
        {
            if (LevelLoader.instance == null) Debug.LogError("Warning: A LevelLoader needs to exist in the scene for inventory to build.");
            if (LevelLoader.instance.GetInventoryData() == null) Debug.LogError("Warning: LevelLoader exists, but inventory failed to retrieve InventoryData.");

            inventoryData = LevelLoader.instance.GetInventoryData();
            piecesLookup = new Dictionary<Vector2Int, Piece>();
        }

        void Start()
        {
            int pieceNumber = 0;
            foreach (var pieceData in inventoryData.pieces) {
                GameObject pieceObj = new($"Piece {pieceNumber++}");
                Piece pieceObjComp = pieceObj.AddComponent<Piece>();

                pieceObjComp.Initialize(pieceData);
                pieceObjComp.transform.position = Vector3.one * 150;
                pieceObjComp.transform.parent = transform;

                putPiece(pieceObjComp);
            }
        }

        // check if current position is the max position, or something like that
        public bool isMaxPosition(Vector2Int currentPosition, Vector2Int maxPosition) {
            if (currentPosition.y > maxPosition.y) {
                return true;
            }
            else if (currentPosition.y < maxPosition.y) {
                return false;
            }
            else {
                return (currentPosition.x > maxPosition.x);
            }
        }

        // find next open position in inventory
        // NOTE: we should ALWAYS be able to find an open position and add to inventory
        public Vector2Int FindOpenPosition() {
            Vector2Int maxPosition = new(-1, 0);

            foreach (KeyValuePair<Vector2Int, Piece> kvp in piecesLookup)
            {
                if (isMaxPosition(kvp.Key, maxPosition))
                    maxPosition = kvp.Key;
            }

            // Scan in order from (0,0) to maxPosition, looking for gaps
            for (int y = 0; y <= maxPosition.y; y++)
            {
                for (int x = 0; x < inventoryData.rowSize; x++)
                {
                    Vector2Int candidate = new(x, y);
                    if (!piecesLookup.TryGetValue(candidate, out Piece piece) || piece == null)
                        return candidate;

                    if (candidate == maxPosition) break;
                }
            }

            // No gaps found, append after max
            if (maxPosition.x == inventoryData.rowSize - 1)
                return new Vector2Int(0, maxPosition.y + 1);
            else
                return new Vector2Int(maxPosition.x + 1, maxPosition.y);
        }

        // put Piece in next available position
        public Vector2Int putPiece(Piece piece) {
            if (piece == null) {
                Debug.Log("Error in PutPiece(): Piece cannot be null");
                return new Vector2Int(-1, -1);
            }

            Vector2Int emptyLocation = FindOpenPosition();
            piecesLookup[emptyLocation] = piece;

            OnPiecePutIntoInventory?.Invoke(piece, emptyLocation);
            piece.InventoryMode();

            return emptyLocation;
        }

        public override Vector2Int ShiftFocusPosition(Vector2Int direction)
        {
            if (direction == Vector2.zero) return Vector2Int.zero;
            direction.y *= -1;

            int maxY = 0;
            foreach (var key in piecesLookup.Keys) if (key.y > maxY) maxY = key.y;

            Vector2Int candidate = focusPosition + direction;

            if (candidate.x < 0 || candidate.x >= inventoryData.rowSize || candidate.y < 0 || candidate.y > maxY)
            {
                OnFocusMovementFailure?.Invoke(direction);
                return Vector2Int.zero;
            }

            focusPosition = candidate;
            OnFocusMovementSuccess?.Invoke(direction, focusPosition);
            return Vector2Int.zero;
        }

        public override void FocusGrid(bool isHoldingPiece=false)
        {
            base.FocusGrid();
            OnInventoryFocused?.Invoke(focusPosition);
        }
        public override void UnfocusGrid()
        {
            base.UnfocusGrid();
            OnInventoryUnfocused?.Invoke();
        }

        /// <summary>
        /// try to take piece at focus position
        /// </summary>
        /// <returns>piece if there is a piece at the focus position, nothing otherwise</returns>
        public override Piece TakeAtFocusPosition() {
            if (piecesLookup.TryGetValue(focusPosition, out Piece piece))
            {
                piecesLookup.Remove(focusPosition);
                OnPieceTakenOutOfInventory?.Invoke(piece);
                return piece;
            }

            return null;
        }

        public override Vector2Int? PlaceAtFocusPosition(Piece piece) {
            return putPiece(piece);
        }

        public void PrintInventory()
        {
            if (piecesLookup == null)
            {
                Debug.Log("Error: inventory not initialized.");
                return;
            }

            if (piecesLookup.Count == 0)
            {
                Debug.Log("Inventory is empty.");
                return;
            }

            Debug.Log("=== Inventory Contents ===");

            foreach (var kvp in piecesLookup)
            {
                Vector2Int position = kvp.Key;
                Piece piece = kvp.Value;

                string pieceName = piece != null ? piece.name : "NULL";

                Debug.Log($"Position: {position} → Piece: {pieceName}");
            }
        }
    }
}
