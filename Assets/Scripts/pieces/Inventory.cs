using GamePieces;
using Unity.VisualScripting;
using UnityEngine;

namespace Inventory
{
    public class Inventory : PlayerInteractableGrid
    {
        [SerializeField] private InventoryData inventoryData;

        void Awake()
        {
            // todo - foreach Piece p in inventoryData.pieces: 
            // GameObject clone = Instantiate(p.gameObject)
            // add to internal inventory data structures -- might just be able to call PutPiece()
        }

        public override Vector2 ShiftFocusPosition(Vector2Int direction)
        {
            return Vector2.zero;
        }

        public override Vector2Int? PlaceAtFocusPosition(Piece p)
        {
            throw new System.NotImplementedException();
        }
        public override Piece TakeAtFocusPosition()
        {
            throw new System.NotImplementedException();
        }
    }
}
