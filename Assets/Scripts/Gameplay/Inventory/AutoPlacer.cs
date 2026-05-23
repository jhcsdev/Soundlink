using GamePieces;
using UnityEngine;

namespace Inventory
{
    [RequireComponent(typeof(PuzzleGrid.PuzzleGrid))]
    public class AutoPlacer : MonoBehaviour
    {
        [SerializeField] private AutoPlaceData pieces;
        
        private PuzzleGrid.PuzzleGrid grid;
        void OnEnable()
        {
            grid = GetComponent<PuzzleGrid.PuzzleGrid>();
        }

        void Start()
        {
            int pieceNumber = 0;
            foreach (var d in pieces.autoPlacements)
            {
                GameObject pieceObj = new($"Piece {pieceNumber++}");
                Piece pieceObjComp = pieceObj.AddComponent<Piece>();

                pieceObjComp.Initialize(d.data);
                pieceObjComp.transform.position = Vector3.one * 150;
                pieceObjComp.transform.parent = transform;

                for (int i = 0; i < d.clockwiseRotations; i++) pieceObjComp.RotatePieceClockwise();

                grid.PlaceAtFocusPositionWithPositionOverride(d.where, pieceObjComp);
            }
        }
    }
}