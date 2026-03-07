using GamePieces;
using UnityEngine;
using System.Collections.Generic;

namespace Inventory
{
    public class Inventory : PlayerInteractableGrid
    {
        [SerializeField] private InventoryData inventoryData;
        private Dictionary<Vector2Int, Piece> piecesLookup;

        // NOTE: these are jonathan's original comments ... could prob remove at some point
        // todo - foreach Piece p in inventoryData.pieces: 
        // GameObject clone = Instantiate(p.gameObject)
        // add to internal inventory data structures -- might just be able to call PutPiece()

        void Awake()
        {
            if (inventoryData == null) {
                Debug.Log("Error in Awake(): Inventory not assigned!");
                return;
            }

            piecesLookup = new Dictionary<Vector2Int, Piece>();

            foreach (var square in inventoryData.startingSquares) {
                if (square.piece == null) {
                    continue;
                }

                piecesLookup[square.position] = square.piece;
            }

            PrintInventory();
        }

        // given Piece, put it in next available location or something like that? 
        public bool putPiece(Vector2Int position, Piece piece) {
            if (piece == null) {
                Debug.Log("Error in PutPiece(): Piece cannot be null");
                return false;
            }

            if (piecesLookup.ContainsKey(position)) {
                Debug.Log("Error in PutPiece(): Location already taken");
                return false;
            }

            piecesLookup[position] = piece;
            return true;
        }

        // given location, take a Piece from the board
        public Piece takePiece(Vector2Int location)
        {
            if (piecesLookup.TryGetValue(location, out Piece piece))
            {
                piecesLookup.Remove(location);
                return piece;
            }

            return null;
        }
        
        public override Vector2 ShiftFocusPosition(Vector2 direction)
        {
            return Vector2.zero;
        }

        public void PrintInventory()
        {
            if (piecesLookup == null)
            {
                Debug.Log("Inventory not initialized.");
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
