using UnityEngine;
using CapySorter.Core;

namespace CapySorter.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class GrabbableItem : MonoBehaviour
    {
        public ItemType Type;

        private Rigidbody2D _rb;
    private float _lastGrabTime;
    private bool _grabbed;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
        }

        public void OnGrab()
        {
            _grabbed = true;
            _lastGrabTime = Time.time;
        }

        public float TimeSinceGrab => (Time.time - _lastGrabTime);

        public void OnToss(Vector2 velocity)
        {
            _grabbed = false;
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
            _rb.AddForce(velocity, ForceMode2D.Impulse);
        }

        public void Nudge(Vector2 impulse)
        {
            _rb.AddForce(impulse, ForceMode2D.Impulse);
        }
    }
}
