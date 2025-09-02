using UnityEngine;

namespace NeonShift.Infra
{
    public class ItemPool : MonoBehaviour
    {
        [System.Serializable]
        public struct Entry { public string key; public GameObject prefab; public int prewarm; }
        [SerializeField] private Entry[] _entries;
        private GameObject[] _ring; private int _head;

        public void Prewarm(int count = 40)
        {
            if (count < 1) count = 1; _ring = new GameObject[count]; _head = 0;
            for (int i = 0; i < count; i++)
            { _ring[i] = null; }
        }

        public GameObject Get(GameObject prefab)
        {
            for (int i = 0; i < _ring.Length; i++)
            {
                int idx = (_head + i) % _ring.Length;
                var go = _ring[idx];
                if (go != null && !go.activeSelf)
                { _head = idx; go.SetActive(true); go.transform.SetParent(null, false); return go; }
            }
            var inst = Instantiate(prefab);
            inst.SetActive(true); inst.transform.SetParent(null, false); return inst;
        }

        public void Return(GameObject go)
        {
            if (!go) return; go.SetActive(false); go.transform.SetParent(transform, false);
            _ring[_head] = go; _head = (_head + 1) % _ring.Length;
        }
    }
}
