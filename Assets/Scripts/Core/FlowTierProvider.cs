using System;
using UnityEngine;
using NeonShift.Meta;

namespace NeonShift.Core
{
    public class FlowTierProvider : MonoBehaviour
    {
        [SerializeField] private int _currentTier = 1; // 1..5
        [SerializeField] private int _pips;
        private static readonly float[] Speeds = {1.0f,1.3f,1.6f,1.9f,2.2f};
        private static readonly float[] Factors = {0.55f,0.50f,0.45f,0.40f,0.40f};

        public int CurrentTier { get => _currentTier; private set => _currentTier = value; }
        public event Action<int,string> OnTierChanged;

        public void AddPips(int count)
        {
            if (count == 0) return;
            int prev = _pips;
            _pips = Mathf.Max(0, _pips + count);
            AnalyticsBridge.Log("flow_pips_change", ("pipsDelta", _pips - prev), ("pipsTotal", _pips));
            int desired = Mathf.Clamp(1 + (_pips / 5), 1, 5);
            if (desired != _currentTier) SetTier(desired, "flow_up");
        }

        public void ResetOnBomb()
        {
            _pips = 0;
            SetTier(1, "bomb_reset");
            AnalyticsBridge.Log("flow_pips_change", ("pipsDelta", 0), ("pipsTotal", _pips));
        }

        public void Contamination()
        {
            int before = _pips;
            _pips = Mathf.Max(0, _pips - 3);
            AnalyticsBridge.Log("flow_pips_change", ("pipsDelta", _pips - before), ("pipsTotal", _pips));
            SetTier(Mathf.Clamp(_currentTier - 1, 1, 5), "mistake_down");
        }

        public float CurrentSpeed() { return Speeds[Mathf.Clamp(_currentTier,1,5)-1]; }
        public float CurrentSiFactor() { return Factors[Mathf.Clamp(_currentTier,1,5)-1]; }

        private void SetTier(int t, string reason)
        {
            if (t == _currentTier) return;
            _currentTier = t;
            OnTierChanged?.Invoke(_currentTier, reason);
            AnalyticsBridge.Log("tier_change", ("tier", _currentTier), ("reason", reason));
        }
    }
}
