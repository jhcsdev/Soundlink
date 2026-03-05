using GamePieces;
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
    }
}
