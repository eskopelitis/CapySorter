using System;

namespace CapySorter.Util
{
    // Fast deterministic PRNG with small state (no allocations)
    [Serializable]
    public struct XorShift32
    {
        private uint _state;

        public XorShift32(uint seed)
        {
            _state = seed == 0 ? 2463534242u : seed; // non-zero seed
        }

        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13;
            x ^= x >> 17;
            x ^= x << 5;
            _state = x;
            return x;
        }

        public int NextInt(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) return minInclusive;
            var range = (uint)(maxExclusive - minInclusive);
            return (int)(NextUInt() % range) + minInclusive;
        }

        public float NextFloat() // [0,1)
        {
            return (NextUInt() & 0x00FFFFFF) / 16777216f; // 24-bit mantissa fraction
        }

        public float NextFloat(float minInclusive, float maxInclusive)
        {
            var t = NextFloat();
            return minInclusive + (maxInclusive - minInclusive) * t;
        }
    }
}
