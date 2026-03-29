using System.Collections.Generic;
using UnityEngine;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private PieceData data;

        private List<PieceTile> tileObjects;

        void Awake()
        {
            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = PieceTileBuilder.Instance.CreateTile();
            }
        }

        public void InventoryMode()
        {
            
        }
        public void GridMode()
        {
            
        }
        public List<PieceTile> GetPieceTiles()
        {
            // todo
            return new();
        }

        public void RotatePieceClockwise()
        {
            
        }
        public void RotatePieceCounterClockwise()
        {
            
        }
    }
}
