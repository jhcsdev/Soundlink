using System.Collections.Generic;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid.Visuals 
{
    [RequireComponent(typeof(GridTile))]
    public class GridTileVisuals : MonoBehaviour
    {
        private SpriteRenderer srBase;
        private SpriteRenderer rendererTwo;
        private SpriteRenderer overlayRenderer;
        private Sprite baseEmptySprite;
        private GridTile tile;
        void Awake()
        {
            srBase = CreateChildRenderer("base render");
            srBase.sortingOrder = 0;
            rendererTwo = CreateChildRenderer("renderer level 2");
            rendererTwo.sortingOrder = 1;
            overlayRenderer = CreateChildRenderer("overlay renderer");
            overlayRenderer.sortingOrder = 2;
            tile = GetComponent<GridTile>();
        }

        private SpriteRenderer CreateChildRenderer(string childName)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform);
            child.transform.localPosition = Vector3.zero;
            return child.AddComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            if(tile == null) 
            tile.OnRerender += UpdateSprites;
            tile.OnJoinedLink += UpdateLinkVisuals;
        }
        void OnDisable()
        {
            tile.OnRerender -= UpdateSprites;
            tile.OnJoinedLink -= UpdateLinkVisuals;
        }

        void UpdateSprites()
        {
            // request the tiles the gridtile owns
            List<PieceTile> pieceTiles = tile.GetPieceTiles();

            // set srBase to the first tile's representation
            if (pieceTiles == null || pieceTiles.Count <= 0) { 
                srBase.sprite = baseEmptySprite;
                rendererTwo.sprite = null;
                overlayRenderer.sprite = null;
                return;
            }


            // set rendererTwo only if necessary
            if (pieceTiles.Count == 1) { rendererTwo.sprite = null; return; }
            
        
        } 

        void UpdateLinkVisuals()
        {
            
        }

        public void SetBaseSprite(Sprite to)
        {
            baseEmptySprite = to;
            UpdateSprites();
        }
    }
}