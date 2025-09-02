using NUnit.Framework;
using UnityEngine;
using CapySorter.Core;
using CapySorter.Gameplay;
using CapySorter.Infra;

namespace CapySorter.Tests
{
    public class TestHarness
    {
        [Test]
        public void XorShift_Deterministic()
        {
            var a = new Util.XorShift32(12345);
            var b = new Util.XorShift32(12345);
            for (int i = 0; i < 100; i++)
                Assert.AreEqual(a.NextUInt(), b.NextUInt());
        }

        [Test]
        public void FlowTier_TieringPips()
        {
            var go = new GameObject("tier");
            var tier = go.AddComponent<FlowTierProvider>();
            tier.ResetAll();
            int last = tier.CurrentTier;
            int changes = 0;
            tier.OnTierChanged += t => { last = t; changes++; };
            tier.AddPips(5);
            Assert.AreEqual(2, last);
            tier.AddPips(10);
            Assert.AreEqual(4, last);
            tier.AddPips(100);
            Assert.AreEqual(5, last);
            tier.Contamination();
            Assert.AreEqual(4, last);
            tier.ResetOnBomb();
            Assert.AreEqual(1, last);
        }

        [Test]
        public void ScoreModel_Rules()
        {
            var s = new ScoreModel();
            s.Reset();
            s.AddCorrect(false); // +1
            s.AddCorrect(true); // +2
            s.AddContamination(); // -2
            s.BombDefuse(); // +3
            s.BombExplode(); // -4
            Assert.AreEqual(0, s.Score);
        }

        [Test]
        public void Spawner_DeterministicAndBombGap()
        {
            var root = new GameObject("root");
            var pool = root.AddComponent<ItemPool>();
            var tierGo = new GameObject("tier");
            var tier = tierGo.AddComponent<FlowTierProvider>();
            tier.ResetAll();
            var spGo = new GameObject("sp");
            var sp = spGo.AddComponent<CenterDropSpawner>();
            // inject private refs via serialized fields using Unity magic: assign from editor; in tests via reflection
            typeof(CenterDropSpawner).GetField("_pool", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sp, pool);
            typeof(CenterDropSpawner).GetField("_tier", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).SetValue(sp, tier);

            sp.Init(12345);
            var seq1 = new System.Collections.Generic.List<ItemType>();
            sp.OnItemSpawn += (t, p) => seq1.Add(t);
            for (int i = 0; i < 50; i++) sp.Tick(0.5f); // force many ticks to spawn

            sp.Init(12345);
            var seq2 = new System.Collections.Generic.List<ItemType>();
            sp.OnItemSpawn += (t, p) => seq2.Add(t);
            for (int i = 0; i < 50; i++) sp.Tick(0.5f);

            Assert.AreEqual(seq1.Count, seq2.Count);
            for (int i = 0; i < seq1.Count; i++) Assert.AreEqual(seq1[i], seq2[i]);

            // bomb gap >= 4 normals
            int normalsSinceBomb = 100;
            foreach (var t in seq1)
            {
                if (t == ItemType.Bomb)
                {
                    Assert.GreaterOrEqual(normalsSinceBomb, 4);
                    normalsSinceBomb = 0;
                }
                else normalsSinceBomb++;
            }
        }
    }
}
