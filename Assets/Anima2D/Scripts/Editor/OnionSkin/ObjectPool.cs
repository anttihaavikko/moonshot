using System.Collections.Generic;
using UnityEngine;

namespace Anima2D.Pool
{
    public interface ICreationPolicy<T>
    {
        T Create();
        void Destroy(T o);
    }

    public class DefaultCreationPolicy<T> : ICreationPolicy<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }

        public virtual void Destroy(T o)
        {
        }
    }

    public class InstantiateCreationPolicy<T> : ICreationPolicy<T> where T : Object
    {
        public InstantiateCreationPolicy(T _original)
        {
            original = _original;
        }

        public T original { get; }

        public virtual T Create()
        {
            if (original) return Object.Instantiate(original);

            return null;
        }

        public virtual void Destroy(T o)
        {
            if (o) Object.DestroyImmediate(o);
        }
    }

    public abstract class ObjectPool<T>
    {
        private readonly ICreationPolicy<T> m_CreationPolicy;

        protected ObjectPool()
        {
            availableObjects = new List<T>();
            dispatchedObjects = new List<T>();
        }

        public ObjectPool(ICreationPolicy<T> _creationPolicy) : this()
        {
            m_CreationPolicy = _creationPolicy;
        }

        public List<T> availableObjects { get; }

        public List<T> dispatchedObjects { get; }

        public T Get()
        {
            T l_instance = default;

            if (availableObjects.Count == 0)
            {
                l_instance = m_CreationPolicy.Create();
            }
            else
            {
                l_instance = availableObjects[availableObjects.Count - 1];
                availableObjects.Remove(l_instance);
            }

            dispatchedObjects.Add(l_instance);

            return l_instance;
        }

        public void Return(T instance)
        {
            if (instance != null && dispatchedObjects.Contains(instance))
            {
                dispatchedObjects.Remove(instance);
                availableObjects.Add(instance);
            }
        }

        public void ReturnAll()
        {
            while (dispatchedObjects.Count > 0) Return(dispatchedObjects[dispatchedObjects.Count - 1]);
        }

        public void Clear()
        {
            ReturnAll();

            for (var i = 0; i < availableObjects.Count; i++)
            {
                var l_obj = availableObjects[i];

                if (l_obj != null) m_CreationPolicy.Destroy(l_obj);
            }

            availableObjects.Clear();
            dispatchedObjects.Clear();
        }
    }

    public class DefaultObjectPool<T> : ObjectPool<T> where T : new()
    {
        public DefaultObjectPool() : base(new DefaultCreationPolicy<T>())
        {
        }
    }
}