using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "InventoryData", menuName = "Gameplay/Inventory Data")]
    public class InventoryData : ScriptableObject
    {
        public List<PieceData> pieces = new();

        // how many pieces should be displayed on each row of the inventory
        public int rowSize = 6;
    }
}
