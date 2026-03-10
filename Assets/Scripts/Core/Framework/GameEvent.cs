using System;

namespace Game.Framework
{
    public abstract class GameEventBase<TDelegate> where TDelegate : Delegate
    {
        private TDelegate _action;

        public void Subscribe(TDelegate listener)
        {
            if (listener == null) return;
            _action = (TDelegate)Delegate.Combine(_action, listener);
        }

        public void Unsubscribe(TDelegate listener)
        {
            if (listener == null) return;
            _action = (TDelegate)Delegate.Remove(_action, listener);
        }

        protected void PublishInternal(Action<TDelegate> invoker)
        {
            if (_action == null) return;

            var listeners = _action.GetInvocationList();
            for (int i = 0; i < listeners.Length; i++)
            {
                try
                {
                    invoker((TDelegate)listeners[i]);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"Exception in GameEvent listener: {ex.Message}\n{ex.StackTrace}");
                }
            }
        }

        public void Clear()
        {
            _action = null;
        }
    }

    public class GameEvent : GameEventBase<Action>
    {
        public void Publish()
        {
            PublishInternal(action => action.Invoke());
        }
    }

    public class GameEvent<T> : GameEventBase<Action<T>>
    {
        public void Publish(T param)
        {
            PublishInternal(action => action.Invoke(param));
        }
    }
}
