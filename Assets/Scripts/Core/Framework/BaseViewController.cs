using UnityEngine;

namespace Game.Framework
{
    [DefaultExecutionOrder(-10)]
    public abstract class BaseViewController : MonoBehaviour
    {
        [SerializeField] protected BaseView _viewObject;

        private void Awake()
        {
            Init();
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        protected virtual void Init() { }
        protected virtual void Subscribe() { }
        protected virtual void Unsubscribe() { }
    }
}
