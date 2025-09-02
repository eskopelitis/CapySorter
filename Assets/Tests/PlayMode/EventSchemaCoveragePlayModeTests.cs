using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using NeonShift.Core;
using NeonShift.Items;

public class EventSchemaCoveragePlayModeTests
{
    [UnityTest]
    public IEnumerator All_Analytics_Events_Appear_In_Short_Round()
    {
        using (var logs = new LogCapture())
        {
            logs.Reset();

            // Ensure a minimal runtime scene context
            var tierGO = new GameObject("Tier");
            var tier = tierGO.AddComponent<FlowTierProvider>();
            var gmGO = new GameObject("GM");
            var gm = gmGO.AddComponent<GameManager>();
            var spGO = new GameObject("Spawner");
            var sp = spGO.AddComponent<CenterDropSpawner>();
            var pool = gmGO.AddComponent<NeonShift.Infra.ItemPool>();

            gm.Tier = tier; gm.Spawner = sp; gm.Pressure = gmGO.AddComponent<TugOfWaste>();
            sp.Init(12345, 10f, tier, null, pool);

            // Short round
            gm.SetBestOf(1);
            // if runtime supports a duration field, set it
            var durField = typeof(GameManager).GetField("RoundDurationSec");
            if (durField != null) durField.SetValue(gm, 8f);
            var durProp = typeof(GameManager).GetProperty("RoundDurationSec");
            if (durProp != null && durProp.CanWrite) durProp.SetValue(gm, 8f);

            // Drive simple interactions
            int spawns = 0; int bombs = 0;
            sp.OnItemSpawned += (i, t, isBomb, time) => { spawns++; if (isBomb) bombs++; };

            gm.StartMatch(GameMode.Bo3);

            float t = 0f;
            while (t < 12f && logs.Count("round_end") == 0)
            {
                sp.Tick(0.1f);
                // simulate some scoring effects on You
                if (t < 2f) { gm.You.AddCorrect(perfect: t % 0.4f < 0.2f); tier.AddPips(1); }
                else if (t < 4f) { gm.You.AddContamination(); tier.Contamination(); }
                else { if (gm.Rival.Score - gm.You.Score < 16) gm.Rival.AdjustScore(5); }
                gm.Pressure.SetScores(gm.You.Score, gm.Rival.Score); gm.Pressure.TickPressure(0.2f);
                yield return null; t += 0.2f;
            }

            // Required events at least once
            string[] required = {
                "run_start","item_spawn","item_sorted","bomb_spawn","bomb_defuse","bomb_explode",
                "combo_change","flow_pips_change","tier_change",
                "pressure_start","pressure_break","pressure_complete","round_end"
            };
            foreach (var evt in required) Assert.Greater(logs.Count(evt), 0, $"missing {evt}");
        }
    }
}
