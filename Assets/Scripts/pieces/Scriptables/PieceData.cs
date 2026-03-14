using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    [CreateAssetMenu(fileName = "PieceData", menuName = "Gameplay/Piece Data")]
    public class PieceData : ScriptableObject
    {
        public List<PieceTileData> tiles = new();
    }
}