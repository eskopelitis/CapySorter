using System.Collections.Generic;
using System.Diagnostics;
using System;
using UnityEngine;

namespace NeonShift.Bootstrap
{
    public class GcSanityTracker : MonoBehaviour
    {
        [Tooltip("Enable sampling in Play mode")] public bool Enabled = true;
        private readonly Queue<long> _window = new Queue<long>(10);
        private float _timer;

        void Update()
        {
            if (!Enabled) return;
            _timer += Time.unscaledDeltaTime;
            if (_timer < 1f) return;
            _timer = 0f;
            long mem = GC.GetTotalMemory(false);
            if (_window.Count == 10) _window.Dequeue();
            _window.Enqueue(mem);
            if (_window.Count == 10)
            {
                long first = 0; long last = 0; int i = 0;
                foreach (var v in _window) { if (i == 0) first = v; last = v; i++; }
                long growth = last - first;
                // If ~>50KB per frame at 60fps -> ~3MB/s. We evaluate per-second moving window: warn if >3MB increase over 10s.
                if (growth > 3_000_000)
                    UnityEngine.Debug.LogWarning($"GC sanity: sustained growth {growth/1024} KB over ~10s. Check allocations.");
            }
        }
    }
}
