using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace UIFX
{
    public class ButtonDecorationController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IUITransitionElement
    {
        [Header("Animation")]
        [SerializeField] private float fadeInTime;
        [SerializeField] private float fadeOutTime;
        [SerializeField] private Color fadeInColor;
        [SerializeField] private Color fadedOutColor;
        [SerializeField] private Color underlineColor;
        [SerializeField] private float hoveredScale;
        [SerializeField] private float transitionMoveDistance = 10f;
        [Header("Object config")]
        [SerializeField] private Image backdropImage;
        [SerializeField] private TMP_Text labelTMP;

        #region sequences
        private Sequence fadeInSequence;
        private Sequence fadeOutSequence;
        private Sequence enterTransitionSequence;
        private Sequence exitTransitionSequence;
        #endregion
        private Vector3 originalLocalPosition;

        void Awake()
        {
            originalLocalPosition = transform.localPosition;
        }

        void OnEnable()
        {
            backdropImage.color = fadedOutColor;
        }

        public void OnPointerEnter(PointerEventData ped)
        {
            // Debug.Log("hello hovering over " + gameObject.name);
            if (fadeOutSequence != null && fadeOutSequence.active) { fadeOutSequence.Kill(); }
            fadeOutSequence = null;

            fadeInSequence = DOTween.Sequence().Append(
                backdropImage.DOColor(fadeInColor, fadeInTime)
            ).Join(
                transform.DOScale(hoveredScale, fadeInTime)
            );
        }
        public void OnPointerExit(PointerEventData ped)
        {
            // Debug.Log("And that's an exit");
            if (fadeInSequence != null && fadeInSequence.active) { fadeInSequence.Kill(); }
            fadeInSequence = null;

            fadeOutSequence = DOTween.Sequence().Append(
                backdropImage.DOColor(fadedOutColor, fadeOutTime)
            ).Join(
                transform.DOScale(Vector3.one, fadeOutTime)
            );
        }

        public void Enter(UnityAction<IUITransitionElement> enterComplete)
        {
            if (exitTransitionSequence != null && exitTransitionSequence.active) exitTransitionSequence.Complete();
            exitTransitionSequence = null;

            var c = labelTMP.color; labelTMP.color = new Color(c.r, c.g, c.b, 0f);

            enterTransitionSequence = DOTween.Sequence()
                .Append(
                    transform.DOLocalMoveY(originalLocalPosition.y, fadeInTime)
                ).Join(
                    labelTMP.DOColor(new Color(c.r, c.g, c.b, 1f), fadeInTime)
                )
                .OnComplete(() => enterComplete.Invoke(this)).Play();
                    
        }
        public void Exit(UnityAction<IUITransitionElement> exitComplete)
        {
            if (enterTransitionSequence != null && enterTransitionSequence.active) enterTransitionSequence.Complete();
            enterTransitionSequence = null;

            var c = labelTMP.color;

            exitTransitionSequence = DOTween.Sequence().Append(
                    backdropImage.DOColor(fadedOutColor, fadeOutTime)
                ).Join(
                    transform.DOLocalMoveY(originalLocalPosition.y - transitionMoveDistance, fadeOutTime)
                ).Join(
                    labelTMP.DOColor(new Color(c.r, c.g, c.b, 0f), fadeOutTime)
                ).OnComplete(() => exitComplete.Invoke(this)).Play();
        }
    }
}