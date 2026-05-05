using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace GamePieces
{
    public class Piece : MonoBehaviour
    {
        [SerializeField] private PieceData data;
        private PieceState currentState;
        private static float tileScale = 1;
        private static float gridTileScale = 1f;


        private readonly List<PieceTile> tileObjects = new();
        private bool initialized = false;
        private List<List<PieceTile>> pulseWaves;
        private int pulseProgress = -1;

        // this is a canvas object that is exclusively used for visuals / the inventory. it is not a child object. 
        private GameObject canvasPiece;
        public GameObject GetCanvasPiece() => canvasPiece;

        #region initialization
        void Awake()
        {
            if (data == null) return;

            initialized = true;
            CreateTiles();
        }
        public void Initialize(PieceData pd)
        {
            if (initialized == true) { Debug.Log("Duplicate intiialization on piece: " + name); return; }

            initialized = true;
            data = pd;
            CreateTiles();
        }
        private void CreateTiles()
        {
            canvasPiece = new GameObject("CanvasPiece", typeof(RectTransform));

            foreach (PieceTileData ptd in data.tiles)
            {
                PieceTile t = TileManager.Instance.CreatePieceAndCanvasTile(ptd, out GameObject canvasVisual, isSilent: data.isSilentPiece)
                    .Initialize(ptd, this)
                    .SetParent(transform)
                    .SetLocalPositionAndScaleByTileSize(tileScale)
                    .CreateGlueSprites();
                tileObjects.Add(t);
                
                canvasVisual.transform.SetParent(canvasPiece.transform, false);

                RectTransform rt = canvasVisual.GetComponent<RectTransform>();
                float tileSize = rt.sizeDelta.x; 
                rt.anchoredPosition = new Vector2(ptd.relativeOffset.x * tileSize, ptd.relativeOffset.y * tileSize);
            }
        }
        #endregion 

        #region mode context
        public Piece InventoryMode()
        {
            currentState = PieceState.INVENTORY;
            return this;
        }
        public Piece LimboMode()
        {
            if (currentState == PieceState.HOVER_GRID) { 
                foreach(PieceTile pt in tileObjects) pt.Hovered();
                return this;
            } 
            currentState = PieceState.HOVER_GRID;

            transform.localScale = Vector2.one * gridTileScale; 

            foreach(PieceTile pt in tileObjects) { pt.PickedUp(); pt.Hovered(); }// these are visuals

            return this;
        }
        public Piece GridMode()
        {
            if (currentState == PieceState.PLACED_GRID) return this;
            currentState = PieceState.PLACED_GRID;
                        
            return this;
        }
        #endregion
        
        #region other
        public List<PieceTile> GetPieceTiles()
        {
            return tileObjects;
        }

        public void RotatePieceClockwise()
        {
            foreach (PieceTile pt in tileObjects)
            {
                pt.RotateRelativeOffsetClockwise();
            }
        }
        public void RotatePieceCounterClockwise()
        {
            foreach (PieceTile pt in tileObjects)
            {
                pt.RotateRelativeOffsetCounterClockwise();
            }
        }
        public bool IsSilentPiece() => data.isSilentPiece;
        #endregion

        #region things that should really be in a visuals class but aren't
        public void FailedPlace()
        {
            foreach(PieceTile pt in tileObjects) pt.ColorPulse(Color.red, 0.5f);
        }
        public void SetColorPermanent(Color color, float overTime=0.2f)
        {
            foreach(PieceTile pt in tileObjects) {
                pt.SetColorPermanent(color, overTime);
            }
        }
        #endregion 

        #region pulsing logic
        public void BuildPulseOrder(PieceTile startTile, PieceTile endTile)
        {
            if (startTile == null) Debug.LogWarning("BUILD PULSE ORDER START TILE NULL");
            if (endTile == null) Debug.LogWarning("BUILD PULSE ORDER END TILE NULL");

            var rawWaves = new List<List<PieceTile>>();

            var visited = new Dictionary<PieceTile, int>();
            var queue = new Queue<PieceTile>();

            visited[startTile] = 0;
            queue.Enqueue(startTile);

            while (queue.Count > 0)
            {
                PieceTile current = queue.Dequeue();
                int depth = visited[current];

                while (rawWaves.Count <= depth)
                    rawWaves.Add(new List<PieceTile>());

                rawWaves[depth].Add(current);

                foreach (PieceTile neighbor in GetIntraPieceNeighbors(current))
                {
                    if (visited.ContainsKey(neighbor)) continue;
                    visited[neighbor] = depth + 1;
                    queue.Enqueue(neighbor);
                }
            }

            ExpandWavesWithGapBeats(rawWaves);
        }

        /// <summary>
        /// adds "null" waves for waves of greater than 1 beat
        /// </summary>
        private void ExpandWavesWithGapBeats(List<List<PieceTile>> rawWaves)
        {
            pulseWaves = new List<List<PieceTile>>();

            foreach (List<PieceTile> wave in rawWaves)
            {
                pulseWaves.Add(wave);

                for (int i = 1; i < wave.Count; i++)
                    pulseWaves.Add(null);
            }
        }

        /// <summary>
        /// grabs nearby tiles based off unrotated relative offset
        /// </summary>
        private List<PieceTile> GetIntraPieceNeighbors(PieceTile tile)
        {
            var neighbors = new List<PieceTile>();
            Vector2Int pos = tile.GetUnrotatedRelativeOffset();

            foreach (PieceTile other in tileObjects)
            {
                if (other == tile) continue;
                Vector2Int delta = other.GetUnrotatedRelativeOffset() - pos;
                if ((Mathf.Abs(delta.x) == 1 && delta.y == 0) ||
                    (delta.x == 0 && Mathf.Abs(delta.y) == 1))
                {
                    neighbors.Add(other);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// pulse tiles in wave order, provided pulseWaves is already created (creates if not)
        /// </summary>
        /// <param name="color">The pulse color</param>
        /// <param name="startTile">The tile to begin pulsing from</param>
        /// <param name="isFirst">If true, rebuilds the pulse order from startTile</param>
        public void LinkPulse(Color color, PieceTile startTile, PieceTile endTile, bool isFirst)
        {
            if (isFirst || pulseWaves == null)
            {
                BuildPulseOrder(startTile, endTile);
                pulseProgress = 0;
            } else pulseProgress += 1;

            if (pulseProgress >= pulseWaves.Count) Debug.LogWarning("Pulse progress exceeds pulseWaves.Count.");
            else
            {
                if (pulseWaves[pulseProgress] == null) return;
                foreach (PieceTile pt in pulseWaves[pulseProgress])
                {
                    pt.ColorPulse(color, 2);
                }
            }
        }
        #endregion
    }

    public enum PieceState
    {
        INVENTORY,
        PLACED_GRID, 
        HOVER_GRID
    }
}
