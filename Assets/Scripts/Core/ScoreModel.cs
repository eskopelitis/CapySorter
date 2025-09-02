using System;
using NeonShift.Meta;

namespace NeonShift.Core
{
    [Serializable]
    public class ScoreModel
    {
        public int Score { get; private set; }
        public int Streak { get; private set; }
        public event Action OnScoreChanged;

        public void Reset()
        { Score = 0; Streak = 0; OnScoreChanged?.Invoke(); AnalyticsBridge.Log("combo_change", ("streak", Streak)); }

        public void AddCorrect(bool perfect)
        {
            Score += perfect ? 2 : 1;
            Streak += perfect ? 2 : 1;
            OnScoreChanged?.Invoke();
            AnalyticsBridge.Log("combo_change", ("streak", Streak));
        }

        public void AddContamination()
        {
            Score -= 2; // tier down handled externally
            if (Streak > 0) Streak = Streak >= 3 ? Streak - 3 : 0;
            OnScoreChanged?.Invoke();
            AnalyticsBridge.Log("combo_change", ("streak", Streak));
        }

        public void BombExplode()
        {
            Score -= 4;
            Streak = 0;
            OnScoreChanged?.Invoke();
            AnalyticsBridge.Log("bomb_explode", ("time", 0));
            AnalyticsBridge.Log("combo_change", ("streak", Streak));
        }

        public void BombDefuse()
        {
            Score += 3; // +1 tier externally
            OnScoreChanged?.Invoke();
            AnalyticsBridge.Log("bomb_defuse", ("time", 0));
        }

        // For pressure rule
        public void AdjustScore(int delta)
        { Score += delta; OnScoreChanged?.Invoke(); }
    }
}
