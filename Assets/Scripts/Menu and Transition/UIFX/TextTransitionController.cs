using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace UIFX
{
    [RequireComponent(typeof(TMP_Text))]
    public class TextTransitionController : IUITransitionElement
    {
        [SerializeField] private float fadeInTime  = 0.3f;
        [SerializeField] private float fadeOutTime = 0.2f;
        [SerializeField] private Color fadedInColor = Color.white;
        [SerializeField] private Color fadedOutColor = new(1, 1, 1, 0);

        [SerializeField] private TMP_Text label;
        private Sequence enterSequence;
        private Sequence exitSequence;

        void Awake()
        {
            label = GetComponent<TMP_Text>();
        }
        void Reset()
        {
            if (!TryGetComponent(out label)) Debug.LogWarning($"{name} failed to get label text");
        } 

        void OnEnable()
        {
            var c = label.color;
            label.color = new Color(c.r, c.g, c.b, 0f);
        }

        public override void Enter(UnityAction<IUITransitionElement> onComplete)
        {
            Debug.Log($"{name} enter");
            exitSequence?.Kill();
            exitSequence = null;

            enterSequence = DOTween.Sequence()
                .Append(label.DOColor(fadedInColor, fadeInTime))
                .OnComplete(() => onComplete?.Invoke(this))
                .Play();
        }

        public override void Exit(UnityAction<IUITransitionElement> onComplete)
        {
            enterSequence?.Kill();
            enterSequence = null;

            exitSequence = DOTween.Sequence()
                .Append(label.DOColor(fadedOutColor, fadeOutTime))
                .OnComplete(() => onComplete?.Invoke(this))
                .Play();
        }

        public override void FastKill()
        {
            enterSequence?.Kill(); enterSequence = null;
            exitSequence?.Kill();  exitSequence  = null;
        }
    }
}