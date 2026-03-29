

namespace GamePieces
{
    public enum PieceTileSpriteType
    {
        NO_EDGE, 
        SINGLE_EDGE, // single edge is on the top when FACES_UP
        DOUBLE_OPPOSING_EDGE, // Edge on top and edge on bottom when FACES_UP and FACES_DOWN
        DOUBLE_CORNER_EDGE, // Edge on top and edge on right when FACES_UP
        TRIPLE_EDGE, // Edge on all sides but down when FACES_UP
        ALL_EDGE, // Edge on all sides
    }

    public enum TileSpriteDirection // similar to cardinality, separated into own enum just to make things a little clearer
    {
        FACES_UP, 
        FACES_RIGHT, 
        FACES_DOWN, 
        FACES_LEFT
    }
}