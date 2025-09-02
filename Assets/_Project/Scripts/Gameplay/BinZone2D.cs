using UnityEngine;
using CapySorter.Core;
using CapySorter.Infra;

namespace CapySorter.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class BinZone2D : MonoBehaviour
    {
        public ItemType Accepts;
        public float PerfectRadius = 0.25f; // u
        public float PerfectTimeToBin = 0.8f; // s since grab

        [SerializeField] private FlowTierProvider _tier;
        [SerializeField] private GameManager _gm;

        void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            var item = other.GetComponent<GrabbableItem>();
            if (!item || _gm == null || _tier == null) return;

            bool correct = item.Type == Accepts || (item.Type == ItemType.Bomb && Accepts == ItemType.Trash);
            bool isBomb = item.Type == ItemType.Bomb;

            if (isBomb)
            {
                // Bomb in bin = explode
                _gm.YouBombExplode();
                _tier.ResetOnBomb();
                Haptics.Bomb();
                AnalyticsBridge.Log("bomb", ("result", "explode"));
                _gm.ReleaseItem(other.gameObject);
                return;
            }

            if (correct)
            {
                bool perfect = Vector2.Distance(transform.position, other.transform.position) <= PerfectRadius && item.TimeSinceGrab <= PerfectTimeToBin;
                _gm.YouAddCorrect(perfect);
                _tier.AddPips(perfect ? 2 : 1);
                if (perfect) Haptics.Perfect();
                AnalyticsBridge.Log("bin_result", ("ok", true), ("perfect", perfect));
            }
            else
            {
                _gm.YouContam();
                _tier.Contamination();
                Haptics.Contam();
                AnalyticsBridge.Log("bin_result", ("ok", false));
            }

            _gm.ReleaseItem(other.gameObject);
        }
    }
}
