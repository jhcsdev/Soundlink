using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UIFX
{
    [RequireComponent(typeof(Image))]
    public class ImageFadeInController : IUITransitionElement
    {
        [SerializeField] private Color fadeInColor = new(0.1f, 0.1f, 0.1f, 0.7f);
        [SerializeField] private Color fadeOutColor;
        [SerializeField] private float fadeTime;
        
        private Sequence currentSequence;
        private Image _image; 
        private Image TargetImage 
        {
            get 
            {
                if (_image == null) _image = GetComponent<Image>();
                return _image;
            }
        }

        public override void Enter(UnityAction<IUITransitionElement> onComplete)
        {
            if (currentSequence != null && currentSequence.active) currentSequence.Kill();
            currentSequence = null;  
            
            currentSequence = DOTween.Sequence()
                .Append(TargetImage.DOColor(fadeInColor, fadeTime))
                .OnComplete(() => { onComplete?.Invoke(this); })
                .Play();
        }

        public override void Exit(UnityAction<IUITransitionElement> onComplete)
        {
            if (currentSequence != null && currentSequence.active) currentSequence.Kill();
            currentSequence = null;

            currentSequence = DOTween.Sequence()
                .Append(TargetImage.DOColor(fadeOutColor, fadeTime))
                .OnComplete(() => { onComplete?.Invoke(this); })
                .Play();
        }

        public override void FastKill()
        {
            if (currentSequence != null && currentSequence.active) currentSequence.Kill();
            currentSequence = null;
        }
    }
}