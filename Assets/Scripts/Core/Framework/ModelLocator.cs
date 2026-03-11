using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Framework
{
    public static class ModelLocator
    {
        private static readonly Dictionary<Type, object> _models = new Dictionary<Type, object>();

        public static void Register<T>(T model) where T : class
        {
            Type type = typeof(T);
            if (!_models.ContainsKey(type))
            {
                _models.Add(type, model);
                Debug.Log($"<color=cyan>{model}</color> is added to Collection!");
            }
            else
            {
                throw new Exception($"Model of type {type} is already registered.");
            }
        }

        public static T Get<T>() where T : class, new()
        {
            Type type = typeof(T);

            if (_models.TryGetValue(type, out var model))
            {
                return model as T;
            }

            T newModel = new T();
            Register(newModel);
            return newModel;
        }

        public static void Clear()
        {
            _models.Clear();
        }
    }
}
