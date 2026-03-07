using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "InventoryData", menuName = "Gameplay/Inventory Data")]
    public class InventoryData : ScriptableObject
    {
        // NOTE: ChatGPT told me this was a good architecture but I want to hear your opinion
        [SerializeField] private List<InventorySquare> inventorySquares = new();
        public IReadOnlyList<InventorySquare> startingSquares => inventorySquares;

        // how many pieces should be displayed on each row of the inventory
        public int rowSize = 6;
    }

    [System.Serializable]
    public struct InventorySquare
    {
        public Vector2Int position;
        public Piece piece;
    }
}
