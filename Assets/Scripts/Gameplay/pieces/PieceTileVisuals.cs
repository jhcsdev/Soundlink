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

        #region active DOTweens
        Tween activePulseTween = null;

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
        }
        void OnDisable()
        {
            pieceTile.OnPlaced -= OnPlacedHappened;
            pieceTile.OnHover -= OnHovering;
            pieceTile.OnPickedUp -= OnPickup;
            pieceTile.OnColorPulse -= OnColorPulse;
            pieceTile.OnSetColor -= OnSetColor; 
            pieceTile.OnChangeSpriteRendererIndex -= ChangeRendererIndex;
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
    }
}