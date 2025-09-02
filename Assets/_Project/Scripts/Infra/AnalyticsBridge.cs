using System;
using System.Text;
using UnityEngine;

namespace CapySorter.Infra
{
    public static class AnalyticsBridge
    {
        // Reusable StringBuilder to limit GC. Not thread-safe by design (main thread only in Unity).
        static readonly StringBuilder sb = new StringBuilder(256);

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        public static void Log(string evt, params (string key, object val)[] data)
        {
            sb.Length = 0;
            sb.Append('{');
            sb.Append("evt:\"").Append(evt).Append('\"');
            sb.Append(',').Append("t:").Append((long)(Time.realtimeSinceStartup * 1000f));
            if (data != null)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var (k, v) = data[i];
                    sb.Append(',').Append(k).Append(':');
                    if (v is string s)
                    {
                        sb.Append('\"').Append(s).Append('\"');
                    }
                    else if (v is bool b)
                    {
                        sb.Append(b ? "true" : "false");
                    }
                    else
                    {
                        sb.Append(v);
                    }
                }
            }
            sb.Append('}');
            Debug.Log(sb.ToString());
        }
    }
}
