using System;
using UnityEngine;
using CapySorter.Core;
using CapySorter.Util;
using CapySorter.Infra;

namespace CapySorter.Gameplay
{
    public class CenterDropSpawner : MonoBehaviour
    {
        public event Action<ItemType, Vector2> OnItemSpawn;

        [Header("World Config")]
        public float SpawnX = -3.0f;
        public float BeltYCenter = 0f;
        public float LaneSeparation = 1.2f; // lanes on y = {+sep, 0, -sep}

        [Header("Refs")]
        [SerializeField] private ItemPool _pool;
        [SerializeField] private FlowTierProvider _tier;

        private XorShift32 _rng;
        private float _time;
        private float _nextSpawnAt;
        private int _spawnIndex;
        private int _sinceLastBomb = 1000;

        private static readonly float[] Speeds = { 1.0f, 1.3f, 1.6f, 1.9f, 2.2f };
        private static readonly float[] Factors = { 0.55f, 0.50f, 0.45f, 0.40f, 0.40f };
        private const float BeltLen = 12f;

    public void Init(int seed)
        {
            _rng = new XorShift32((uint)seed);
            _time = 0f;
            _nextSpawnAt = 0f;
            _spawnIndex = 0;
            _sinceLastBomb = 1000;
        }

        public void Tick(float dt)
        {
            _time += dt;
            if (_time + 0.001f < _nextSpawnAt) return;
            // compute next interval based on tier
            int tierIdx = Mathf.Clamp(_tier.CurrentTier, 1, 5) - 1;
            float speed = Speeds[tierIdx];
            float baseInterval = BeltLen / speed * Factors[tierIdx];
            float jitter = baseInterval * (0.95f + 0.1f * _rng.NextFloat()); // ±5%
            _nextSpawnAt = _time + jitter;

            var type = PickType(_spawnIndex);
            Spawn(_spawnIndex, type);
            _spawnIndex++;
        }

        public ItemType PickType(int index)
        {
            // deterministic using rng state advanced in a predictable way
            // Keep bomb gap >= 4 normals between
            if (_sinceLastBomb >= 4)
            {
                // ~10% chance for bomb when gap satisfied
                if (_rng.NextInt(0, 10) == 0)
                {
                    _sinceLastBomb = 0;
                    return ItemType.Bomb;
                }
            }
            _sinceLastBomb++;
            int r = _rng.NextInt(0, 3);
            return (ItemType)r; // Recycle/Compost/Trash
        }

        public void Spawn(int index, ItemType type)
        {
            // lane selection deterministic by index
            int lane = index % 3; // 0..2
            float y = BeltYCenter + (lane - 1) * LaneSeparation; // lanes: top, mid, bottom -> +sep,0,-sep
            Vector2 pos = new Vector2(SpawnX, y);

            // Notify mirror systems deterministically regardless of pooling
            OnItemSpawn?.Invoke(type, pos);

            var go = _pool != null ? _pool.Get(type) : null;
            if (go != null)
            {
                go.transform.position = pos;
                var rider = go.GetComponent<ConveyorRider2D>();
                if (rider)
                {
                    int tierIdx = Mathf.Clamp(_tier.CurrentTier, 1, 5) - 1;
                    float speed = Speeds[tierIdx];
                    rider.SetSpeed(speed);
                    rider.Enable();
                }
            }
            AnalyticsBridge.Log("spawn", ("i", index), ("type", type.ToString()), ("x", pos.x), ("y", pos.y));
        }
    }
}
