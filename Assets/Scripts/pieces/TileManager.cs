using UnityEngine;

namespace GamePieces
{
    /// <summary>
    /// "builder" for tiles. holds the sprite sheet for different tile types, and creates the tile based on it.
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance;

        [SerializeField] private Sprite basicTileSprite; // todo - assign a set of sprites rather than just default using this one

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public PieceTile CreateTile(PieceTileData data)
        {
            // todo - use data.tileType to choose a proper sprite
            GameObject tile = new("Tile");
            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = basicTileSprite; // todo - change to not use always basic
            PieceTile pt = tile.AddComponent<PieceTile>();
            return pt;
        }
    }
}