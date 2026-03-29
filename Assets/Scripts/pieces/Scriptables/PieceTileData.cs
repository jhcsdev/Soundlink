using System;
using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    [Serializable]
    public class PieceTileData
    {
        public Vector2Int relativeOffset;
        public bool isOrigin;
        public PieceTileType type;
        public List<GlueCardinality> glue;
        public PieceTileSpriteType tileType;
        public TileSpriteDirection spriteDirection;
    }
}