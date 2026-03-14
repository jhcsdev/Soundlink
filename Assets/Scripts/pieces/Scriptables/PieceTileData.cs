using System;
using UnityEngine;

namespace GamePieces
{
    [Serializable]
    public class PieceTileData
    {
        public Vector2Int relativeOffset;
        public bool isOrigin;
        public PieceTileType type;
    }
}