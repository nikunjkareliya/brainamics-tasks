using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    /// <summary>
    /// Generic object pool for any Component type. Subclass with a concrete
    /// type to use in the Inspector (e.g. public class MyPool : ObjectPool&lt;MyView&gt; { }).
    /// Automatically calls IPoolable callbacks on items that implement it.
    /// </summary>
    public abstract class ObjectPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T _prefab;
        [SerializeField] private Transform _container;
        [SerializeField] private int _preWarmCount = 3;

        private readonly Queue<T> _pool = new Queue<T>();
        private readonly List<T> _active = new List<T>();

        /// <summary>
        /// Pre-instantiates items into the pool for immediate use.
        /// </summary>
        public void PreWarm()
        {
            for (int i = 0; i < _preWarmCount; i++)
            {
                T item = Instantiate(_prefab, _container);
                item.gameObject.SetActive(false);
                _pool.Enqueue(item);
            }
        }

        /// <summary>
        /// Gets an item from the pool or creates a new one if the pool is empty.
        /// </summary>
        public T Get()
        {
            T item = _pool.Count > 0 ? _pool.Dequeue() : Instantiate(_prefab, _container);
            item.gameObject.SetActive(true);
            _active.Add(item);

            if (item is IPoolable poolable)
                poolable.OnPoolGet();

            return item;
        }

        /// <summary>
        /// Returns an item to the pool for reuse.
        /// </summary>
        public void Return(T item)
        {
            if (item == null) return;

            if (item is IPoolable poolable)
                poolable.OnPoolReturn();

            item.gameObject.SetActive(false);
            _active.Remove(item);
            _pool.Enqueue(item);
        }

        /// <summary>
        /// Returns all active items to the pool.
        /// </summary>
        public void ReturnAll()
        {
            for (int i = _active.Count - 1; i >= 0; i--)
            {
                T item = _active[i];

                if (item is IPoolable poolable)
                    poolable.OnPoolReturn();

                item.gameObject.SetActive(false);
                _pool.Enqueue(item);
            }
            _active.Clear();
        }

        private void OnDestroy()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                if (_active[i] != null)
                    Destroy(_active[i].gameObject);
            }
            _active.Clear();

            while (_pool.Count > 0)
            {
                T item = _pool.Dequeue();
                if (item != null)
                    Destroy(item.gameObject);
            }
        }
    }
}
