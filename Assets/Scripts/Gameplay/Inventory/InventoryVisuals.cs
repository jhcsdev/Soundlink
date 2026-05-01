using System.Collections.Generic;
using DG.Tweening;
using GamePieces;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    [RequireComponent(typeof(Inventory))]
    public class InventoryVisuals : MonoBehaviour
    {
        #region settable in inspector

        #region config
        [Header("Visual Configuration")]
        [SerializeField] private int maxRowsDisplayedAtOnce;
        [SerializeField] private int columnsDisplayedAtOnce;
        [SerializeField] private float minRowAnchorX = 0.05f;
        [SerializeField] private float maxRowAnchorX = 0.95f;
        [SerializeField] private float inventoryAnchorWidth = 0.45f;
        private float rowAnchorYHeight;
        [SerializeField] private Vector2 piecePositionWhileInInventory = new(-1000, -1000);
        [SerializeField] private float pieceScalingMultiplier = 0.25f;
        #endregion config

        #region animation
        [Header("Animation configuration")]
        [Header("Focus / unfocus animation")]
        [SerializeField] private float focusedAnchorX = 0.55f;
        [SerializeField] private float unfocusedAnchorX = 0.8f;
        [SerializeField] private float focusUnfocusAnimationTime = 0.2f;
        [Header("Focus pointer animation")]
        [SerializeField] private float pointerMoveTime = 0.1f;
        [SerializeField] private float pointerSquishTo = 0.8f;
        [SerializeField] private float pointerNormalSize = 1f;
        [SerializeField] private float pointerVanishTime = 0.3f;
        [SerializeField] private float pointerReappearTime = 0.3f;
        [SerializeField] private float pointerFailedMovementTime = 0.2f;
        [SerializeField] private Color pointerFailedMovementColor = new(0.9f, 0.5f, 0.5f);
        private Color pointerNormalColor;
        [SerializeField, Tooltip("the percent of the way through that the focus pointer will appear when grid is focused")]
        private float pointerAppearAtRatioTime = 0.7f;
        [SerializeField, Tooltip("vice-versa to above")]
        private float pointerDisappearAtRatioTime = 0.3f;

        [Header("Scroll animation")]
        [SerializeField] private float scrollAnimationTime = 0.15f;

        [Header("pieces")]
        [SerializeField] private float pointerScaleDownTime = 0.2f;
        #endregion

        #region objs / textures
        [Header("Objects")]
        [SerializeField] private RectTransform inventoryCanvas;
        [SerializeField] private RectTransform inventoryRows;
        [SerializeField] private RectTransform pointerObject;
        private Image pointerImageComponent;
        #endregion

        #endregion

        #region state management

        private List<RectTransform> _rowObjects = new();

        // dictionary of actual pieces; does not have entries for empty locations
        private Dictionary<Vector2Int, RectTransform> _pieceAtPosition = new();

        // has an entry for every potential spot in the grid
        private Dictionary<Vector2Int, RectTransform> _slotAnchors = new();

        #region saved animations
        Sequence unfocusSequence;
        Sequence focusSequence;
        Sequence activeFocusObjectMovementSequence;
        Sequence activeFailedMovement;
        Sequence takingPieceOut;
        Sequence activeScrollSequence;
        #endregion

        #endregion

        private Inventory inventory;

        public void Awake()
        {
            if (!TryGetComponent(out inventory))
                Debug.LogWarning("Missing required Inventory component on " + name);

            rowAnchorYHeight = 1f / maxRowsDisplayedAtOnce;
            pointerImageComponent = pointerObject.GetComponent<Image>();
            pointerNormalColor = pointerImageComponent.color;
        }

        void OnEnable()
        {
            inventory.OnFocusMovementSuccess += FocusLocationChanged;
            inventory.OnFocusMovementFailure += FocusLocationFailedChange;
            inventory.OnInventoryFocused += FocusGrid;
            inventory.OnInventoryUnfocused += UnfocusGrid;
            inventory.OnPiecePutIntoInventory += PlacePieceInNewSpot;
            inventory.OnPieceTakenOutOfInventory += PieceTakenOut;
        }

        void OnDisable()
        {
            inventory.OnFocusMovementSuccess -= FocusLocationChanged;
            inventory.OnFocusMovementFailure -= FocusLocationFailedChange;
            inventory.OnInventoryFocused -= FocusGrid;
            inventory.OnInventoryUnfocused -= UnfocusGrid;
            inventory.OnPiecePutIntoInventory -= PlacePieceInNewSpot;
            inventory.OnPieceTakenOutOfInventory -= PieceTakenOut;
        }

        #region actual visual implementations

        void FocusGrid(Vector2Int focusPosition)
        {
            unfocusSequence?.Pause();

            if (focusSequence == null)
            {
                Vector2 destinationMin = new(focusedAnchorX, 0);
                Vector2 destinationMax = new(focusedAnchorX + inventoryAnchorWidth, 1);
                focusSequence = DOTween.Sequence()
                    .Append(
                        inventoryCanvas.DOAnchorMin(destinationMin, focusUnfocusAnimationTime)
                            .ChangeStartValue(new(unfocusedAnchorX, 0))
                    ).Join(
                        inventoryCanvas.DOAnchorMax(destinationMax, focusUnfocusAnimationTime)
                            .ChangeStartValue(new(unfocusedAnchorX + inventoryAnchorWidth, 1))
                    ).Insert(
                        focusUnfocusAnimationTime * pointerAppearAtRatioTime,
                        pointerObject.DOScale(Vector3.one * pointerNormalSize, pointerReappearTime)
                            .ChangeStartValue(Vector3.zero)
                            .SetEase(Ease.OutCubic)
                    ).SetAutoKill(false);
                focusSequence.Play();
            }
            else focusSequence.Restart();
        }

        void UnfocusGrid()
        {
            focusSequence?.Pause();

            if (unfocusSequence == null)
            {
                Vector2 destinationMin = new(unfocusedAnchorX, 0);
                Vector2 destinationMax = new(unfocusedAnchorX + inventoryAnchorWidth, 1);
                unfocusSequence = DOTween.Sequence()
                    .Append(
                        inventoryCanvas.DOAnchorMin(destinationMin, focusUnfocusAnimationTime)
                            .ChangeStartValue(new(focusedAnchorX, 0))
                    ).Join(
                        inventoryCanvas.DOAnchorMax(destinationMax, focusUnfocusAnimationTime)
                            .ChangeStartValue(new(focusedAnchorX + inventoryAnchorWidth, 1))
                    ).Insert(
                        focusUnfocusAnimationTime * pointerDisappearAtRatioTime,
                        pointerObject.DOScale(Vector3.zero, pointerVanishTime)
                            .ChangeStartValue(Vector3.one * pointerNormalSize)
                            .SetEase(Ease.OutCubic)
                    ).SetAutoKill(false).Pause();
            }

            if (takingPieceOut != null && takingPieceOut.IsActive())
                takingPieceOut.OnComplete(() => unfocusSequence.Restart());
            else
                unfocusSequence.Restart();
        }

        void FocusLocationChanged(Vector2Int direction, Vector2Int newFocus)
        {
            if (!_slotAnchors.TryGetValue(newFocus, out RectTransform targetPos))
            {
                Debug.LogWarning($"FocusLocationChanged: no slot anchor at {newFocus}");
                return;
            }

            if (activeFocusObjectMovementSequence != null)
                activeFocusObjectMovementSequence.Complete();

            pointerObject.localScale = Vector3.one * pointerNormalSize;

            activeFocusObjectMovementSequence = DOTween.Sequence()
                .Append(
                    pointerObject.DOAnchorPos(
                        (Vector2)pointerObject.parent.InverseTransformPoint(targetPos.position),
                        pointerMoveTime)
                ).Join(
                    pointerObject.DOPunchScale(
                        new(-pointerSquishTo * Mathf.Abs(direction.y),
                             -pointerSquishTo * Mathf.Abs(direction.x), 1),
                        pointerMoveTime)
                ).Append(
                    pointerObject.DOScale(Vector3.one * pointerNormalSize, 0)
                ).Play();

            int totalRows = _rowObjects.Count;
            if (totalRows > maxRowsDisplayedAtOnce)
            {
                float rowHeight = inventoryCanvas.rect.height / maxRowsDisplayedAtOnce;
                float targetScrollY = Mathf.Clamp(
                    (newFocus.y - (maxRowsDisplayedAtOnce - 1) * 0.5f) * rowHeight,
                    0f,
                    (totalRows - maxRowsDisplayedAtOnce) * rowHeight
                );

                activeScrollSequence?.Kill();
                activeScrollSequence = DOTween.Sequence()
                    .Append(
                        inventoryRows.DOAnchorPosY(targetScrollY, scrollAnimationTime)
                            .SetEase(Ease.OutQuad)
                    ).Play();
            }
        }

        void FocusLocationFailedChange(Vector2Int directionFailed)
        {
            if (activeFailedMovement != null) activeFailedMovement.Complete();

            pointerObject.localScale = Vector3.one * pointerNormalSize;

            activeFailedMovement = DOTween.Sequence()
                .Append(
                    pointerObject.DOPunchAnchorPos(directionFailed * 10, pointerFailedMovementTime)
                ).Join(
                    pointerImageComponent.DOColor(pointerFailedMovementColor, pointerFailedMovementTime * 0.6f)
                ).Join(
                    pointerObject.DOPunchScale(
                        new(-pointerSquishTo * Mathf.Abs(directionFailed.y) * 0.3f,
                             -pointerSquishTo * Mathf.Abs(directionFailed.x) * 0.3f, 1),
                        pointerFailedMovementTime)
                ).Insert(
                    pointerFailedMovementTime * 0.6f,
                    pointerImageComponent.DOColor(pointerNormalColor, pointerFailedMovementTime * 0.7f)
                ).Append(
                    pointerObject.DOScale(Vector3.one * pointerNormalSize, 0)
                ).Play();
        }

        void PlacePieceInNewSpot(Piece piece, Vector2Int spot)
        {
            Debug.Log("Place piece in new spot");
            piece.transform.position = piecePositionWhileInInventory;

            while (_rowObjects.Count <= spot.y)
            {
                int newRowIndex = _rowObjects.Count;
                GameObject row = new($"Row {newRowIndex}", typeof(RectTransform));
                RectTransform rowTransform = row.GetComponent<RectTransform>();
                rowTransform.SetParent(inventoryRows, false);

                float anchorY = 1f - (newRowIndex + 0.5f) / maxRowsDisplayedAtOnce;
                rowTransform.anchorMin = new(minRowAnchorX, anchorY - 0.5f * rowAnchorYHeight);
                rowTransform.anchorMax = new(maxRowAnchorX, anchorY + 0.5f * rowAnchorYHeight);
                _rowObjects.Add(rowTransform);

                for (int col = 0; col < columnsDisplayedAtOnce; col++)
                {
                    GameObject anchor = new($"Anchor ({col},{newRowIndex})", typeof(RectTransform));
                    RectTransform anchorTransform = anchor.GetComponent<RectTransform>();
                    anchorTransform.SetParent(rowTransform, false);
                    anchorTransform.sizeDelta = Vector2.zero;

                    float anchorX = (col + 0.5f) / columnsDisplayedAtOnce;
                    anchorTransform.anchorMin = anchorTransform.anchorMax = new(anchorX, 0.5f);

                    _slotAnchors[new Vector2Int(col, newRowIndex)] = anchorTransform;
                }
            }

            RectTransform canvasPieceTransform = (RectTransform)piece.GetCanvasPiece().transform;
            canvasPieceTransform.SetParent(_rowObjects[spot.y], false);
            canvasPieceTransform.localScale = Vector2.one * pieceScalingMultiplier;

            float pieceAnchorX = (spot.x + 0.5f) / columnsDisplayedAtOnce;
            canvasPieceTransform.anchorMin = canvasPieceTransform.anchorMax = new(pieceAnchorX, 0.5f);
            canvasPieceTransform.anchoredPosition = Vector2.zero;

            _pieceAtPosition[spot] = canvasPieceTransform;
        }

        void PieceTakenOut(Piece piece)
        {
            RectTransform canvasPieceTransform = (RectTransform)piece.GetCanvasPiece().transform;

            Vector2Int removedKey = default;
            bool found = false;
            foreach (var kvp in _pieceAtPosition)
            {
                if (kvp.Value != canvasPieceTransform) continue;
                removedKey = kvp.Key;
                found = true;
                break;
            }

            if (found)
                _pieceAtPosition.Remove(removedKey);
            else
                Debug.LogWarning($"PieceTakenOut: canvas piece for '{piece.name}' not found in _pieceAtPosition");

            if (takingPieceOut != null) takingPieceOut.Complete();

            takingPieceOut = DOTween.Sequence()
                .Append(canvasPieceTransform.DOScale(0, pointerScaleDownTime))
                .AppendCallback(() => canvasPieceTransform.SetParent(transform))
                .Play();
        }

        #endregion
    }
}