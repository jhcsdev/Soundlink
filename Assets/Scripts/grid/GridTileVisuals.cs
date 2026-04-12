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
        private GridTile tile;
        void Awake()
        {
            srBase = gameObject.AddComponent<SpriteRenderer>();
            rendererTwo = gameObject.AddComponent<SpriteRenderer>();
            overlayRenderer = gameObject.AddComponent<SpriteRenderer>();
            tile = GetComponent<GridTile>();
        }

        void OnEnable()
        {
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
            if (pieceTiles.Count <= 0) return;


            // set rendererTwo only if necessary
            if (pieceTiles.Count == 1) { rendererTwo.sprite = null; return; }
            
        
        } 

        void UpdateLinkVisuals()
        {
            
        }
    }
}