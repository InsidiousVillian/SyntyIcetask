using System.Collections.Generic;
using UnityEngine;

namespace GnomeGuard
{
    public sealed class RuntimePool<T> where T : Component
    {
        readonly Stack<T> _free = new Stack<T>(32);
        readonly Transform _parent;
        readonly System.Func<T> _factory;

        public RuntimePool(Transform parent, System.Func<T> factory)
        {
            _parent = parent;
            _factory = factory;
        }

        public T Get()
        {
            T item = _free.Count > 0 ? _free.Pop() : _factory();
            if (item == null) item = _factory();
            item.gameObject.SetActive(true);
            return item;
        }

        public void Release(T item)
        {
            if (item == null) return;
            item.gameObject.SetActive(false);
            if (_parent != null) item.transform.SetParent(_parent, false);
            _free.Push(item);
        }

        public void Clear()
        {
            _free.Clear();
        }
    }

    public static class ActorFolder
    {
        public static Transform Create(Transform root, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(root, false);
            return go.transform;
        }

        public static void Clear(Transform folder)
        {
            if (folder == null) return;
            for (int i = folder.childCount - 1; i >= 0; i--)
            {
                var child = folder.GetChild(i).gameObject;
                child.SetActive(false);
                Object.Destroy(child);
            }
        }
    }

    public static class WaitCache
    {
        static readonly Dictionary<int, WaitForSeconds> Map = new Dictionary<int, WaitForSeconds>(32);

        public static WaitForSeconds Seconds(float duration)
        {
            int key = Mathf.Max(1, Mathf.RoundToInt(duration * 20f));
            if (!Map.TryGetValue(key, out var wait))
            {
                wait = new WaitForSeconds(key * 0.05f);
                Map[key] = wait;
            }

            return wait;
        }
    }
}
