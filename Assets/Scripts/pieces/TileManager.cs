using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GamePieces
{
    /// <summary>
    /// "builder" for tiles. holds the sprite sheet for different tile types, and creates the tile based on it.
    /// </summary>
    public class TileManager : MonoBehaviour
    {
        public static TileManager Instance;

        [SerializeField] private List<TileSpriteTypeToSprite> basicTileSprite; 
        [SerializeField] private Sprite verticalSingleGlueSprite;
        private Dictionary<PieceTileSpriteType, Sprite> tileSpriteLookup = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // convert the list of tiletype to sprite pairs to a dictionary for easy lookup
            foreach (var t in basicTileSprite)
            {
                tileSpriteLookup.Add(t.type, t.sprite);
            }
        }

        public PieceTile CreateTile(PieceTileData data)
        {
            GameObject tile = new("Tile");

            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = tileSpriteLookup[data.tileType];

            tile.transform.rotation = Quaternion.Euler(0, 0, GetSpriteRotationDegrees(data.spriteDirection));

            PieceTile pt = tile.AddComponent<PieceTile>();

            return pt.SetSpriteVariable(tileSpriteLookup[data.tileType]).SetTileDirection(data.spriteDirection).CreateGlueSprites(verticalSingleGlueSprite);
        }

        public GameObject CreateCanvasTile(PieceTileData data)
        {
            GameObject canvasTile = new("CanvasTile");

            Image image = canvasTile.AddComponent<Image>();
            image.sprite = tileSpriteLookup[data.tileType];

            canvasTile.transform.rotation = Quaternion.Euler(0, 0, GetSpriteRotationDegrees(data.spriteDirection));

            return canvasTile;
        }
        public PieceTile CreatePieceAndCanvasTile(PieceTileData data, out GameObject canvasTile)
        {
            canvasTile = CreateCanvasTile(data);
            return CreateTile(data);
        }

        public static float GetSpriteRotationDegrees(TileSpriteDirection data) => data switch
        {
            TileSpriteDirection.FACES_RIGHT => -90,
            TileSpriteDirection.FACES_DOWN => 180,
            TileSpriteDirection.FACES_LEFT => 90,
            _ => 0
        };  

        public static TileSpriteDirection RotateCW90(TileSpriteDirection dir) => dir switch
        {
            TileSpriteDirection.FACES_RIGHT => TileSpriteDirection.FACES_DOWN, 
            TileSpriteDirection.FACES_DOWN => TileSpriteDirection.FACES_LEFT, 
            TileSpriteDirection.FACES_LEFT => TileSpriteDirection.FACES_UP, 
            _ => TileSpriteDirection.FACES_RIGHT
        };
        public static TileSpriteDirection RotateCCW90(TileSpriteDirection dir) => dir switch
        {
            TileSpriteDirection.FACES_RIGHT => TileSpriteDirection.FACES_UP, 
            TileSpriteDirection.FACES_DOWN => TileSpriteDirection.FACES_RIGHT, 
            TileSpriteDirection.FACES_LEFT => TileSpriteDirection.FACES_DOWN, 
            _ => TileSpriteDirection.FACES_LEFT
        };
    }

    [Serializable]
    public class TileSpriteTypeToSprite
    {
        public PieceTileSpriteType type;
        public Sprite sprite;
    }
}