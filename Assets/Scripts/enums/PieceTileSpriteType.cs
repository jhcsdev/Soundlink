

namespace GamePieces
{
    public enum PieceTileSpriteType
    {
        NO_EDGE_NO_CORNER, // for when all eight surrounding coordinates have a block
        NO_EDGE_ONE_CORNER, // for when there is no block at -1, -1 relative
        NO_EDGE_OPPOSITE_CORNER, // for when there is no block at -1, -1 and 1, 1 relative
        NO_EDGE_ADJACENT_CORNERS, // for when there is no block at -1, -1 and 1, -1 relative
        NO_EDGE_THREE_CORNERS, // for when there is no block at -1, -1, and -1, 1 and 1, -1
        NO_EDGE_FOUR_CORNERS, // for when there is no block at -1, -1 and -1, 1 and 1, 1 and 1, -1 
        T_SECTION_NO_CORNERS, // for when all five surrounding coordinates have a block
        T_SECTION_CORNER_BOTTOM_LEFT, // for when there is no block at -1, -1 relative
        T_SECTION_CORNER_BOTTOM_RIGHT, // for when there is no block at 1, -1 relative,
        T_SECTION_BOTH_CORNERS, // for when there is no block at -1, -1 and 1, -1 relative,
        OPPOSITE_EDGE, // when FACES_UP, top and bottom edges are filled
        ADJACENT_EDGE_NO_CORNER, // when FACES_UP, top and right edges are filled
        ADJACENT_EDGE_CORNER,
        ALL_EDGE,
        TRIPLE_EDGE, // when FACES_UP, bottom edge not filled
    }

    public enum TileSpriteDirection // similar to cardinality, separated into own enum just to make things a little clearer
    {
        FACES_UP, 
        FACES_RIGHT, 
        FACES_DOWN, 
        FACES_LEFT
    }
}