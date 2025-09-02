using UnityEngine;
using CapySorter.Core;
using CapySorter.Util;

namespace CapySorter.Gameplay
{
    public class RivalSimulator : MonoBehaviour
    {
        private XorShift32 _rng;
        private float _accuracy;
        private float _reactDelay;
        private int _score;

        public int CurrentScore => _score;

        public void Init(int seed, float accuracy = 0.85f, float reactDelayMs = 300f)
        {
            _rng = new XorShift32((uint)seed ^ 0xA5A5A5A5u);
            _accuracy = Mathf.Clamp01(accuracy);
            _reactDelay = reactDelayMs * 0.001f;
            _score = 0;
        }

        public void OnSpawn(ItemType type)
        {
            // simulate after delay
            if (type == ItemType.Bomb)
            {
                // rival defuses with small probability, otherwise explode
                bool defuse = _rng.NextFloat() < 0.2f;
                if (defuse) _score += 3; else _score -= 4;
                return;
            }

            bool correct = _rng.NextFloat() < _accuracy;
            if (correct) _score += 1;
            else _score -= 2;
        }
    }
}
