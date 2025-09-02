using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonShift.Core;
using NeonShift.Items;

public class Sprint1AcceptanceTests
{
    [UnityTest]
    public IEnumerator Spawner_Determinism_BombGap_Cadence()
    {
        var tierGO = new GameObject("Tier");
        var tier = tierGO.AddComponent<FlowTierProvider>();
        var spawnerGO = new GameObject("Spawner");
        var sp = spawnerGO.AddComponent<CenterDropSpawner>();

        // Subscribe first, then drive spawns
        var types = new List<ItemType>();
        var times = new List<float>();
        sp.OnItemSpawned += (i, t, isBomb, time) => { types.Add(t); times.Add(time); };

        sp.Init(12345, 10f, tier, null, null);
        // Simulate ticks until 30 spawns
        float simTime = 0f;
        while (types.Count < 30)
        {
            sp.Tick(0.1f); simTime += 0.1f; // advance in small steps to cross spawn thresholds
            yield return null;
        }

        // A) Determinism: re-run and compare
        var types2 = new List<ItemType>();
        var times2 = new List<float>();
        var sp2 = new GameObject("Spawner2").AddComponent<CenterDropSpawner>();
        sp2.OnItemSpawned += (i, t, isBomb, time) => { types2.Add(t); times2.Add(time); };
        sp2.Init(12345, 10f, tier, null, null);
        simTime = 0f;
        while (types2.Count < 30)
        {
            sp2.Tick(0.1f); simTime += 0.1f; yield return null;
        }
        Assert.AreEqual(types.Count, types2.Count);
        for (int i = 0; i < types.Count; i++) Assert.AreEqual(types[i], types2[i], $"spawn index {i}");

        // B) Bomb gap >= 4
        int normals = 100; bool gapOk = true;
        for (int i = 0; i < types.Count; i++)
        { if (types[i] == ItemType.Bomb) { if (normals < 4) { gapOk = false; break; } normals = 0; } else normals++; }
        Assert.IsTrue(gapOk, "Bomb gap < 4 detected");

        // C) Cadence ±5% for current tier
        float[] speeds={1.0f,1.3f,1.6f,1.9f,2.2f}; float[] factors={0.55f,0.50f,0.45f,0.40f,0.40f};
        int tierIdx = Mathf.Clamp(tier.CurrentTier,1,5)-1;
        float T = 10f / speeds[tierIdx]; float exp = T * factors[tierIdx];
        for (int i = 1; i < times.Count; i++)
        { float dt = times[i]-times[i-1]; Assert.IsTrue(dt>=exp*0.95f && dt<=exp*1.05f, $"cadence dt={dt} at i={i}"); }
    }

    [UnityTest]
    public IEnumerator Pressure_Once_Round()
    {
        var gmGO = new GameObject("GM");
        var gm = gmGO.AddComponent<GameManager>();
        var tow = gmGO.AddComponent<TugOfWaste>();
        gm.Pressure = tow;

        gm.You.Reset(); gm.Rival.Reset();
        tow.ResetForRound();
        tow.SetScores(0, 20);
        float t=0f; bool applied=false; int startSum = gm.You.Score + gm.Rival.Score;
        while (t < 6f)
        {
            tow.TickPressure(1f);
            if (!applied && tow.ShouldApplyPenaltyOnce()) { applied = true; gm.You.AdjustScore(-10); }
            t += 1f; yield return null;
        }
        int endSum = gm.You.Score + gm.Rival.Score;
        Assert.IsTrue(applied, "Pressure did not apply");
        Assert.AreEqual(startSum - 10, endSum, "Pressure applied more than once or wrong delta");
    }

    [Test]
    public void Bo3_SuddenDeath_Resolution_Helper()
    {
        var gm = new GameObject("GM").AddComponent<GameManager>();
        int wy0 = gm.WinsYou; int wr0 = gm.WinsRival;
        gm.Editor_SimulateSuddenDeathResolve(true, 11, 10);
        Assert.Greater(gm.WinsYou, wy0);
        Assert.AreEqual(wr0, gm.WinsRival);
    }
}
