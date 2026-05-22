using System;
using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace Inventory
{
    [CreateAssetMenu(fileName = "AutoPlaceData", menuName = "Gameplay/Autoplace Data")]
    public class AutoPlaceData : ScriptableObject
    {
        public List<PieceAutoPlacement> autoPlacements;
    }
    [Serializable]
    public class PieceAutoPlacement
    {
        public PieceData data;
        public Vector2 where;
        public int clockwiseRotations;
    }
}