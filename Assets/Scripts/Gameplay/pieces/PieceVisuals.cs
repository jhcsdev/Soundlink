

using System.Collections.Generic;
using PuzzleGrid;
using UnityEngine;

namespace GamePieces
{
    [RequireComponent(typeof(Piece))]
    public class PieceVisuals : MonoBehaviour
    {
        #region vars
        #region anim config

        #endregion
        #region anim state management
        
        #endregion
        #region other
        private Piece piece;
        private List<PieceTile> pieceTiles;
        private List<List<PieceTile>> pulseWaves;
        private int pulseProgress = -1;
        #endregion
        #endregion
        #region initialization
        void Awake()
        {
            piece = GetComponent<Piece>();
        }

        void OnEnable()
        {
            pieceTiles = piece.GetPieceTiles();
            piece.OnPieceFailedToPlaceOnGrid += FailedGridPlace;
            piece.OnPiecePlacedOnGrid += SucceededGridPlace;
            piece.OnPieceBeingHovered += PieceIsHovering;
            piece.OnPiecePickedUp += PieceWasPickedUp;
            piece.OnPieceReturnedToInventory += PieceReturnedToInventory;

            piece.OnLinkPulse += LinkPulse;
            piece.OnJoinedLink += LinkJoined;
            piece.OnLeftLink += LinkLeft;
            piece.OnLinkBroken += LinkBroken;
        }
        void OnDisable()
        {
            
        }
        #endregion
        #region coroutine wrappers
        private void FailedGridPlace()
        {
            pieceTiles.ForEach(it => it.ColorPulse(Color.red, 0.5f));
        }
        private void SucceededGridPlace()
        {
            
        }
        private void PieceIsHovering()
        {
            pieceTiles.ForEach(it => { it.PickedUp(); it.Hovered(); });
        }
        private void PieceWasPickedUp()
        {
            
        }
        private void PieceReturnedToInventory()
        {
            
        }
        private void LinkJoined(LinkPlacementData what)
        {
            // todo::
            Debug.Log("Linked joined!");
            foreach(PieceTile pt in pieceTiles) {
                pt.SetColorPermanent(what.GetBaseColor(), 0.2f);
            }
        }
        private void LinkLeft()
        {
            Debug.Log("Link left!");
            foreach(PieceTile pt in pieceTiles)
            {
                pt.SetColorPermanent(Color.white);
            }
        }
        private void LinkBroken()
        {
            //todo:: decide if use or not
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

        #region coroutines

        #endregion
        #region helpers
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

            foreach (PieceTile other in pieceTiles)
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
        #endregion 

    }
}