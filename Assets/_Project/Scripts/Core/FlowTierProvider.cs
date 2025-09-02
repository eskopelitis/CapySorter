using System;
using UnityEngine;

namespace CapySorter.Core
{
    public class FlowTierProvider : MonoBehaviour
    {
        [SerializeField] private int _currentTier = 1; // 1..5
        [SerializeField] private int _pips;

        public int CurrentTier => _currentTier;
        public event Action<int> OnTierChanged;

        const int MinTier = 1;
        const int MaxTier = 5;

        public void ResetAll()
        {
            _pips = 0;
            SetTier(1);
        }

        public void AddPips(int count)
        {
            if (count == 0) return;
            _pips = Math.Max(0, _pips + count);
            // Tier up every 5 pips
            int desiredTier = Mathf.Clamp(1 + (_pips / 5), MinTier, MaxTier);
            if (desiredTier != _currentTier)
                SetTier(desiredTier);
        }

        public void ResetOnBomb()
        {
            _pips = 0;
            SetTier(1);
        }

        public void Contamination()
        {
            // −3 pips and tier−1
            _pips = Math.Max(0, _pips - 3);
            SetTier(Mathf.Clamp(_currentTier - 1, MinTier, MaxTier));
        }

        public void AddTierDirect(int delta)
        {
            SetTier(Mathf.Clamp(_currentTier + delta, MinTier, MaxTier));
        }

        private void SetTier(int t)
        {
            if (t == _currentTier) return;
            _currentTier = t;
            OnTierChanged?.Invoke(_currentTier);
        }
    }
}
