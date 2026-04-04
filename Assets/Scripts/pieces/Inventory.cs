using GamePieces;
using UnityEngine;
using System.Collections.Generic;

namespace Inventory
{
    public class Inventory : PlayerInteractableGrid
    {
        [SerializeField] private InventoryData inventoryData;
        [SerializeField] private float inventorySpacing;
        [SerializeField] private GameObject focusVisualObject; // todo: a temporary visual for now, helping to see selection positon
        private Color focusVisualInactiveColor = new(0.9f, 0.2f, 0.9f, 0.1f);
        private Color focusVisualActiveColor = new(0.9f, 0.2f, 0.9f, 0.4f);
        private Dictionary<Vector2Int, Piece> piecesLookup;

        void Awake()
        {
            if (inventoryData == null) {
                Debug.Log("Error in Awake(): Inventory not assigned!");
                return;
            }

            piecesLookup = new Dictionary<Vector2Int, Piece>();

            int pieceNumber = 0;
            foreach (var pieceData in inventoryData.pieces) {
                GameObject pieceObj = new($"Piece {pieceNumber++}");
                Piece pieceObjComp = pieceObj.AddComponent<Piece>();

                pieceObjComp.Initialize(pieceData);
                pieceObjComp.InventoryMode();
                pieceObjComp.transform.parent = transform;

                Vector2Int location = putPiece(pieceObjComp);
            }

            PrintInventory();
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
            Vector2Int maxPosition = new(-1,0);
            
            // iterate through all pieces in inventory, return first empty location
            foreach (KeyValuePair<Vector2Int, Piece> kvp in piecesLookup)
            {
                Vector2Int position = kvp.Key;
                Piece piece = kvp.Value;

                if (piece == null) {
                    return position;
                }

                if (isMaxPosition(position, maxPosition)) {
                    maxPosition = position;
                }
            }
            
            // if there are no empty locations, need to append to inventory's dictionary
            // however, also need to check if we are currently at end of row 
            // if end of row, create a new row object
            // otherwise: just place in next new space
            if (maxPosition.x == inventoryData.rowSize - 1)
            {
                return new Vector2Int(0, maxPosition.y + 1);
            }
            else
            {
                return new Vector2Int(maxPosition.x + 1, maxPosition.y);
            }
        }

        // put Piece in next available position
        public Vector2Int putPiece(Piece piece) {
            if (piece == null) {
                Debug.Log("Error in PutPiece(): Piece cannot be null");
                return new Vector2Int(-1, -1);
            }

            Vector2Int emptyLocation = FindOpenPosition();
            piecesLookup[emptyLocation] = piece;
            Vector2 shiftedLoc = new(emptyLocation.x * inventorySpacing, emptyLocation.y * inventorySpacing);
            piece.transform.localPosition = shiftedLoc;
            return emptyLocation;
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
        
        // TODO: currently allows movement outside of bounds bc it doesn't know what the max inventory position is, we probably should store that somewhere
        public override Vector2Int ShiftFocusPosition(Vector2Int direction)
        {
            if (direction == Vector2.zero) return Vector2Int.zero;

            Vector2Int intended = focusPosition + direction;

            // check y down - are we going back to the grid?
            if (intended.y < 0) { return Vector2Int.up; }

            // otherwise movement is ok
            focusPosition = intended;

            focusVisualObject.transform.localPosition = Vec2IntToNormal(focusPosition) * inventorySpacing;

            return Vector2Int.zero;
        }

        public override void FocusGrid()
        {
            base.FocusGrid();
            focusVisualObject.GetComponent<SpriteRenderer>().color = focusVisualActiveColor;
        }
        public override void UnfocusGrid()
        {
            base.UnfocusGrid();
            focusVisualObject.GetComponent<SpriteRenderer>().color = focusVisualInactiveColor;
        }

        public override Piece TakeAtFocusPosition() {
            Debug.Log("Attempting to take piece");
            return takePiece(focusPosition);
        }

        public override Vector2Int? PlaceAtFocusPosition(Piece piece) {
            return putPiece(piece);
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

        private Vector2 Vec2IntToNormal(Vector2Int v) => new(v.x, v.y);
    }
}
