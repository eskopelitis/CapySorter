using UnityEngine;
using NeonShift.Items;

namespace NeonShift.Gameplay
{
    public class GrabbableItem : MonoBehaviour
    {
        public ItemType Type;
        private Rigidbody _rb;
        void Awake() { _rb = GetComponent<Rigidbody>(); }
        public void OnGrab() { /* timing handled externally if needed */ }
        public void OnToss(Vector2 v) { if (_rb) _rb.AddForce(new Vector3(v.x, v.y, 0f), ForceMode.Impulse); }
        public void Nudge(Vector2 impulse) { if (_rb) _rb.AddForce(new Vector3(impulse.x, impulse.y, 0f), ForceMode.Impulse); }
    }
}
