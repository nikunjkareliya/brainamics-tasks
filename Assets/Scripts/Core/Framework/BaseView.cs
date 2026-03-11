using DG.Tweening;
using UnityEngine;

namespace Game.Framework
{
    public abstract class BaseView : MonoBehaviour
    {
        [SerializeField] protected CanvasGroup _canvasGroup;
        [SerializeField] protected float _transitionSpeed = 0.25f;

        private Tween _fadeTween;

        public virtual void Show()
        {
            if (_canvasGroup == null) return;

            _fadeTween?.Kill();
            _fadeTween = _canvasGroup.DOFade(1f, _transitionSpeed)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                });
        }

        public virtual void Hide(bool isInstant = false)
        {
            if (_canvasGroup == null) return;

            _fadeTween?.Kill();

            if (isInstant)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                return;
            }

            _fadeTween = _canvasGroup.DOFade(0f, _transitionSpeed)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                });
        }

        protected virtual void OnDestroy()
        {
            _fadeTween?.Kill();
        }
    }
}
