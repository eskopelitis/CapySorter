using UnityEngine;

namespace CapySorter.Infra
{
    public static class Haptics
    {
        [System.Diagnostics.Conditional("UNITY_IOS")]
        public static void Perfect() { Handheld.Vibrate(); }
        [System.Diagnostics.Conditional("UNITY_IOS")]
        public static void Contam() { Handheld.Vibrate(); }
        [System.Diagnostics.Conditional("UNITY_IOS")]
        public static void Bomb() { Handheld.Vibrate(); }
    }
}
