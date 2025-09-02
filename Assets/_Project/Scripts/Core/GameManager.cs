using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using CapySorter.Infra;
using CapySorter.Gameplay;
using CapySorter.Core;

namespace CapySorter.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private CenterDropSpawner _spawner;
        [SerializeField] private RivalSimulator _rival;
        [SerializeField] private FlowTierProvider _tier;
        [SerializeField] private ItemPool _pool;

        public ScoreModel You = new ScoreModel();
        public ScoreModel Rival = new ScoreModel();

        private bool _pressureApplied;
        private float _pressureTimer;
        private int _bestOf = 3;
        private int _winsYou, _winsRival;

        public void StartMatch(int bestOf = 3)
        {
            _bestOf = Mathf.Max(1, bestOf);
            _winsYou = 0;
            _winsRival = 0;
            StartCoroutine(RunRound());
        }

        public IEnumerator RunRound()
        {
            // Setup
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            _pressureApplied = false;
            _pressureTimer = 0f;
            You.Reset();
            Rival.Reset();
            _tier.ResetAll();
            _pool.Prewarm();
            _spawner.Init(12345);
            _spawner.OnItemSpawn += OnSpawnEvent;
            _rival.Init(12345, 0.85f, 300f);

            AnalyticsBridge.Log("round_start");

            float t = 0f;
            const float RoundDur = 90f;
            while (t < RoundDur)
            {
                float dt = Time.deltaTime;
                t += dt;
                _spawner.Tick(dt);
                PressureCheck(dt);
                yield return null;
            }

            EndRound(You.Score, Rival.Score);
            _spawner.OnItemSpawn -= OnSpawnEvent;

            // Continue match until best-of resolved
            if (_winsYou < (_bestOf + 1) / 2 && _winsRival < (_bestOf + 1) / 2)
                StartCoroutine(RunRound());
        }

        public void EndRound(int you, int rival)
        {
            int result = you.CompareTo(rival);
            if (result > 0) _winsYou++; else if (result < 0) _winsRival++; else
            {
                // tiebreak: first correct after horn - simplified: nudge in your favor if next correct within 5s
                _winsYou++;
            }
            AnalyticsBridge.Log("round_end", ("you", you), ("rival", rival), ("wy", _winsYou), ("wr", _winsRival));
        }

        private void PressureCheck(float dt)
        {
            if (_pressureApplied) return;
            int diff = Mathf.Abs(You.Score - Rival.Score);
            if (diff >= 15)
            {
                _pressureTimer += dt;
                if (_pressureTimer >= 5f)
                {
                    if (You.Score < Rival.Score) YouPenalty(); else RivalPenalty();
                    _pressureApplied = true; // once per round
                    AnalyticsBridge.Log("pressure", ("applied", true));
                }
            }
            // else: pause (do not reset), as per spec
        }

#if UNITY_EDITOR
        // Editor-test hook to advance only the pressure rule without running the whole round.
        internal void __Test_PressureTick(float dt)
        {
            PressureCheck(dt);
        }
#endif

    private void YouPenalty() { You.AdjustScore(-10); }
    private void RivalPenalty() { Rival.AdjustScore(-10); }

        // API for zones
        public void YouAddCorrect(bool perfect) { You.AddCorrect(perfect); }
        public void YouContam() { You.AddContamination(); }
        public void YouBombExplode() { You.BombExplode(); }
        public void YouBombDefuse() { You.BombDefuse(); }

    private void OnSpawnEvent(ItemType t, Vector2 _)
    { _rival.OnSpawn(t); }

        public void ReleaseItem(GameObject go)
        {
            _pool.Release(go);
        }
    }
}
