using UnityEngine;

namespace NeonShift.Infra
{
    public static class Haptics
    {
        public static void Perfect(){ Handheld.Vibrate(); }
        public static void Pressure(){ Handheld.Vibrate(); }
        public static void BombExplode(){ Handheld.Vibrate(); }
        public static void BombDefuse(){ Handheld.Vibrate(); }
    }
}
