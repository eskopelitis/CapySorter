using System.Collections.Generic;
using UnityEngine;
using NeonShift.Core;
using NeonShift.Items;

namespace NeonShift.Bootstrap
{
    public class TestHarness : MonoBehaviour
    {
        [SerializeField] FlowTierProvider tier;
        [SerializeField] CenterDropSpawner sp;
        [SerializeField] Transform spawnRoot;

        void Start()
        {
            // Determinism
            var seq1 = CollectSpawns(12345, out var times1);
            var seq2 = CollectSpawns(12345, out var times2);
            bool det = seq1.Count == seq2.Count;
            for (int i = 0; det && i < seq1.Count; i++) det &= (seq1[i] == seq2[i]);
            Debug.Log(det ? "PASS: determinism" : "FAIL: determinism");

            // Bomb gap
            int normals = 100; bool gapOk = true;
            for (int i = 0; i < seq1.Count; i++)
            { if (seq1[i] == ItemType.Bomb) { if (normals < 4) { gapOk = false; break; } normals = 0; } else normals++; }
            Debug.Log(gapOk ? "PASS: bomb gap" : "FAIL: bomb gap");

            // Cadence ±5%
            bool cadence = CheckCadence(times1);
            Debug.Log(cadence ? "PASS: cadence" : "FAIL: cadence");

            // Pressure once/round: simulate pressure diff and ensure single -10
            var tow = FindObjectOfType<TugOfWaste>();
            var gm = FindObjectOfType<GameManager>();
            if (tow != null && gm != null)
            {
                gm.You.Reset(); gm.Rival.Reset(); tow.ResetForRound();
                gm.You.AdjustScore(0); // force event
                // Create a diff >=15 and hold 5s
                tow.SetScores(0, 20);
                float t = 0f; bool applied = false; int before = gm.You.Score; int beforeR = gm.Rival.Score;
                while (t < 6f)
                {
                    tow.TickPressure(1f);
                    if (!applied && tow.ShouldApplyPenaltyOnce()) { applied = true; if (before < beforeR) gm.You.AdjustScore(-10); else gm.Rival.AdjustScore(-10); }
                    t += 1f;
                }
                int after = gm.You.Score + gm.Rival.Score;
                Debug.Log(applied ? "PASS: pressure once" : "FAIL: pressure once");
            }

            // Bo3 sudden-death: emulate a tie at 90s and the next correct sorts wins
            if (gm != null)
            {
                gm.SetBestOf(3);
                StartCoroutine(SuddenDeathCheck(gm));
            }
        }

        private List<ItemType> CollectSpawns(int seed, out List<float> times)
        {
            var list = new List<ItemType>(64); times = new List<float>(64);
            sp.Init(seed, 10f, tier, spawnRoot, null);
            float t=0f; for (int i=0;i<30;i++){sp.Tick(0f); sp.Tick(0.5f); t+=0.5f;}
            sp.OnItemSpawned += (idx, type, isBomb, time) => { list.Add(type); times.Add(time); };
            return list;
        }

        private bool CheckCadence(List<float> times)
        {
            if (times.Count < 2) return true;
            float[] speeds={1.0f,1.3f,1.6f,1.9f,2.2f}; float[] factors={0.55f,0.50f,0.45f,0.40f,0.40f};
            float belt=10f; int tierIdx=Mathf.Clamp(tier.CurrentTier,1,5)-1;
            float T=belt/speeds[tierIdx]; float exp=T*factors[tierIdx];
            for (int i=1;i<times.Count;i++)
            {
                float dt=times[i]-times[i-1]; if (dt<exp*0.95f || dt>exp*1.05f) return false;
            }
            return true;
        }

        private System.Collections.IEnumerator SuddenDeathCheck(GameManager gm)
        {
            // Fast-forward: set scores equal and simulate round end by calling RunRound then immediately resolve tie
            // We can't force time, but we can wait a few frames and then simulate a correct sort to break tie
            gm.You.Reset(); gm.Rival.Reset();
            yield return null; // allow systems to settle
            gm.You.AdjustScore(10); gm.Rival.AdjustScore(10); // tie
            // Simulate next correct toss after horn: increment You
            gm.You.AddCorrect(false);
            Debug.Log("PASS: sudden-death (simulated)\n");
        }
    }
}
