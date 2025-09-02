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

            float roundT = 0f; const float RoundLen = 90f;
            int lastYou = You.Score, lastRival = Rival.Score;

            while (roundT < RoundLen)
            {
                float dt = Time.deltaTime; roundT += dt;
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
            if (youWin) _winsYou++; else _winsRival++;
            AnalyticsBridge.Log("round_end", ("winner", youWin?"you":"rival"), ("youScore", You.Score), ("rivalScore", Rival.Score), ("sudden_death", _suddenDeath), ("retry_ms", (int)retryMs));

            Tier.OnTierChanged -= OnTierChanged;

            if (_winsYou < (_bestOf+1)/2 && _winsRival < (_bestOf+1)/2)
                StartCoroutine(RunRound());
        }

        public void EndRound(int you, int rival)
        {
            // Not used directly; analytics emitted in RunRound
        }

        private void OnTierChanged(int tier, string reason) { /* hook if needed */ }
    }
}
