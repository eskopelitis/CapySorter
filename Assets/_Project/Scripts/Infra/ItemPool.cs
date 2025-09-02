using UnityEngine;
using CapySorter.Core;

namespace CapySorter.Infra
{
    public class ItemPool : MonoBehaviour
    {
        [System.Serializable]
        public struct Entry { public ItemType type; public GameObject prefab; public int prewarm; }

        [SerializeField] private Entry[] _entries;
        private Pool[] _pools;

        public void Prewarm(int count = 40)
        {
            if (_entries == null) return;
            _pools = new Pool[_entries.Length];
            for (int i = 0; i < _entries.Length; i++)
            {
                int c = Mathf.Max(count, _entries[i].prewarm);
                _pools[i] = new Pool(_entries[i].prefab, c, transform);
            }
        }

        public GameObject Get(ItemType t)
        {
            for (int i = 0; i < _entries.Length; i++)
            {
                if (_entries[i].type == t) return _pools[i].Get();
            }
            return null;
        }

        public void Release(GameObject go)
        {
            if (!go) return;
            go.SetActive(false);
            go.transform.SetParent(transform, false);
        }

        private class Pool
        {
            private readonly GameObject _prefab;
            private readonly GameObject[] _ring;
            private int _head;
            private readonly Transform _parent;

            public Pool(GameObject prefab, int capacity, Transform parent)
            {
                _prefab = prefab;
                _parent = parent;
                if (capacity < 1) capacity = 1;
                _ring = new GameObject[capacity];
                for (int i = 0; i < capacity; i++)
                {
                    _ring[i] = Object.Instantiate(_prefab, parent);
                    _ring[i].SetActive(false);
                }
            }

            public GameObject Get()
            {
                // ring buffer
                for (int n = 0; n < _ring.Length; n++)
                {
                    _head = (_head + 1) % _ring.Length;
                    if (_ring[_head] != null && !_ring[_head].activeSelf)
                    {
                        var go = _ring[_head];
                        go.transform.SetParent(null, false);
                        go.SetActive(true);
                        return go;
                    }
                }
                // Fallback: expand one (rare)
                var extra = Object.Instantiate(_prefab, _parent);
                extra.SetActive(true);
                extra.transform.SetParent(null, false);
                return extra;
            }
        }
    }
}
