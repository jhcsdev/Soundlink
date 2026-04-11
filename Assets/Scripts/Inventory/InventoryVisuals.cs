using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using GamePieces;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    /// <summary>
    /// The inventory data is entirely controlled by the Inventory class. Inventory Visuals assumes that whatever it is sent is
    /// the absolute source of truth, and it also assumes that following those instructions will not cause visual conflicts 
    /// (which imo is fine). 
    /// 
    /// It is a canvas object, and it is arranged into row objects, which each get piece canvas objects placed as children beneath them. 
    /// When moving left / right, you move between rows; when moving up & down, the entire grid may have to shift in order to bring pieces 
    /// into the screen.
    /// </summary>
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

        #endregion

        #region objs
        [Header("Objects")]
        [SerializeField] private RectTransform inventoryCanvas;
        [SerializeField] private RectTransform pointerObject; 
        private Image pointerImageComponent;
        #endregion
        
        #endregion

        #region state management 
        private int _currentFocusedRow;
        private int _totalNumberOfRows;
        private List<RectTransform> _rowObjects = new();

        #region saved animations
        Sequence unfocusSequence;
        Sequence focusSequence;
        Sequence activeFocusObjectMovementSequence;
        Sequence activeFailedMovement;
        #endregion
        #endregion

        private Inventory inventory;

        public void Awake()
        {
            if (!TryGetComponent(out inventory)) Debug.LogWarning("Missing required Inventory component on " + name);

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
            Debug.Log("focus grid");
            unfocusSequence?.Pause();
            // Todo - pull to the left and do some sort of visuals
            if (focusSequence == null)
            {
                Debug.Log("setting up focus sequence");
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
            } else focusSequence.Restart();
        }

        void UnfocusGrid()
        {
            Debug.Log("unfocus grid");
            focusSequence.Pause();
            // todo - fade visuals slightly, push back to right

            if (unfocusSequence == null)
            {
                Debug.Log("setting up unfocus sequence");
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
                    ).SetAutoKill(false);
                unfocusSequence.Play();
            } else unfocusSequence.Restart();
        }

        /// <summary>
        /// moves the pointer object and squeezes it a bit. 
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="newFocus"></param>
        void FocusLocationChanged(Vector2Int direction, Vector2Int newFocus)
        {
            // Find the target object to focus
            if (newFocus.y >= _rowObjects.Count) { Debug.LogWarning($"Tried to move to: {newFocus}, but there are only {_rowObjects.Count} rows!"); return; }
            
            if (newFocus.x >= _rowObjects[newFocus.y].childCount) { Debug.LogWarning($"Tried to move to: {newFocus}; the row exists, but it only has {_rowObjects[newFocus.y].childCount} children"); return;}

            RectTransform targetPos = (RectTransform)_rowObjects[newFocus.y].GetChild(newFocus.x);

            // complete an existing move tween (this will create some jitter instantly, but i believe that to be okay?)
            if (activeFocusObjectMovementSequence != null) activeFocusObjectMovementSequence.Complete();

            activeFocusObjectMovementSequence = DOTween.Sequence()
                .Append(
                    pointerObject.DOAnchorPos((Vector2)pointerObject.parent.InverseTransformPoint(targetPos.position), pointerMoveTime)
                ).Join(
                    pointerObject.DOPunchScale(new(-pointerSquishTo * Mathf.Abs(direction.y), -pointerSquishTo * Mathf.Abs(direction.x), 1), pointerMoveTime)
                ).Play();
        }

        void FocusLocationFailedChange(Vector2Int directionFailed)
        {
            // Thought: Stretch the focus shape in a given direction, color it red for now
            if (activeFailedMovement != null) activeFailedMovement.Complete();

            activeFailedMovement = DOTween.Sequence()
                .Append(
                    pointerObject.DOPunchAnchorPos(directionFailed * 10, pointerFailedMovementTime)
                ).Join(
                    pointerImageComponent.DOColor(pointerFailedMovementColor, pointerFailedMovementTime * 0.6f)
                ).Join(
                    pointerObject.DOPunchScale(
                        new(-pointerSquishTo * Mathf.Abs(directionFailed.y) * 0.3f, -pointerSquishTo * Mathf.Abs(directionFailed.x) * 0.3f, 1),
                        pointerFailedMovementTime
                    )
                ).Insert(
                    pointerFailedMovementTime * 0.6f, 
                    pointerImageComponent.DOColor(pointerNormalColor, pointerFailedMovementTime * 0.7f)
                ).Play();
        }

        /// <summary>
        /// positions a piece in the given spot, assigning the canvas representation to the row transform and hiding the actual piece object
        /// does some extra effects too
        /// </summary>
        /// <param name="piece"></param>
        /// <param name="spot"></param>
        void PlacePieceInNewSpot(Piece piece, Vector2Int spot)
        {
            piece.transform.position = piecePositionWhileInInventory;

            // first - check if we have a row ready (y axis of "spot")
            while (_rowObjects.Count <= spot.y) // this means no, we don't, so gotta make a new row
            {
                GameObject row = new($"Row {_rowObjects.Count}", typeof(RectTransform));
                RectTransform rowTransform = row.GetComponent<RectTransform>();
                rowTransform.SetParent(inventoryCanvas, false);

                // set the position of the row based on the y axis of spot; it should be perfectly halfway on inventory canvas and some amount down
                float anchorY = 1f - (spot.y + 0.5f) / maxRowsDisplayedAtOnce;
                rowTransform.anchorMin = new(minRowAnchorX, anchorY - 0.5f * rowAnchorYHeight);
                rowTransform.anchorMax = new(maxRowAnchorX, anchorY + 0.5f * rowAnchorYHeight);
                _rowObjects.Add(rowTransform);
            }
            RectTransform canvasPieceTransform = (RectTransform)piece.GetCanvasPiece().transform;
            canvasPieceTransform.SetParent(_rowObjects[spot.y].transform, false);
            canvasPieceTransform.localScale = Vector2.one * pieceScalingMultiplier;

            // then set the anchor position based on the number of objects we can have in a row, so that they are equally spaced apart
            float anchorX = (spot.x + 0.5f) / columnsDisplayedAtOnce;
            canvasPieceTransform.anchorMin = canvasPieceTransform.anchorMax = new(anchorX, 0.5f);
        }

        void PieceTakenOut(Piece piece)
        {
            // Thought: Shrink the piece's canvas representation.
        }

        #endregion
    }
}