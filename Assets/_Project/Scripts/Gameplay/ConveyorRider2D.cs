using UnityEngine;

namespace CapySorter.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class ConveyorRider2D : MonoBehaviour
    {
        private Rigidbody2D _rb;
        private float _speed;
        private bool _enabled;

        void Awake()
        {
            _rb = GetComponent<Rigidbody2D>();
            _rb.gravityScale = 0f;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }

        public void SetSpeed(float unitsPerSec) => _speed = unitsPerSec;
        public void Enable() => _enabled = true;
        public void Disable() => _enabled = false;

        void FixedUpdate()
        {
            if (!_enabled) return;
            var v = new Vector2(_speed, 0f);
            _rb.linearVelocity = v;
        }
    }
}
