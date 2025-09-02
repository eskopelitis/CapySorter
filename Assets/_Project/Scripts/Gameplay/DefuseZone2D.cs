using UnityEngine;
using CapySorter.Core;
using CapySorter.Infra;

namespace CapySorter.Gameplay
{
    [RequireComponent(typeof(Collider2D))]
    public class DefuseZone2D : MonoBehaviour
    {
        public float HoldSeconds = 1.0f;
        public float MaxSpeed = 0.2f; // u/s

        [SerializeField] private FlowTierProvider _tier;
        [SerializeField] private GameManager _gm;

        private void Reset()
        {
            var col = GetComponent<Collider2D>();
            if (col) col.isTrigger = true;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            var item = other.GetComponent<GrabbableItem>();
            if (item == null || item.Type != ItemType.Bomb || _gm == null || _tier == null) return;

            var rb = other.attachedRigidbody;
            if (!rb) return;
            if (rb.velocity.sqrMagnitude > MaxSpeed * MaxSpeed) return;

            // Track time using a component-bound timer to avoid allocations
            var timer = other.GetComponent<_DefuseTimer>();
            if (!timer) timer = other.gameObject.AddComponent<_DefuseTimer>();
            timer.t += Time.deltaTime;
            if (timer.t >= HoldSeconds)
            {
                _gm.YouBombDefuse();
                _tier.AddTierDirect(+1);
                Haptics.Perfect();
                AnalyticsBridge.Log("bomb", ("result", "defuse"));
                Destroy(timer);
                _gm.ReleaseItem(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var timer = other.GetComponent<_DefuseTimer>();
            if (timer) Destroy(timer);
        }

        private sealed class _DefuseTimer : MonoBehaviour { public float t; }
    }
}
