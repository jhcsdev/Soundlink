using DG.Tweening;
using PuzzleGrid;
using UnityEngine;

namespace GamePieces
{
    [RequireComponent(typeof(PieceTile), typeof(SpriteRenderer))]
    public class PieceTileVisuals : MonoBehaviour
    {
        private static float colorPulseTime = 1;
        private PieceTile pieceTile;
        private SpriteRenderer spriteRenderer;
        private Color baseColor;

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
            pieceTile.OnColorPulse += OnLinkPulse;
            pieceTile.OnSetColor += OnSetColor;
        }
        void OnDisable()
        {
            pieceTile.OnPlaced -= OnPlacedHappened;
            pieceTile.OnHover -= OnHovering;
            pieceTile.OnPickedUp -= OnPickup;
            pieceTile.OnColorPulse -= OnLinkPulse;
            pieceTile.OnSetColor -= OnSetColor;
        }

        void OnPlacedHappened(GridTile gridTile)
        {
            transform.SetParent(gridTile.transform);
            transform.localPosition = Vector2.zero;
            transform.localScale = Vector2.one;
            spriteRenderer.color = new(1, 1, 1, 1f); // todo:: hardset color...?
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

        void OnLinkPulse(Color c)
        {
            Debug.Log($"Pulsing to {c} from {baseColor} over {colorPulseTime}");
            spriteRenderer.color = c;
            spriteRenderer.DOColor(baseColor, colorPulseTime);
        }
        void OnSetColor(Color c, float time)
        {
            Debug.Log($"Setting color to: {c}, {time}");
            spriteRenderer.color = c;
            baseColor = c;
            // spriteRenderer.DOColor(c, time);
        }
    }
}