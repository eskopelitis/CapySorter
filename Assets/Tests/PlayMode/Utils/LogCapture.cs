#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using UnityEngine;

// Simple log capture helper for PlayMode tests only.
internal sealed class LogCapture : IDisposable
{
    private readonly List<string> _lines = new List<string>(64);

    public LogCapture()
    {
        Application.logMessageReceived += OnLog;
    }

    public void Reset() => _lines.Clear();

    public int Count(string contains)
    {
        if (string.IsNullOrEmpty(contains)) return _lines.Count;
        int c = 0;
        for (int i = 0; i < _lines.Count; i++)
            if (_lines[i].IndexOf(contains, StringComparison.OrdinalIgnoreCase) >= 0) c++;
        return c;
    }

    private void OnLog(string condition, string stackTrace, LogType type)
    {
        _lines.Add($"[{type}] {condition}");
    }

    public void Dispose()
    {
        Application.logMessageReceived -= OnLog;
    }
}
#endif
