using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GamePieces;
using UnityEngine;

namespace PuzzleGrid
{
    [RequireComponent(typeof(PuzzleGrid))]
    public class PuzzleGridVisuals : MonoBehaviour
    {
        #region variables
        #region animation config
        [Header("animation config")]
        [SerializeField] private float tilePulseScaleTime = 0.1f;
        [SerializeField] private float tilePulseToSize = 1.2f;
        #region pointer
        [Header("pointer-specific")]
        [SerializeField] private float pointerMoveTime = 0.1f;
        [SerializeField] private float pointerFailedMoveTime = 0.1f;
        [SerializeField] private Color pointerFailedMoveColor = new(0.9f, 0.5f, 0.5f);
        private Color initialPointerColor;
        [SerializeField] private float pointerSquishTo = 0.9f;
        [SerializeField] private float pointerNormalSize = 1f;
        [SerializeField] private float pointerScaleInOutTime = 0.2f;
        #endregion
        #endregion

        #region animation state
        Sequence pointerSuccessMoveSequence;
        Sequence pointerFailureMoveSequence;

        Sequence activatePointerSequence;
        Sequence deactivatePointerSequence;

        Dictionary<GridTile, Sequence> hoverSequences;

        private Sequence camMoveSequence;

        #endregion

        #region objects / textures
        [Header("objects to be set")]
        [SerializeField] private Transform gridPointer;
        private SpriteRenderer pointerRenderer;
        [SerializeField] private List<Sprite> gridTiles;
        #endregion

        #region camera
        private Camera cam;
        [SerializeField] private Vector2 camOffset = new(1.5f, 0);
        [SerializeField] private int displayGridTilesX;
        [SerializeField] private int displayGridTilesY;
        [SerializeField] private float camMoveTime = 0.15f;
        [SerializeField, Tooltip("applied to zooming to give slight buffer at edges")] 
        private float camZoomRatioMult = 1.25f; 
        #endregion
        
        #region other variables
        PuzzleGrid grid;
        private Vector2 maximumKnownTilePosition = Vector2.zero;
        private Vector2 maximumKnownPosition = Vector2.negativeInfinity;
        private Vector2 minimumKnownPosition = Vector2.positiveInfinity;
        #endregion

        #endregion

        void Awake()
        {
            grid = GetComponent<PuzzleGrid>();
            pointerRenderer = gridPointer.GetComponent<SpriteRenderer>();
            initialPointerColor = pointerRenderer.color;
            cam = Camera.main;
        }

        #region initialize
        void OnEnable()
        {
            grid.OnFailedLeavingGrid += FocusPositionFailedChange;
            grid.OnNewFocusedTile += FocusPositionSuccessfullyChanged;
            grid.OnActivatePointer += PointerActivate;
            grid.OnDeactivatePointer += PointerDeactivate;

            grid.OnNewHover += HoverPositionChanged;
            grid.OnHoveringTile += HoverTile;
            grid.OnGridTileInitialize += InitializeTile;
            grid.OnGridFinishedInitialize += InitializeGrid;
            
            grid.OnPieceYoinked += PickupPiece;
            grid.OnPiecePlacementSuccess += PlacePieceSuccess;
            grid.OnPiecePlacementFailure += PlacePieceFailure;
            grid.OnPieceSentBackToInventory += SendPieceToInventory;
        }
        void OnDisable()
        {
            grid.OnFailedLeavingGrid -= FocusPositionFailedChange;
            grid.OnNewFocusedTile -= FocusPositionSuccessfullyChanged;
            grid.OnActivatePointer -= PointerActivate;
            grid.OnDeactivatePointer -= PointerDeactivate;

            grid.OnNewHover -= HoverPositionChanged;
            grid.OnHoveringTile -= HoverTile;
            grid.OnGridTileInitialize -= InitializeTile;
            grid.OnGridFinishedInitialize -= InitializeGrid;
            
            grid.OnPieceYoinked -= PickupPiece;
            grid.OnPiecePlacementSuccess -= PlacePieceSuccess;
            grid.OnPiecePlacementFailure -= PlacePieceFailure;
            grid.OnPieceSentBackToInventory -= SendPieceToInventory;
        }

        void InitializeTile(GridTile tile, GridTileType type, Vector2 position)
        {
            tile.ResetChildRenderer("GridTileRenderer");
            int spriteIndex = (int)(position.x + position.y) % gridTiles.Count;
            tile.SetSprite(gridTiles[spriteIndex]);
            if (tile.HasLinkPlacementData()) tile.SetLinkColorByPlacementData();
            Vector2 worldPos = tile.transform.position;

            maximumKnownTilePosition = Vector2.Max(maximumKnownTilePosition, position);
            minimumKnownPosition = Vector2.Min(minimumKnownPosition, worldPos);
            maximumKnownPosition = Vector2.Max(maximumKnownPosition, worldPos);
        }

        void InitializeGrid()
        {
            Vector3 halfway = minimumKnownPosition + (maximumKnownPosition - minimumKnownPosition) / 2;
            cam.transform.position = new(halfway.x + camOffset.x, halfway.y + camOffset.y, cam.transform.position.z); 

            float distance = maximumKnownPosition.y - minimumKnownPosition.y;
            cam.orthographicSize = distance * (camZoomRatioMult + (distance > 10 ? 0 : 1/distance)) / cam.aspect;
        }

        #endregion

        #region actual visual implementations
        
        #region focusing grid / pointer
        void PointerActivate(GridTile focusedTile)
        {
            deactivatePointerSequence?.Pause();

            gridPointer.position = focusedTile.transform.position;

            if (activatePointerSequence == null)
            {
                activatePointerSequence = DOTween.Sequence()
                    .Append(
                        gridPointer.DOScale(Vector3.one * pointerNormalSize, pointerScaleInOutTime)
                            .ChangeStartValue(Vector3.zero)
                            .SetEase(Ease.OutCubic)
                    ).SetAutoKill(false);
                activatePointerSequence.Play();
            } else activatePointerSequence.Restart();
        }

        void PointerDeactivate(GridTile focusedTile)
        {
            ResetAllHovers();
            activatePointerSequence.Pause();

            gridPointer.position = focusedTile.transform.position;

            if (deactivatePointerSequence == null)
            {
                deactivatePointerSequence = DOTween.Sequence()
                    .Append(
                        gridPointer.DOScale(Vector3.zero, pointerScaleInOutTime)
                            .ChangeStartValue(Vector3.one * pointerNormalSize)
                            .SetEase(Ease.OutCubic)
                    ).SetAutoKill(false).Pause();
            } 

            deactivatePointerSequence.Restart();
        }

        void FocusPositionSuccessfullyChanged(Vector2Int direction, GridTile toTile, bool isPointerActive)
        {
            // complete an existing move tween (this will create some jitter instantly, but i believe that to be okay?
            if (isPointerActive)
            {
                if (pointerSuccessMoveSequence != null) pointerSuccessMoveSequence.Complete();

                gridPointer.localScale = Vector3.one * pointerNormalSize;

                pointerSuccessMoveSequence = DOTween.Sequence()
                    .Append(
                        gridPointer.DOMove(toTile.transform.position, pointerMoveTime)
                    ).Join(
                        gridPointer.DOPunchScale(new(-pointerSquishTo * Mathf.Abs(direction.y), -pointerSquishTo * Mathf.Abs(direction.x), 1), pointerMoveTime)
                    ).Append(
                        gridPointer.DOScale(Vector3.one * pointerNormalSize, 0)
                    ).Play();
            }

            Vector2 gridTileCount = maximumKnownTilePosition + Vector2.one;

            Vector2 tileSize = new(
                gridTileCount.x > 1 ? (maximumKnownPosition.x - minimumKnownPosition.x) / (gridTileCount.x - 1) : 1f,
                gridTileCount.y > 1 ? (maximumKnownPosition.y - minimumKnownPosition.y) / (gridTileCount.y - 1) : 1f
            );

            Vector2 focusWorldPos = toTile.transform.position;
            Vector2 gridCenter = (minimumKnownPosition + maximumKnownPosition) * 0.5f;

            float targetX, targetY;

            if (gridTileCount.x <= displayGridTilesX)
                targetX = gridCenter.x;
            else
            {
                float halfDisplayX = (displayGridTilesX * 0.5f) * tileSize.x;
                targetX = Mathf.Clamp(focusWorldPos.x, minimumKnownPosition.x + halfDisplayX, maximumKnownPosition.x - halfDisplayX);
            }

            if (gridTileCount.y <= displayGridTilesY)
                targetY = gridCenter.y;
            else
            {
                float halfDisplayY = (displayGridTilesY * 0.5f) * tileSize.y;
                targetY = Mathf.Clamp(focusWorldPos.y, minimumKnownPosition.y + halfDisplayY, maximumKnownPosition.y - halfDisplayY);
            }

            Vector3 targetPos = new(targetX + camOffset.x, targetY + camOffset.y, cam.transform.position.z);

            camMoveSequence?.Kill();
            camMoveSequence = DOTween.Sequence()
                .Append(
                    cam.transform.DOMove(targetPos, camMoveTime)
                        .SetEase(Ease.OutQuad)
                ).Play();
        }

        void FocusPositionFailedChange(Vector2Int direction)
        {
            // Thought: Stretch the focus shape in a given direction, color it red for now
            if (pointerFailureMoveSequence != null) pointerFailureMoveSequence.Complete();

            gridPointer.localScale = Vector3.one * pointerNormalSize;

            pointerFailureMoveSequence = DOTween.Sequence()
                .Append(
                    gridPointer.DOPunchPosition(new (direction.x * 0.2f, direction.y*0.2f, 0), pointerFailedMoveTime)
                ).Join(
                    pointerRenderer.DOColor(pointerFailedMoveColor, pointerFailedMoveTime * 0.6f)
                ).Join(
                    gridPointer.DOPunchScale(
                        new(-pointerSquishTo * Mathf.Abs(direction.y) * 0.3f, -pointerSquishTo * Mathf.Abs(direction.x) * 0.3f, 1),
                        pointerFailedMoveTime
                    )
                ).Insert(
                    pointerFailedMoveTime * 0.6f, 
                    pointerRenderer.DOColor(initialPointerColor, pointerFailedMoveTime * 0.7f)
                ).Append(
                    gridPointer.DOScale(Vector3.one * pointerNormalSize, 0)
                ).Play();
        }

        #endregion

        #region tile updates
        void HoverTile(GridTile tile)
        {
            if (hoverSequences == null) { Debug.LogWarning("hover list null, allocating, but it shouldn't be"); hoverSequences = new(); }

            Sequence s = DOTween.Sequence()
                .Append(
                    tile.transform.DOScale(Vector3.one * tilePulseToSize, tilePulseScaleTime)
                        .SetEase(Ease.InOutSine)
                ).SetLoops(
                    -1, LoopType.Yoyo
                ).Play();
            hoverSequences.Add(tile, s);
        }

        void ResetHover(GridTile tile)
        {
            if (hoverSequences.TryGetValue(tile, out var tween))
            {
                tween.Kill();
                tile.transform.localScale = Vector3.one;
            }
        }
        
        void ResetAllHovers()
        {
            if (hoverSequences != null)
            {
                foreach (var tile in hoverSequences.Keys.ToList()) ResetHover(tile);
            }
            hoverSequences = new();
        }

        void HoverPositionChanged(Vector2Int focusPosition)
        {
            ResetAllHovers();
        }
        #endregion

        #region piece updates
        void PickupPiece(Piece p)
        {
            p.gameObject.SetActive(true);
        }
        void SendPieceToInventory(Piece p)
        {
            p.gameObject.SetActive(true);
            ResetAllHovers();
            // todo: cool visuals? maybe it flies back to inventory...who knows
        }

        void PlacePieceFailure(Piece p)
        {
            p.FailedGridPlace();
        }
        void PlacePieceSuccess(Piece p)
        {
            ResetAllHovers();
        }
        #endregion
        
        #endregion 
    }
}