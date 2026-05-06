using DG.Tweening;
using PuzzleGrid;
using UnityEngine;

namespace GamePieces
{
    [RequireComponent(typeof(PieceTile), typeof(SpriteRenderer))]
    public class PieceTileVisuals : MonoBehaviour
    {
        private PieceTile pieceTile;
        private SpriteRenderer spriteRenderer;
        private Color baseColor = Color.white;
        #region animation config
        [Header("rotation")]
        private static float spriteRotationTime = 0.25f; 
        private static float movementRotationTime = 0.25f;
        private static Ease rotationEaseMode = Ease.OutBack;
        #endregion

        #region active DOTweens
        Tween activePulseTween = null;
        Sequence activeRotationSequence = null;

        #endregion

        void Awake()
        {
            pieceTile = GetComponent<PieceTile>();
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            pieceTile.OnPlaced += OnPlacedHappened;
            pieceTile.OnHover += OnHovering;
            pieceTile.OnPickedUp += OnPickup;
            pieceTile.OnColorPulse += OnColorPulse;
            pieceTile.OnSetColor += OnSetColor;
            pieceTile.OnChangeSpriteRendererIndex += ChangeRendererIndex;
            pieceTile.OnRotate += RotateTile;
        }
        void OnDisable()
        {
            pieceTile.OnPlaced -= OnPlacedHappened;
            pieceTile.OnHover -= OnHovering;
            pieceTile.OnPickedUp -= OnPickup;
            pieceTile.OnColorPulse -= OnColorPulse;
            pieceTile.OnSetColor -= OnSetColor; 
            pieceTile.OnChangeSpriteRendererIndex -= ChangeRendererIndex;
            pieceTile.OnRotate -= RotateTile;
        }

        void OnPlacedHappened(GridTile gridTile)
        {
            // transform.SetParent(gridTile.transform);
            // transform.localPosition = Vector2.zero;
            // transform.localScale = Vector2.one;
        }
        void OnHovering()
        {
            spriteRenderer.color = new(1,1,1,0.4f); // todo:: hardset color...?
            transform.localScale = new(0.5f,0.5f,0.5f);
        }
        void OnPickup(Piece owningPiece, Vector2 localOffset)
        {
            transform.SetParent(owningPiece.transform);
            transform.localPosition = localOffset;
        }

        void OnColorPulse(Color c, float overTime)
        {
            Debug.Log($"Pulsing to {c} from {baseColor} over {overTime}");
            if (activePulseTween != null) activePulseTween.Kill();
            spriteRenderer.color = c;
            activePulseTween = spriteRenderer.DOColor(baseColor, overTime);
        }
        void OnSetColor(Color c, float time)
        {
            Debug.Log($"Setting color to: {c}, {time}");
            // Color mixCol = pieceTile.GetMixColor();
            if (activePulseTween != null) activePulseTween.Kill();
            spriteRenderer.color = c;
            baseColor = c;
            // spriteRenderer.DOColor(c, time);
        }

        void ChangeRendererIndex(int to)
        {
            spriteRenderer.sortingOrder = to;
        }

        void RotateTile(Vector2Int newLocation, Quaternion newRotation)
        {
            Vector3 currentPos = transform.localPosition;
            float radius = currentPos.magnitude;

            float startAngle = Mathf.Atan2(currentPos.y, currentPos.x);
            float targetAngle = Mathf.Atan2(newLocation.y, newLocation.x);

            float delta = Mathf.DeltaAngle(
                startAngle * Mathf.Rad2Deg,
                targetAngle * Mathf.Rad2Deg
            ) * Mathf.Deg2Rad;

            float currentAngle = startAngle;

            if (activeRotationSequence != null && activeRotationSequence.active) activeRotationSequence.Kill();

            activeRotationSequence = DOTween.Sequence().Join(
                DOTween.To(
                    () => currentAngle,
                    angle =>
                    {
                        currentAngle = angle;
                        transform.localPosition = new Vector3(
                            Mathf.Cos(angle) * radius,
                            Mathf.Sin(angle) * radius,
                            currentPos.z
                        );
                    },
                    startAngle + delta,
                    movementRotationTime
                ).SetEase(rotationEaseMode)
            ).Join(
                transform.DOLocalRotate(
                    newRotation.eulerAngles,
                    spriteRotationTime
                ).SetEase(rotationEaseMode)
            ).Play();
        }
    }
}