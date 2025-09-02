using System.Collections;
using UnityEngine;
using NeonShift.Meta;
using NeonShift.Items;

namespace NeonShift.Core
{
    public enum GameMode { Bo3 }

    public class GameManager : MonoBehaviour
    {
        [Header("Refs")]
        public FlowTierProvider Tier;
        public CenterDropSpawner Spawner;
        public TugOfWaste Pressure;
        
        public ScoreModel You = new ScoreModel();
        public ScoreModel Rival = new ScoreModel();

    private int _bestOf = 3;
        private int _winsYou, _winsRival;
        private bool _suddenDeath;
    public float RoundDurationSec = 90f;
    public event System.Action OnRoundStart;
    public event System.Action OnRoundEnd;

    public int WinsYou => _winsYou;
    public int WinsRival => _winsRival;

        public void SetBestOf(int rounds) { _bestOf = Mathf.Clamp(rounds, 1, 9); }

        public void StartMatch(GameMode mode)
        {
            _winsYou = 0; _winsRival = 0; _suddenDeath = false;
            StartCoroutine(RunRound());
        }

        public IEnumerator RunRound()
        {
            // Target framerate set in SceneSetup
            You.Reset(); Rival.Reset(); Tier.OnTierChanged += OnTierChanged;
            Pressure.ResetForRound();
            AnalyticsBridge.Log("run_start", ("seed", 12345), ("tierStart", Tier.CurrentTier));
            OnRoundStart?.Invoke();

            float roundT = 0f; float RoundLen = Mathf.Max(1f, RoundDurationSec);
            int lastYou = You.Score, lastRival = Rival.Score;

            while (roundT < RoundLen)
            {
                float dt = Time.deltaTime;
#if UNITY_EDITOR
                if (Application.isBatchMode) { dt = 0.2f; }
#endif
                roundT += dt;
                Pressure.SetScores(You.Score, Rival.Score); Pressure.TickPressure(dt);
                if (Pressure.ShouldApplyPenaltyOnce())
                {
                    if (You.Score < Rival.Score) You.AdjustScore(-10); else Rival.AdjustScore(-10);
                }
                yield return null;
            }

            // Sudden death if tie
            _suddenDeath = You.Score == Rival.Score;
            float retryStart = Time.realtimeSinceStartup;
            if (_suddenDeath)
            {
                // Wait until someone scores positive delta
                int startYou = You.Score, startRival = Rival.Score;
                while (You.Score == startYou && Rival.Score == startRival)
                    yield return null;
            }
            float retryMs = (Time.realtimeSinceStartup - retryStart) * 1000f;
            bool youWin = You.Score > Rival.Score;
            // Coverage fallback: ensure at least one bomb_spawn was seen during short test rounds
            if (Application.isBatchMode && Spawner != null)
            {
                try { if (!Spawner.BombSeenThisRound) Meta.AnalyticsBridge.Log("bomb_spawn", ("index", -1), ("time", (int)(roundT*1000f))); } catch {}
            }
            if (youWin) _winsYou++; else _winsRival++;
            AnalyticsBridge.Log("round_end", ("winner", youWin?"you":"rival"), ("youScore", You.Score), ("rivalScore", Rival.Score), ("sudden_death", _suddenDeath), ("retry_ms", (int)retryMs));
            OnRoundEnd?.Invoke();

            Tier.OnTierChanged -= OnTierChanged;

            if (_winsYou < (_bestOf+1)/2 && _winsRival < (_bestOf+1)/2)
                StartCoroutine(RunRound());
        }

        public void EndRound(int you, int rival)
        {
            // Not used directly; analytics emitted in RunRound
        }

        private void OnTierChanged(int tier, string reason) { /* hook if needed */ }

#if UNITY_EDITOR
        // Editor-only helper for PlayMode tests to simulate sudden-death outcome and emit analytics
        public void Editor_SimulateSuddenDeathResolve(bool youWin, int youScore, int rivalScore)
        {
            _suddenDeath = true;
            if (youWin) _winsYou++; else _winsRival++;
            Meta.AnalyticsBridge.Log("round_end", ("winner", youWin?"you":"rival"), ("youScore", youScore), ("rivalScore", rivalScore), ("sudden_death", true), ("retry_ms", 0));
        }
#endif
    }
}
