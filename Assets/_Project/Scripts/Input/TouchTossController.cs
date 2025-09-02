#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using CapySorter.Gameplay;

namespace CapySorter.Input
{
    public class TouchTossController : MonoBehaviour
    {
        public float GrabRadius = 0.4f; // meters
        public float ImpulseScale = 1.0f; // tune to feel

        private Camera _cam;
        private GrabbableItem[] _grabbed = new GrabbableItem[10]; // map finger index -> item (by finger index % len)
        private Vector2[] _lastWorld = new Vector2[10];
        private float[] _grabTime = new float[10];

        void OnEnable()
        {
            EnhancedTouchSupport.Enable();
            #if UNITY_EDITOR
            TouchSimulation.Enable();
            #endif
            if (_cam == null) _cam = Camera.main;
            Touch.onFingerDown += OnFingerDown;
            Touch.onFingerUp += OnFingerUp;
            Touch.onFingerMove += OnFingerMove;
        }

        void OnDisable()
        {
            Touch.onFingerDown -= OnFingerDown;
            Touch.onFingerUp -= OnFingerUp;
            Touch.onFingerMove -= OnFingerMove;
            #if UNITY_EDITOR
            TouchSimulation.Disable();
            #endif
            EnhancedTouchSupport.Disable();
        }

        private int Slot(int fingerIndex) => fingerIndex % _grabbed.Length;

        private void OnFingerDown(Finger f)
        {
            if (_cam == null) _cam = Camera.main;
            var world = _cam != null ? (Vector2)_cam.ScreenToWorldPoint(f.currentTouch.screenPosition) : Vector2.zero;
            var hit = Physics2D.OverlapCircle(world, GrabRadius);
            if (hit == null) return;
            var item = hit.GetComponent<GrabbableItem>();
            if (item == null) return;
            int s = Slot(f.index);
            _grabbed[s] = item;
            _grabTime[s] = Time.time;
            _lastWorld[s] = world;
            item.OnGrab();
        }

        private void OnFingerMove(Finger f)
        {
            int s = Slot(f.index);
            var item = _grabbed[s];
            if (item == null) return;
            if (_cam == null) _cam = Camera.main;
            var world = _cam != null ? (Vector2)_cam.ScreenToWorldPoint(f.currentTouch.screenPosition) : Vector2.zero;
            // Nudge item towards finger for feedback
            var delta = world - _lastWorld[s];
            _lastWorld[s] = world;
            if (delta.sqrMagnitude > 0f)
            {
                item.Nudge(delta * 0.25f);
            }
        }

        private void OnFingerUp(Finger f)
        {
            int s = Slot(f.index);
            var item = _grabbed[s];
            if (item == null) return;
            if (_cam == null) _cam = Camera.main;
            var touch = f.currentTouch;
            var worldPos = _cam != null ? (Vector2)_cam.ScreenToWorldPoint(touch.screenPosition) : Vector2.zero;
            var startWorld = _lastWorld[s];
            var delta = worldPos - startWorld;
            float dt = Mathf.Max(0.016f, Time.time - _grabTime[s]);
            var vel = delta / dt * ImpulseScale * 0.05f; // convert to impulse-ish scale
            item.OnToss(vel);
            _grabbed[s] = null;
        }
    }
}
#else
using UnityEngine;
namespace CapySorter.Input
{
    public class TouchTossController : MonoBehaviour { }
}
#endif
