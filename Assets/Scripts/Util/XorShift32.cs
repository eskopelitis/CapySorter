// Sprint-1 PRNG: no allocations, deterministic
namespace NeonShift.Util
{
    public struct XorShift32
    {
        private uint _state;
        public XorShift32(uint seed) { _state = seed == 0 ? 2463534242u : seed; }
        public uint NextUInt()
        {
            uint x = _state;
            x ^= x << 13; x ^= x >> 17; x ^= x << 5;
            _state = x; return x;
        }
        public float NextFloat01() { return (NextUInt() & 0x00FFFFFF) / 16777216f; }
        public int NextRange(int min, int max)
        {
            if (max <= min) return min;
            uint range = (uint)(max - min);
            return (int)(NextUInt() % range) + min;
        }
    }
}
