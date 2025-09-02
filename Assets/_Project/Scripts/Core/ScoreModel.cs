using System;
using UnityEngine;

namespace CapySorter.Core
{
    [Serializable]
    public class ScoreModel
    {
        public int Score { get; private set; }
        public int Streak { get; private set; }

        public event Action OnScoreChanged;

        public void Reset()
        {
            Score = 0;
            Streak = 0;
            OnScoreChanged?.Invoke();
        }

        public void AddCorrect(bool perfect)
        {
            Streak += perfect ? 2 : 1; // Flow pips are handled at FlowTierProvider; Streak mirrors pips for tests
            Score += perfect ? 2 : 1;
            OnScoreChanged?.Invoke();
        }

        public void AddContamination()
        {
            Streak = Math.Max(0, Streak - 3);
            Score -= 2;
            OnScoreChanged?.Invoke();
        }

        public void BombExplode()
        {
            Streak = 0;
            Score -= 4;
            OnScoreChanged?.Invoke();
        }

        public void BombDefuse()
        {
            // +3 and +1 tier handled externally
            Score += 3;
            Streak += 0; // no pip change here; tier system will add +1 tier
            OnScoreChanged?.Invoke();
        }

        // Internal utility for pressure rule and admin adjustments
        public void AdjustScore(int delta)
        {
            Score += delta;
            OnScoreChanged?.Invoke();
        }
    }
}
