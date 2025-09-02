using System;
using UnityEngine;
using NeonShift.Items;
using NeonShift.Util;
using NeonShift.Meta;
using NeonShift.Infra;

namespace NeonShift.Core
{
    public class CenterDropSpawner : MonoBehaviour
    {
        public event Action<int, ItemType, bool, float> OnItemSpawned;

        private XorShift32 _rng;
        private float _beltLength;
        private FlowTierProvider _tier;
        private Transform _spawnRoot;
        private ItemPool _pool;
        private float _time;
        private float _nextSpawnAt;
        private int _spawnIndex;
        private int _sinceLastBomb;

        private static readonly float[] Speeds = {1.0f,1.3f,1.6f,1.9f,2.2f};
        private static readonly float[] Factors = {0.55f,0.50f,0.45f,0.40f,0.40f};

        public void Init(int matchSeed, float beltLength, FlowTierProvider tier, Transform spawnRoot, ItemPool pool)
        {
            _rng = new XorShift32((uint)matchSeed);
            _beltLength = beltLength;
            _tier = tier; _spawnRoot = spawnRoot; _pool = pool;
            _time = 0f; _nextSpawnAt = 0f; _spawnIndex = 0; _sinceLastBomb = 1000;
        }

        public void Tick(float dt)
        {
            _time += dt;
            if (_time + 0.0001f < _nextSpawnAt) return;

            int idx = Mathf.Clamp(_tier.CurrentTier, 1, 5) - 1;
            float T = _beltLength / Speeds[idx];
            float baseSi = T * Factors[idx];
            float jitter = baseSi * (0.95f + 0.1f * _rng.NextFloat01());
            _nextSpawnAt = _time + jitter;

            var type = PickType(_spawnIndex);
            Spawn(_spawnIndex, type);
            _spawnIndex++;
        }

        public ItemType PickType(int index)
        {
            if (_sinceLastBomb >= 4)
            {
                if (_rng.NextRange(0, 10) == 0) { _sinceLastBomb = 0; return ItemType.Bomb; }
            }
            _sinceLastBomb++;
            return (ItemType)_rng.NextRange(0, 3); // Recycle/Compost/Trash
        }

        public void Spawn(int index, ItemType type)
        {
            bool isBomb = type == ItemType.Bomb;
            OnItemSpawned?.Invoke(index, type, isBomb, _time);
            AnalyticsBridge.Log("item_spawn", ("index", index), ("type", type.ToString()), ("isBomb", isBomb), ("time", (int)(_time*1000)));
            if (isBomb) AnalyticsBridge.Log("bomb_spawn", ("index", index), ("time", (int)(_time*1000)));

            if (_pool == null || _spawnRoot == null) return;
            // Minimal visual: instantiate pooled prefab externally; here we just mark position
            // Left to scene wiring to fetch appropriate prefab
        }
    }
}
