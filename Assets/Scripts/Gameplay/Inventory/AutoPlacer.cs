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
    }
}