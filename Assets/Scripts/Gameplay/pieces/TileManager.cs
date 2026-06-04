using System;
using System.Collections.Generic;
using NUnit.Framework;
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
        [SerializeField] private List<TileSpriteTypeToSprite> silentTileSprites;
        [SerializeField] private Sprite verticalSingleGlueSprite; 
        [SerializeField] private Sprite passthroughSprite; 
        [SerializeField] private Material pieceMaterial;
        private Dictionary<PieceTileSpriteType, Sprite> tileSpriteLookup = new();
        private Dictionary<PieceTileSpriteType, Sprite> silentSpriteLookup = new();

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);

            // convert the list of tiletype to sprite pairs to a dictionary for easy lookup
            foreach (var t in basicTileSprite)
            {
                tileSpriteLookup.Add(t.type, t.sprite);
            }
            foreach (var t in silentTileSprites)
            {
                silentSpriteLookup.Add(t.type, t.sprite);
            }
        }

        public PieceTile CreateTile(PieceTileData data, bool isSilent)
        {
            GameObject tile = new("Tile");

            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            if (data.type == PieceTileType.PASSTHROUGH)
            {
                sr.sprite = passthroughSprite;
                sr.sortingOrder = (int)SPRITE_ORDER.PIECE_TILE_PASSTHROUGH_SPRITE_INDEX_HOVER_GRID;
            } else
            {
                sr.sprite = isSilent ? silentSpriteLookup[data.tileType] : tileSpriteLookup[data.tileType];
                sr.sortingOrder = (int)SPRITE_ORDER.PIECE_TILE_SPRITE_INDEX_HOVER_GRID;
            }

            tile.transform.rotation = Quaternion.Euler(0, 0, GetSpriteRotationDegrees(data.spriteDirection));

            PieceTile pt = tile.AddComponent<PieceTile>();

            return pt.SetSpriteVariable(tileSpriteLookup[data.tileType]).SetTileDirection(data.spriteDirection).SetGlueSprite(verticalSingleGlueSprite);
        }

        public GameObject CreateCanvasTile(PieceTileData data, bool isSilent)
        {
            GameObject canvasTile = new("CanvasTile");

            Image image = canvasTile.AddComponent<Image>();
            if (data.type == PieceTileType.PASSTHROUGH)
            {
                image.sprite = passthroughSprite;
            } 
            else
            {
                image.sprite = isSilent ? silentSpriteLookup[data.tileType] : tileSpriteLookup[data.tileType];
            }

            foreach (var glueCardinality in data.glue)
            {
                GameObject glueObj = new($"CanvasGlue{glueCardinality}");
                glueObj.transform.SetParent(canvasTile.transform);
                glueObj.transform.localPosition = Vector3.zero;

                Image sr = glueObj.AddComponent<Image>();
                sr.sprite = verticalSingleGlueSprite;
                glueObj.transform.localRotation = Quaternion.Euler(0, 0, GetGlueRotationDegrees(glueCardinality) - GetSpriteRotationDegrees(data.spriteDirection));
            }

            canvasTile.transform.rotation = Quaternion.Euler(0, 0, GetSpriteRotationDegrees(data.spriteDirection));

            return canvasTile;
        }
        public PieceTile CreatePieceAndCanvasTile(PieceTileData data, out GameObject canvasTile, bool isSilent = false)
        {
            canvasTile = CreateCanvasTile(data, isSilent);
            return CreateTile(data, isSilent);
        }

        public static float GetSpriteRotationDegrees(TileSpriteDirection data) => data switch
        {
            TileSpriteDirection.FACES_RIGHT => -90,
            TileSpriteDirection.FACES_DOWN => 180,
            TileSpriteDirection.FACES_LEFT => 90,
            _ => 0
        };  
        public static float GetGlueRotationDegrees(GlueCardinality glue) => glue switch
        {
            GlueCardinality.EAST => -90,
            GlueCardinality.SOUTH => 180,
            GlueCardinality.WEST => 90,
            _ => 0
        };

        public Material GetPieceMaterial() { return pieceMaterial; }

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