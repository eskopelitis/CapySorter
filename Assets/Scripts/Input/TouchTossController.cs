// This file is fully guarded to avoid compile issues when Input System is absent or symbols not yet set.
#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
using ETouch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using EFinger = UnityEngine.InputSystem.EnhancedTouch.Finger;
using NeonShift.Gameplay;
namespace NeonShift.Input
{
    public class TouchTossController : MonoBehaviour
    {
        public float GrabRadius = 0.4f; public float ImpulseScale = 1.0f; private Camera _cam;
        void OnEnable()
        {
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
            #if UNITY_EDITOR
            UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Enable();
            #endif
            ETouch.onFingerDown += OnDown;
            ETouch.onFingerUp += OnUp;
            ETouch.onFingerMove += OnMove;
            if(!_cam) _cam = Camera.main;
        }
        void OnDisable()
        {
            ETouch.onFingerDown -= OnDown;
            ETouch.onFingerUp -= OnUp;
            ETouch.onFingerMove -= OnMove;
            #if UNITY_EDITOR
            UnityEngine.InputSystem.EnhancedTouch.TouchSimulation.Disable();
            #endif
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        }
        private void OnDown(EFinger f)
        {
            var p = ToWorld(f.currentTouch.screenPosition);
            var hit = Physics.OverlapSphere(p, GrabRadius);
            if (hit.Length > 0)
            {
                var g = hit[0].GetComponent<GrabbableItem>();
                if (g) { g.OnGrab(); Debug.Log("touch_grab"); }
            }
        }
        private void OnMove(EFinger f) { /* optional nudge */ }
        private void OnUp(EFinger f) { var p = ToWorld(f.currentTouch.screenPosition); Debug.Log("touch_toss"); }
        private Vector3 ToWorld(Vector2 sp){if(!_cam) _cam=Camera.main; var w=_cam? _cam.ScreenToWorldPoint(new Vector3(sp.x,sp.y,1f)):new Vector3(sp.x,sp.y,0f); return w;}
    }
}
#else
using UnityEngine;
namespace NeonShift.Input
{
    public class TouchTossController : MonoBehaviour { }
}
#endif
