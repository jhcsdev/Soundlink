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
        public PieceTileType type; // todo:: make it clearer what the difference between "type" and "tileType" is here.
        public List<GlueCardinality> glue;
        public PieceTileSpriteType tileType;
        public TileSpriteDirection spriteDirection;
    }
}