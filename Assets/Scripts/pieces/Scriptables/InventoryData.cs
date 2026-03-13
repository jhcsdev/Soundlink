using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "InventoryData", menuName = "Gameplay/Inventory Data")]
    public class InventoryData : ScriptableObject
    {
        // NOTE: ChatGPT told me this was a good architecture but I want to hear your opinion
        // [SerializeField] private List<Piece> pieces = new();
        public List<Piece> pieces = new();

        // how many pieces should be displayed on each row of the inventory
        public int rowSize = 6;
    }
}
