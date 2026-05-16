

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
        private static float hoverOutScaleTime = 0.4f;
        private static Ease hoverOutEaseMode = Ease.OutQuart;
        [Header("movement")]
        private static float movementTime = 0.1f;
        private static Ease movementEase = Ease.OutSine;
        private static float movementFailPunchStrength = 0.1f;
        [Header("materials")]
        static readonly int FillOriginID = Shader.PropertyToID("_FillOrigin");
        static readonly int FillAmountID = Shader.PropertyToID("_FillDistance");
        static readonly int FillFadeColorID = Shader.PropertyToID("_FillFadeColor");
        static readonly int FillBaseColorID = Shader.PropertyToID("_FillBaseColor");
        static readonly int FillFadeDistanceID = Shader.PropertyToID("_FillFadeToBaseDistance");
        static readonly int BorderColorID = Shader.PropertyToID("_BorderColor");
        static readonly int BaseColorID = Shader.PropertyToID("_BaseColor");
        static readonly int BorderThickness = Shader.PropertyToID("_FillBorderThickness");
        private Material pieceMaterial;
        [Header("links")]
        private static float fillJoinTime = 0.8f;
        private static Ease fillEase = Ease.OutSine;
        private static float zoomBackToZeroFillOnRejoinTime = 0.5f;
        private static Ease zoomBackToZeroFillOnRejoinEase = Ease.InOutSine;
        private static float fillLeaveTime = 1f;
        private static float zoomBackToZeroFillOnLeaveTime = 0.3f;
        private Ease pulseEase = Ease.OutQuint;
        private float pulseOneUnitTime = 2f;

        #endregion
        #region anim state management
        Sequence placementSequence;
        Sequence failedPlacementSequence;
        Sequence pickupSequence;
        Sequence enterHoverSequence;
        Sequence pieceMovedSequence; 
        Sequence pieceJoinLinkColorSequence;
        Sequence pieceLeaveLinkColorSequence;
        Sequence pulseSequence;
        Sequence failedMovementSequence;
        Sequence backToInventoryShrinkSequence; // inverse of enterHoverSequence
        #endregion
        #region other
        private Piece piece;
        private List<PieceTile> pieceTiles;
        private List<PieceTile> pieceTilesFromOrigin; // same list of tiles as above, but organized such that relative 0,0 is index 0, 0,1 is index 1, and so on
        private List<List<PieceTile>> pulseWaves;
        private int pulseProgress = -1;
        private int maximumPulseDistance = -1;
        private float materialFillFadeDistance = 5;
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
            
            piece.OnSetMaterial += MaterialSet;
            piece.OnMaterialFillOriginChange += ChangeMaterialFillOrigin;
            piece.OnMaterialFillAmountChange += ChangeMaterialFillAmount;
            piece.OnMaterialBaseColorChange += ChangeMaterialBaseColor;
            piece.OnMaterialBorderColorChange += ChangeMaterialBorderColor;
            piece.OnMaterialFillFadeColorChange += ChangeMaterialFillFadeColor;
            piece.OnMaterialFillBaseColorChange += ChangeMaterialFillBaseColor;
            piece.OnMaterialFillFadeDistanceChange += ChangeMaterialFillFadeDistance;
            piece.OnMaterialBorderThicknessChange += ChangeMaterialBorderThickness;

            piece.OnLinkPulse += LinkPulse;
            piece.OnJoinedLink += LinkJoined;
            piece.OnLeftLink += LinkLeft;
        }
        void OnDisable()
        {
            piece.OnPieceFailedToPlaceOnGrid -= FailedGridPlace;
            piece.OnPiecePlacedOnGrid -= SucceededGridPlace;
            piece.OnPieceStartedHover -= PieceIsHovering;
            piece.OnPiecePickedUp -= PieceWasPickedUp;
            piece.OnPieceReturnedToInventory -= PieceReturnedToInventory;
            piece.OnHoverPieceMoved -= PieceMoved;

            piece.OnSetMaterial -= MaterialSet;
            piece.OnMaterialFillOriginChange -= ChangeMaterialFillOrigin;
            piece.OnMaterialFillAmountChange -= ChangeMaterialFillAmount;
            piece.OnMaterialBaseColorChange -= ChangeMaterialBaseColor;
            piece.OnMaterialBorderColorChange -= ChangeMaterialBorderColor;
            piece.OnMaterialFillFadeColorChange -= ChangeMaterialFillFadeColor;
            piece.OnMaterialFillBaseColorChange -= ChangeMaterialFillBaseColor;
            piece.OnMaterialFillFadeDistanceChange -= ChangeMaterialFillFadeDistance;
            piece.OnMaterialBorderThicknessChange -= ChangeMaterialBorderThickness;

            piece.OnLinkPulse -= LinkPulse;
            piece.OnJoinedLink -= LinkJoined;
            piece.OnLeftLink -= LinkLeft;
        }
        #endregion

        #region specifically dealing with materials
        private void MaterialSet(Material to)
        {
            pieceMaterial = to;
        }
        private void ChangeMaterialFillOrigin(Vector2 to)
        {
            pieceMaterial.SetVector(FillOriginID, to);
        }
        private void ChangeMaterialFillAmount(float to)
        {
            pieceMaterial.SetFloat(FillAmountID, to);
        }
        private void ChangeMaterialBaseColor(Color to)
        {
            pieceMaterial.SetColor(BaseColorID, to);
        }
        private void ChangeMaterialFillFadeColor(Color to)
        {
            pieceMaterial.SetColor(FillFadeColorID, to);
        }
        private void ChangeMaterialFillBaseColor(Color to)
        {
            pieceMaterial.SetColor(FillBaseColorID, to);
        }
        private void ChangeMaterialBorderColor(Color to)
        {
            pieceMaterial.SetColor(BorderColorID, to);
        }
        private void ChangeMaterialBorderThickness(float to)
        {
            pieceMaterial.SetFloat(BorderThickness, to);
        }
        private void ChangeMaterialFillFadeDistance(float to)
        {
            pieceMaterial.SetFloat(FillFadeDistanceID, to);
            materialFillFadeDistance = to;
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
            
            pieceTiles.ForEach(it => it.ColorPulse(Color.red, 0.5f));
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
                        pieceTilesFromOrigin[i].transform.DOScale(
                            placementScale, 
                            placementScaleupTime
                        ).ChangeStartValue(hoveringScale * Vector3.one).SetEase(placementEaseMode)
                    );
                }  
                placementSequence.SetAutoKill(false);
            }

            if (enterHoverSequence != null && enterHoverSequence.active) enterHoverSequence.Complete();
            if (pickupSequence != null && pickupSequence.active) pickupSequence.Complete();
            if (failedPlacementSequence != null && failedPlacementSequence.active) failedPlacementSequence.Complete();
            if (backToInventoryShrinkSequence != null && backToInventoryShrinkSequence.active) { backToInventoryShrinkSequence.Kill(); backToInventoryShrinkSequence = null; }
            
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

            if (backToInventoryShrinkSequence != null && backToInventoryShrinkSequence.active) { backToInventoryShrinkSequence.Kill(); backToInventoryShrinkSequence = null; }

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
            if (backToInventoryShrinkSequence != null && backToInventoryShrinkSequence.active) { backToInventoryShrinkSequence.Kill(); backToInventoryShrinkSequence = null; }
            
            pickupSequence.Restart();
        }
        private void PieceReturnedToInventory()
        {
            if (backToInventoryShrinkSequence == null || !backToInventoryShrinkSequence.active)
            {
                backToInventoryShrinkSequence = DOTween.Sequence().Append(
                    pieceTilesFromOrigin[0].transform.DOScale(
                        0, 
                        hoverOutScaleTime
                    ).SetEase(hoverOutEaseMode)
                );
                for (int i = 1; i < pieceTilesFromOrigin.Count; i++)
                {
                    backToInventoryShrinkSequence.Join(
                        pieceTilesFromOrigin[i].transform.DOScale(
                            0, 
                            hoverOutScaleTime
                        ).SetEase(hoverOutEaseMode)
                    );
                }  
            }

            if (enterHoverSequence != null && enterHoverSequence.active) { enterHoverSequence.Kill(); enterHoverSequence =null; }
            if (placementSequence != null && placementSequence.active) { placementSequence.Kill(); placementSequence = null; }
            if (failedPlacementSequence != null && failedPlacementSequence.active) { failedPlacementSequence.Complete(); }
            if (pickupSequence != null && pickupSequence.active) { pickupSequence.Kill(); pickupSequence = null; }

            backToInventoryShrinkSequence.Restart();
        }
        #region movement
        private void PieceMoved(Transform to)
        {
            if (pieceMovedSequence != null && pieceMovedSequence.active) pieceMovedSequence.Kill();
            pieceMovedSequence = DOTween.Sequence().Append(transform.DOMove(to.position, movementTime).SetEase(movementEase)).Play();
        }
        #endregion

        #region

        #endregion

        #region links
        private void LinkJoined(LinkPlacementData what, PieceTile where)
        {
            maximumPulseDistance = (int)Mathf.Ceil(CalculateFurthestDistance(where));

            // depending on the existing state, we need to do a different animation.
            // if we have an ongoing leaveLinkSequence -- zoom back to 0, then change the fill fade & fill base color, then pulse to max + fillFadeDistance, then set base color & pulse=0
            // if we have an ongoing joinLinkSequence -- error; this should not happen ever
            // if we have an ongoing pulseLinkSequence -- kill the pulse sequence and zoom back to 0
            // if we have no sequence, then pulse to max, fade fill color to base, set base & pulse = 0

            if ((pieceLeaveLinkColorSequence != null && pieceLeaveLinkColorSequence.active) || (pulseSequence != null && pulseSequence.active))
            {
                if (pieceLeaveLinkColorSequence != null && pieceLeaveLinkColorSequence.active)
                {
                    pieceLeaveLinkColorSequence.Kill();
                    pieceLeaveLinkColorSequence = null;
                }
                if (pulseSequence != null && pulseSequence.active)
                {
                    pulseSequence.Kill();
                    pulseSequence = null;
                }
                pieceJoinLinkColorSequence = DOTween.Sequence()
                    .Append(
                        pieceMaterial.DOFloat(0, FillAmountID, zoomBackToZeroFillOnRejoinTime).SetEase(zoomBackToZeroFillOnRejoinEase)
                    ).AppendCallback(() =>
                        {
                            ChangeMaterialBorderColor(what.GetBorderColor());
                            ChangeMaterialFillFadeColor(what.GetPulseColor());
                            ChangeMaterialFillBaseColor(what.GetBaseColor());
                            ChangeMaterialFillOrigin(where.transform.position);
                        }
                    ).Append(
                        pieceMaterial.DOFloat(maximumPulseDistance+materialFillFadeDistance+1, FillAmountID, fillJoinTime).SetEase(fillEase)
                    ).AppendCallback(() => 
                        {
                            ChangeMaterialBaseColor(what.GetBaseColor());
                            ChangeMaterialFillAmount(0);
                        }
                    );
            }
            else if (pieceJoinLinkColorSequence != null && pieceJoinLinkColorSequence.active)
            {
                Debug.LogError("Join link is already active in a different join link!");
            }
            else
            {
                ChangeMaterialBorderColor(what.GetBorderColor());
                ChangeMaterialFillFadeColor(what.GetPulseColor());
                ChangeMaterialFillBaseColor(what.GetBaseColor());
                ChangeMaterialFillOrigin(where.transform.position);

                pieceJoinLinkColorSequence = DOTween.Sequence()
                    .Append(
                        pieceMaterial.DOFloat(maximumPulseDistance+materialFillFadeDistance+1, FillAmountID, fillJoinTime).SetEase(fillEase)
                    ).AppendCallback(() => 
                        {
                            ChangeMaterialBaseColor(what.GetBaseColor());
                            ChangeMaterialFillAmount(0);
                        }
                    );
            }
        }
        private void LinkLeft()
        {
            Debug.Log("link leave");
            // depending on existing sequences, need to make considerations about what is happening
            // if ongoing join sequence -- revert it (todo:: may need to introduce some sort of state parameter that sets true in case of extended join sequence which reverts leaving)
            // if ongoing leave sequence -- error, this should never happen
            // if ongoing pulse sequence -- TODO, not sure what should happen
            // otherwise, just pulse to maximum distance, set baseColor white and pulse 0 afterwards
            if ((pieceJoinLinkColorSequence != null && pieceJoinLinkColorSequence.active) || (pulseSequence != null && pulseSequence.active))
            {
                if (pieceJoinLinkColorSequence != null && pieceJoinLinkColorSequence.active)
                {
                    pieceJoinLinkColorSequence.Kill();
                    pieceJoinLinkColorSequence = null;
                }
                if (pulseSequence != null && pulseSequence.active)
                {
                    pulseSequence.Kill();
                    pulseSequence = null;
                }
                pieceLeaveLinkColorSequence = DOTween.Sequence()
                    .Append(
                        pieceMaterial.DOFloat(0, FillAmountID, zoomBackToZeroFillOnLeaveTime).SetEase(zoomBackToZeroFillOnRejoinEase)
                    ).AppendCallback(() =>
                        {
                            ChangeMaterialBorderColor(Color.white);
                            ChangeMaterialFillFadeColor(Color.white);
                            ChangeMaterialFillBaseColor(Color.white);
                        }
                    ).Append(
                        pieceMaterial.DOFloat(maximumPulseDistance+1, FillAmountID, fillLeaveTime).SetEase(fillEase)
                    ).AppendCallback(() => 
                        {
                            ChangeMaterialBaseColor(Color.white);
                            ChangeMaterialFillAmount(0);
                        }
                    );
            }
            else if (pieceLeaveLinkColorSequence != null && pieceLeaveLinkColorSequence.active)
            {
                Debug.LogError("Leaving a link, but the leave link animation is already active!");
            }
            else
            {
                ChangeMaterialBorderColor(Color.white);
                ChangeMaterialFillFadeColor(Color.white);
                ChangeMaterialFillBaseColor(Color.white);

                pieceLeaveLinkColorSequence = DOTween.Sequence()
                    .Append(
                        pieceMaterial.DOFloat(maximumPulseDistance+1, FillAmountID, fillLeaveTime).SetEase(fillEase)
                    ).AppendCallback(() => 
                        {
                            ChangeMaterialBaseColor(Color.white);
                            ChangeMaterialFillAmount(0);
                        }
                    );
            }
        }

        /// <summary>
        /// pulse tiles in wave order, provided pulseWaves is already created (creates if not)
        /// </summary>
        /// <param name="color">The pulse color</param>
        /// <param name="startTile">The tile to begin pulsing from</param>
        /// <param name="isFirst">If true, rebuilds the pulse order from startTile</param>
        public void LinkPulse(Color color, PieceTile startTile, bool isFirst, float secondsPerBeat)
        {
            ChangeMaterialFillOrigin(startTile.transform.position);
            if (isFirst)
            {
                // pulseWaves = ExpandWavesWithGapBeats(BuildPulseOrder(startTile));
                pulseProgress = 1;
                ChangeMaterialFillAmount(0);
            } else pulseProgress += 1;
            
            float progressThrough = (pulseProgress / (float) pieceTiles.Count) * maximumPulseDistance;

            if (pulseSequence != null && pulseSequence.active) pulseSequence.Kill();
            pulseSequence = DOTween.Sequence();

            if(pulseProgress == pieceTiles.Count) {
                Debug.Log("LAST");
                pulseSequence.Append(
                    pieceMaterial.DOFloat(maximumPulseDistance+materialFillFadeDistance+1, FillAmountID, pulseOneUnitTime)
                ).Pause();
            } else
            {
                pulseSequence.Append(
                    pieceMaterial.DOFloat(progressThrough, FillAmountID, secondsPerBeat).SetEase(pulseEase)
                ).Pause();
            }

            if (pieceJoinLinkColorSequence != null && pieceJoinLinkColorSequence.active)
            {
                // pieceJoinLinkColorSequence.OnComplete(() => pulseSequence?.Play());
            } else pulseSequence.Play();
        }
        #endregion
        #endregion

        #region coroutines

        #endregion

        #region helpers
        private float CalculateFurthestDistance(PieceTile startTile)
        {
            float dist = 0;
            foreach (var tile in pieceTiles)
            {
                float temp = (startTile.GetRotatedRelativeOffset() - tile.GetRotatedRelativeOffset()).magnitude;
                dist = temp > dist ? temp : dist; 
            }
            return dist;
        }
        private List<List<PieceTile>> BuildPulseOrder(PieceTile startTile)
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
