using GamePieces;
using UnityEngine;

namespace Inventory
{
    [RequireComponent(typeof(PuzzleGrid.PuzzleGrid))]
    public class AutoPlacer : MonoBehaviour
    {
        [SerializeField] private AutoPlaceData pieces;
        
        private PuzzleGrid.PuzzleGrid grid;
        private bool hasPlaced;

        void Awake()
        {
            grid = GetComponent<PuzzleGrid.PuzzleGrid>();
            if (grid == null) Debug.LogError("AutoPlacer requires a PuzzleGrid component.");
        }

        void OnEnable()
        {
            if (grid == null) grid = GetComponent<PuzzleGrid.PuzzleGrid>();
            if (grid != null) grid.OnGridFinishedInitialize += InitializeAutoPlacements;
        }

        void OnDisable()
        {
            if (grid != null) grid.OnGridFinishedInitialize -= InitializeAutoPlacements;
        }

        private void InitializeAutoPlacements()
        {
            if (hasPlaced) return;
            hasPlaced = true;
            if (grid == null)
            {
                Debug.LogError("AutoPlacer failed to initialize because PuzzleGrid is missing.");
                return;
            }

            if (pieces == null || pieces.autoPlacements == null)
            {
                Debug.LogWarning("AutoPlacer has no auto-placement data.");
                return;
            }

            int pieceNumber = 0;
            foreach (var d in pieces.autoPlacements)
            {
                GameObject pieceObj = new($"Piece {pieceNumber++}");
                Piece pieceObjComp = pieceObj.AddComponent<Piece>();

                pieceObjComp.Initialize(d.data);
                pieceObjComp.transform.position = Vector3.one * 150;
                pieceObjComp.transform.parent = transform;

                for (int i = 0; i < d.clockwiseRotations; i++) pieceObjComp.RotatePieceClockwise();

                // Debug.Log(pieceObjComp);

                grid.PlaceAtFocusPositionWithPositionOverride(d.where, pieceObjComp);
            }
        }
    }
}