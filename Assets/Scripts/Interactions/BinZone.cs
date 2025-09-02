using UnityEngine;
using NeonShift.Items;
using NeonShift.Core;
using NeonShift.Meta;

namespace NeonShift.Interactions
{
    public class BinZone : MonoBehaviour
    {
        public ItemType Accepts;
        [SerializeField] private FlowTierProvider _tier;
        [SerializeField] private GameManager _gm;

        private void OnTriggerEnter(Collider other)
        {
            var item = other.GetComponent<NeonShift.Gameplay.GrabbableItem>();
            if (!item || _gm == null || _tier == null) return;
            bool isBomb = item.Type == ItemType.Bomb;
            bool correct = !isBomb && item.Type == Accepts;
            if (isBomb)
            {
                _gm.You.BombExplode();
                _tier.ResetOnBomb();
                AnalyticsBridge.Log("bomb_explode", ("time", 0), ("tier", _tier.CurrentTier));
            }
            else if (correct)
            {
                bool perfect = Vector3.Distance(transform.position, other.transform.position) <= 0.25f; // simplistic
                _gm.You.AddCorrect(perfect);
                _tier.AddPips(perfect ? 2 : 1);
                // Log sorting result at the moment score changes
                string bin = Accepts == ItemType.Recycle ? "L" : (Accepts == ItemType.Compost ? "C" : "R");
                AnalyticsBridge.Log("item_sorted", ("type", item.Type.ToString()), ("correct", true), ("bin", bin), ("streak", _gm.You.Streak));
            }
            else
            {
                _gm.You.AddContamination();
                _tier.Contamination();
                string bin = Accepts == ItemType.Recycle ? "L" : (Accepts == ItemType.Compost ? "C" : "R");
                AnalyticsBridge.Log("item_sorted", ("type", item.Type.ToString()), ("correct", false), ("bin", bin), ("streak", _gm.You.Streak));
            }
        }
    }
}
