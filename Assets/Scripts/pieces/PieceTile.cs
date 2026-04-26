using System.Collections.Generic;
using PuzzleGrid;
using UnityEngine;
using UnityEngine.Events;

namespace GamePieces
{
    public class PieceTile : MonoBehaviour
    {
        public UnityAction<GridTile> OnPlaced;
        public UnityAction OnHover;
        public UnityAction<Piece, Vector2> OnPickedUp;
        public UnityAction<Color, float> OnColorPulse;
        public UnityAction<Color, float> OnSetColor;

        [SerializeField] private PieceTileType type;
        private Sprite tileSprite; // todo:: only supports tile right now, no overlay
        private Sprite glueSprite; // todo:: see CreateGlueSprites()
        private TileSpriteDirection spriteDirection;
        private Piece piece;
        private Vector2Int relativeOffset;
        private List<GlueCardinality> glue;

        private PieceTileData ptd;
        private bool initialized = false;

        #region chaining functions
        public PieceTile Initialize(PieceTileData data, Piece parent) // returns self for chaining purposes
        {
            ptd = data;
            relativeOffset = ptd.relativeOffset;
            piece = parent;
            type = ptd.type;
            initialized = true;
            glue = ptd.glue;

            gameObject.AddComponent<PieceTileVisuals>();

            return this;
        }
        private bool IsInitialized() { if (initialized) return true; Debug.Log(name + " not initialized!"); return false; }
        public PieceTile SetParent(Transform to)
        {
            if (IsInitialized()) transform.parent = to;
            return this;
        }
        public PieceTile SetLocalPosition(Vector2 to)
        {
            if (IsInitialized()) transform.localPosition = to; 
            return this;
        }
        public PieceTile SetLocalPositionAndScaleByTileSize(float tileSize)
        {
            if (IsInitialized()) {
                transform.localScale = Vector2.one * tileSize;
                transform.localPosition = new Vector2(relativeOffset.x, relativeOffset.y) * tileSize;
            }
            return this;
        }
        public PieceTile CreateGlueSprites()
        {
            // todo:: questionable if these spriterenderers should really be put here...?
            if (glueSprite == null) { Debug.LogError("Called CreateGlueSprites but glueSprite is null"); return this; }

            float baseRotation = TileManager.GetSpriteRotationDegrees(spriteDirection);

            foreach (var glueCardinality in glue)
            {
                GameObject glueObj = new($"Glue{glueCardinality}");
                glueObj.transform.SetParent(transform);
                glueObj.transform.localPosition = Vector3.zero;

                SpriteRenderer sr = glueObj.AddComponent<SpriteRenderer>();
                sr.sprite = glueSprite;
                sr.sortingOrder = (int)SPRITE_ORDER.PIECE_GLUE_SPRITE_INDEX;
                glueObj.transform.rotation = Quaternion.Euler(0, 0, TileManager.GetGlueRotationDegrees(glueCardinality));
            }
            return this;
        }
        public PieceTile SetPiece(Piece p) { piece = p; return this; }
        public PieceTile SetRelativeOffset(Vector2Int vint) { relativeOffset = vint; return this; }
        public PieceTile SetSpriteVariable(Sprite sprite) { tileSprite = sprite; return this; }
        public PieceTile SetTileDirection(TileSpriteDirection direction)  { spriteDirection = direction; return this; }
        public PieceTile SetGlueSprite(Sprite sprite) { glueSprite = sprite; return this; }
        #endregion

        #region getters
        public List<GlueCardinality> GetGlue => glue;
        public PieceTileType GetTileType() => type;
        public Piece GetPiece() => piece;
        public Vector2Int GetUnrotatedRelativeOffset() => ptd.relativeOffset;
        public Vector2Int GetRotatedRelativeOffset() => relativeOffset;
        public Sprite GetSprite() => tileSprite;
        public TileSpriteDirection GetSpriteDirection() => spriteDirection;
        #endregion

        public void RotateRelativeOffsetClockwise()
        {
            int x = relativeOffset.y * 1;
            int y = relativeOffset.x * -1;
            relativeOffset = new(x, y);
            SetLocalPosition(relativeOffset);
            spriteDirection = TileManager.RotateCW90(spriteDirection);
            transform.rotation = Quaternion.Euler(0,0,TileManager.GetSpriteRotationDegrees(spriteDirection));

            List<GlueCardinality> glueDirs = new();
            foreach(var cardinality in glue)
            {
                glueDirs.Add(RotateCardinalityCW90(cardinality));
            }
            glue = glueDirs;
        }
        public void RotateRelativeOffsetCounterClockwise()
        {
            int x = relativeOffset.y * -1;
            int y = relativeOffset.x * 1;
            relativeOffset = new(x, y);
            SetLocalPosition(relativeOffset);
            spriteDirection = TileManager.RotateCCW90(spriteDirection);
            transform.rotation = Quaternion.Euler(0,0,TileManager.GetSpriteRotationDegrees(spriteDirection));

            List<GlueCardinality> glueDirs = new();
            foreach(var cardinality in glue)
            {
                glueDirs.Add(RotateCardinalityCCW90(cardinality));
            }
            glue = glueDirs;
        }

        private GlueCardinality RotateCardinalityCW90(GlueCardinality cardinality) => cardinality switch
        {
            GlueCardinality.NORTH => GlueCardinality.EAST, 
            GlueCardinality.EAST => GlueCardinality.SOUTH, 
            GlueCardinality.SOUTH => GlueCardinality.WEST, 
            _ => GlueCardinality.NORTH
        };
        private GlueCardinality RotateCardinalityCCW90(GlueCardinality cardinality) => cardinality switch
        {
            GlueCardinality.SOUTH => GlueCardinality.EAST, 
            GlueCardinality.EAST => GlueCardinality.NORTH, 
            GlueCardinality.NORTH => GlueCardinality.WEST, 
            _ => GlueCardinality.SOUTH
        };

        public void Placed(GridTile tile)
        {
            OnPlaced?.Invoke(tile);
        }
        public void Hovered()
        {
            OnHover?.Invoke();
        }
        public void PickedUp()
        {
            OnPickedUp?.Invoke(piece, relativeOffset);
        }
        public void ColorPulse(Color c, float overTime=1f)
        {
            OnColorPulse?.Invoke(c, overTime);
        }
        public void SetColor(Color c, float time=0.2f)
        {
            OnSetColor?.Invoke(c, time);
        }
    }
}
