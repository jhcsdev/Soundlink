

using System.Collections.Generic;
using DG.Tweening;
using PuzzleGrid;
using UnityEngine;

namespace GamePieces
{
    [RequireComponent(typeof(Piece))]
    public class PieceVisuals : MonoBehaviour
    {
        #region vars
        #region anim config
        // todo:: potentially abstract these into a runtime singleton that harvests values from a scriptable object? could be easier to switch out different configs
        [Header("placement")]
        private static float placementBetweenTileTime = 0.05f;
        private static float placementScaleupTime = 0.15f;
        private static Ease placementEaseMode = Ease.OutSine;
        private static float placementFailPunchScaleStrength = 0.07f;
        private static float placementFailPunchScaleTime = 0.6f;

        [Header("pickup")]
        private static float pickupBetweenTileTime =0.05f;
        private static float pickupScaledownTime = 0.1f;
        private static Ease pickupEaseMode = Ease.InOutSine;
        [Header("hovering")]
        private static Ease hoveringInEaseMode = Ease.OutBounce;
        private static float hoverInScaleTime = 0.75f;
        private static float hoveringScale = 0.6f;
        private static float placementScale = 1f;
        [Header("movement")]
        private static float movementTime = 0.1f;
        private static Ease movementEase = Ease.OutSine;
        private static float movementFailPunchStrength = 0.1f;

        #endregion
        #region anim state management
        
        Sequence placementSequence;
        Sequence failedPlacementSequence;
        Sequence pickupSequence;
        Sequence enterHoverSequence;
        Sequence pieceMovedSequence; 
        Sequence failedMovementSequence;
        Sequence returnToInventorySequence;

        #endregion
        #region other
        private Piece piece;
        private List<PieceTile> pieceTiles;
        private List<PieceTile> pieceTilesFromOrigin; // same list of tiles as above, but organized such that relative 0,0 is index 0, 0,1 is index 1, and so on
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
            foreach (var tile in pieceTiles)
            {
                if (tile.GetUnrotatedRelativeOffset() != Vector2Int.zero) continue;

                pieceTilesFromOrigin = UnpackNesting(BuildPulseOrder(tile));
                break;
            }
            piece.OnPieceFailedToPlaceOnGrid += FailedGridPlace;
            piece.OnPiecePlacedOnGrid += SucceededGridPlace;
            piece.OnPieceStartedHover += PieceIsHovering;
            piece.OnPiecePickedUp += PieceWasPickedUp;
            piece.OnPieceReturnedToInventory += PieceReturnedToInventory;
            piece.OnHoverPieceMoved += PieceMoved;

            piece.OnLinkPulse += LinkPulse;
            piece.OnJoinedLink += LinkJoined;
            piece.OnLeftLink += LinkLeft;
            piece.OnLinkBroken += LinkBroken;
        }
        void OnDisable()
        {
            piece.OnPieceFailedToPlaceOnGrid -= FailedGridPlace;
            piece.OnPiecePlacedOnGrid -= SucceededGridPlace;
            piece.OnPieceStartedHover -= PieceIsHovering;
            piece.OnPiecePickedUp -= PieceWasPickedUp;
            piece.OnPieceReturnedToInventory -= PieceReturnedToInventory;
            piece.OnHoverPieceMoved -= PieceMoved;

            piece.OnLinkPulse -= LinkPulse;
            piece.OnJoinedLink -= LinkJoined;
            piece.OnLeftLink -= LinkLeft;
            piece.OnLinkBroken -= LinkBroken;
        }
        #endregion
        #region sequence handlers / coroutine wrappers
        private void FailedGridPlace()
        {
            if (failedPlacementSequence == null)
            {
                failedPlacementSequence = DOTween.Sequence().Append(
                    transform.DOPunchScale(Vector3.one * placementFailPunchScaleStrength, placementFailPunchScaleTime)
                );
                failedPlacementSequence.SetAutoKill(false);
            }

            if (enterHoverSequence != null && enterHoverSequence.active) enterHoverSequence.Complete();
            if (pickupSequence != null && pickupSequence.active) pickupSequence.Complete();

            failedPlacementSequence.Restart();
            
            pieceTiles.ForEach(it => it.ColorPulse(Color.red, 0.5f)); // todo:: probably the tiles should use a tween themselves
        }
        private void SucceededGridPlace()
        {
            if (placementSequence == null) // need to set up the animation sequence
            {
                placementSequence = DOTween.Sequence().Append(
                    pieceTilesFromOrigin[0].transform.DOScale(
                        placementScale, 
                        placementScaleupTime
                    ).SetEase(placementEaseMode)
                );
                for (int i = 1; i < pieceTilesFromOrigin.Count; i++)
                {
                    placementSequence.Join(
                        // placementBetweenTileTime * i, 
                        pieceTilesFromOrigin[i].transform.DOScale(
                            placementScale, 
                            placementScaleupTime
                        ).ChangeStartValue(hoveringScale * Vector3.one).SetEase(placementEaseMode)
                    );
                }  
                placementSequence.SetAutoKill(false);
            }

            // todo:: kill/complete other potentially race-condition-inducing animations
            if (enterHoverSequence != null && enterHoverSequence.active) enterHoverSequence.Complete();
            if (pickupSequence != null && pickupSequence.active) pickupSequence.Complete();
            if (failedPlacementSequence != null && failedPlacementSequence.active) failedPlacementSequence.Complete();
            
            placementSequence.Restart();
        }
        private void PieceIsHovering()
        {
            if (enterHoverSequence == null) // need to set up animation seuqence
            {
                enterHoverSequence = DOTween.Sequence().Append(
                    pieceTilesFromOrigin[0].transform.DOScale(
                        hoveringScale, 
                        hoverInScaleTime
                    ).SetEase(hoveringInEaseMode)
                );
                for (int i = 1; i < pieceTilesFromOrigin.Count; i++)
                {
                    enterHoverSequence.Join(
                        pieceTilesFromOrigin[i].transform.DOScale(
                            hoveringScale, 
                            hoverInScaleTime
                        ).ChangeStartValue(Vector3.zero).SetEase(hoveringInEaseMode)
                    );
                }  
                enterHoverSequence.SetAutoKill(false);
            }

            enterHoverSequence.Play();
        }
        private void PieceWasPickedUp()
        {
            if (pickupSequence == null) // need to set up the animation sequence
            {
                pickupSequence = DOTween.Sequence().Append(
                    pieceTilesFromOrigin[0].transform.DOScale(
                        hoveringScale, 
                        pickupScaledownTime
                    ).SetEase(pickupEaseMode)
                );
                for (int i = 1; i < pieceTilesFromOrigin.Count; i++)
                {
                    pickupSequence.Insert(
                        pickupBetweenTileTime * i, 
                        pieceTilesFromOrigin[i].transform.DOScale(
                            hoveringScale, 
                            pickupScaledownTime
                        ).ChangeStartValue(placementScale * Vector3.one).SetEase(pickupEaseMode)
                    );
                }  
                pickupSequence.SetAutoKill(false);
            }

            if (enterHoverSequence != null && enterHoverSequence.active) enterHoverSequence.Complete();
            if (placementSequence != null && placementSequence.active) placementSequence.Complete();
            if (failedPlacementSequence != null && failedPlacementSequence.active) failedPlacementSequence.Complete();
            
            pickupSequence.Restart();
        }
        private void PieceReturnedToInventory()
        {
            
        }
        #region movement
        private void PieceMoved(Transform to)
        {
            if (pieceMovedSequence != null && pieceMovedSequence.active) pieceMovedSequence.Kill();
            pieceMovedSequence = DOTween.Sequence().Append(transform.DOMove(to.position, movementTime).SetEase(movementEase)).Play();
        }
        #endregion

        #region links
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
        public void LinkPulse(Color color, PieceTile startTile, bool isFirst)
        {
            if (isFirst || pulseWaves == null)
            {
                pulseWaves = ExpandWavesWithGapBeats(BuildPulseOrder(startTile));
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
        #endregion

        #region coroutines

        #endregion

        #region helpers
        public List<List<PieceTile>> BuildPulseOrder(PieceTile startTile)
        {
            if (startTile == null) Debug.LogWarning("BUILD PULSE ORDER START TILE NULL");

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

            return rawWaves;
        }

        /// <summary>
        /// adds "null" waves for waves of greater than 1 beat
        /// </summary>
        private List<List<PieceTile>> ExpandWavesWithGapBeats(List<List<PieceTile>> baseWaves)
        {
            var newWaves = new List<List<PieceTile>>();

            foreach (List<PieceTile> wave in baseWaves)
            {
                newWaves.Add(wave);

                for (int i = 1; i < wave.Count; i++) newWaves.Add(null);
            }
            return newWaves;
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

        private List<PieceTile> UnpackNesting(List<List<PieceTile>> list)
        {
            List<PieceTile> finalList = new();
            list.ForEach(l => finalList.AddRange(l));
            return finalList;
        }
        #endregion 

    }
}