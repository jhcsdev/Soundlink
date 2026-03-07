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
        }

        // given Piece, put it in next available location or something like that? 
        public bool PutPiece(Vector2Int position, Piece piece) {
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
        public Piece takePiece(Vector2Int location) {
            Piece piece = null;
            piecesLookup.TryGetValue(location, out piece);
            return piece;
        }

        public override Vector2 ShiftFocusPosition(Vector2 direction)
        {
            return Vector2.zero;
        }
    }
}
