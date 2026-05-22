using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace UIFX
{
    public class ButtonDecorationController : IUITransitionElement, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Animation")]
        [SerializeField] private float fadeInTime = 0.3f;
        [SerializeField] private float fadeOutTime = 0.3f;
        [SerializeField] private Color fadeInColor;
        [SerializeField] private Color fadedOutColor;
        [SerializeField] private float hoveredScale = 1.15f;
        [SerializeField] private float transitionMoveDistance = 10f;
        [Header("Object config")]
        [SerializeField] private Image backdropImage;
        [SerializeField] private Button button;
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

        void Reset()
        {
            Debug.Log("Reset running on " + gameObject.name);
            if (!TryGetComponent(out backdropImage)) Debug.LogWarning("Failed to retrieve image reference for button deco controller: " + name);
            if (!TryGetComponent(out button)) Debug.LogWarning("Failed to retrieve button for buttondecorationcontroller: " + name);
            labelTMP = GetComponentInChildren<TMP_Text>();
            if (labelTMP == null) Debug.LogWarning("Failed to retrieve labelTMP for button controller: " + name);
        }

        void OnEnable()
        {
            backdropImage.color = fadedOutColor;
        }

        public void OnPointerEnter(PointerEventData ped)
        {
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
            if (fadeInSequence != null && fadeInSequence.active) { fadeInSequence.Kill(); }
            fadeInSequence = null;

            fadeOutSequence = DOTween.Sequence().Append(
                backdropImage.DOColor(fadedOutColor, fadeOutTime)
            ).Join(
                transform.DOScale(Vector3.one, fadeOutTime)
            );
        }

        public override void Enter(UnityAction<IUITransitionElement> enterComplete)
        {
            Debug.Log(name);
            if (exitTransitionSequence != null && exitTransitionSequence.active) exitTransitionSequence.Complete();
            exitTransitionSequence = null;

            var c = labelTMP.color; labelTMP.color = new Color(c.r, c.g, c.b, 0f);

            enterTransitionSequence = DOTween.Sequence()
                .Append(
                    transform.DOLocalMoveY(originalLocalPosition.y, fadeInTime)
                ).Join(
                    labelTMP.DOColor(new Color(c.r, c.g, c.b, 1f), fadeInTime)
                )
                .OnComplete(() => { button.interactable = true; enterComplete.Invoke(this); }).Play();
        }

        public override void Exit(UnityAction<IUITransitionElement> exitComplete)
        {
            Debug.Log("exit" + name);
            if (enterTransitionSequence != null && enterTransitionSequence.active) enterTransitionSequence.Complete();
            enterTransitionSequence = null;

            var c = labelTMP.color;

            exitTransitionSequence = DOTween.Sequence().Append(
                    backdropImage.DOColor(fadedOutColor, fadeOutTime)
                ).Join(
                    transform.DOLocalMoveY(originalLocalPosition.y - transitionMoveDistance, fadeOutTime)
                ).Join(
                    labelTMP.DOColor(new Color(c.r, c.g, c.b, 0f), fadeOutTime)
                ).OnComplete(() => { button.interactable = false; exitComplete.Invoke(this); } ).Play();
        }

        public override void FastKill()
        {
            fadeInSequence?.Kill(); fadeInSequence = null;
            fadeOutSequence?.Kill(); fadeOutSequence = null;
            enterTransitionSequence?.Kill(); enterTransitionSequence = null;
            exitTransitionSequence?.Kill(); exitTransitionSequence  = null;
        }
    }
}