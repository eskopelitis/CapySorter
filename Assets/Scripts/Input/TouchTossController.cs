using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using NeonShift.Gameplay;

namespace NeonShift.Input
{
    public class TouchTossController : MonoBehaviour
    {
        public float GrabRadius = 0.4f; public float ImpulseScale = 1.0f; private Camera _cam;
        void OnEnable(){EnhancedTouchSupport.Enable(); TouchSimulation.Enable(); Touch.onFingerDown+=OnDown; Touch.onFingerUp+=OnUp; Touch.onFingerMove+=OnMove; if(!_cam) _cam=Camera.main;}
        void OnDisable(){Touch.onFingerDown-=OnDown; Touch.onFingerUp-=OnUp; Touch.onFingerMove-=OnMove; TouchSimulation.Disable(); EnhancedTouchSupport.Disable();}
        private void OnDown(Finger f){var p=ToWorld(f.currentTouch.screenPosition); var hit=Physics.OverlapSphere(p,GrabRadius); if(hit.Length>0){var g=hit[0].GetComponent<GrabbableItem>(); if(g){g.OnGrab(); Debug.Log("touch_grab");}}}
        private void OnMove(Finger f){/* optional nudge */}
        private void OnUp(Finger f){var p=ToWorld(f.currentTouch.screenPosition); Debug.Log("touch_toss");}
        private Vector3 ToWorld(Vector2 sp){if(!_cam) _cam=Camera.main; var w=_cam? _cam.ScreenToWorldPoint(new Vector3(sp.x,sp.y,1f)):new Vector3(sp.x,sp.y,0f); return w;}
    }
}
