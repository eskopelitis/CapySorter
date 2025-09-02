using UnityEngine;

namespace NeonShift.Core
{
    public class ConveyorRider : MonoBehaviour
    {
        private float _speed; private bool _enabled;
        public void SetSpeed(float uPerSec) { _speed = uPerSec; }
        public void Enable() { _enabled = true; }
        public void Disable() { _enabled = false; }
        void Update() { if (_enabled && _speed != 0f) transform.Translate(_speed * Time.deltaTime, 0f, 0f, Space.World); }
    }
}
