# Capy Custodian: Neon Shift (Sprint-1 Graybox)

Unity 2022/2023 LTS, URP Mobile, portrait. iOS-first (Metal, IL2CPP, ARM64). Physics: simple 3D (Rigidbody + Collider) for graybox.

## Prereqs
- Enable Input System package (EnhancedTouch). Ensure URP is set in Project Settings.

## Play the Graybox
- Open `Assets/_Project/Scenes/GameScene.unity` (see `Assets/Resources/README_SCENE_WIRING.txt` to wire it fast).
- Press Play. A 90s round runs; Bo3 flow continues. Analytics are printed to Console (JSON-ish lines).

## Systems
- Deterministic PRNG: XorShift32; all spawns seeded from Init(12345).
- Spawns: SI = beltLength / speed[tier] * factor[tier] with ±5% jitter; bomb gap ≥ 4.
- Tiers: speeds [1.0,1.3,1.6,1.9,2.2], factors [0.55,0.50,0.45,0.40,0.40]; tier up each +5 pips; clamp 1..5.
- Scoring: +1/+2 perfect; −2 contam; bomb −4; defuse +3 and +1 tier.
- Pressure: diff ≥ 15 for 5s applies −10 once/round; logs pressure_start/break/complete.
- Tiebreak: if tie at horn, first correct after horn wins.
- iOS frame-rate: 120 if refresh ≥100Hz else 60 (see `SceneSetup`).

## Tests (runtime harness)
- Add `TestHarness` to a scene; it prints PASS/FAIL for:
  A) Determinism (30 spawns match with same seed)
  B) Bomb gap (never < 4 normals between bombs)
  C) Cadence ±5% (interval samples within bounds)
  D) Pressure once/round (single application)
  E) Bo3 sudden-death (tie, next correct wins)

## Retry < 2 s
- `GameManager` logs round_end with `retry_ms`; should be < 2000.

## GC sanity
- No LINQ/allocs in hot paths; pooling; PRNG struct; reuse buffers. Verify in Profiler: no >2ms GC spikes in Play Mode.
- Optional `GcSanityTracker` on `GameRoot` samples `GC.GetTotalMemory(false)` each second and warns on sustained growth (>~3MB/10s). Use Unity Profiler (Memory + Timeline) to validate.

## CI + LFS
- Android CI builds on PR via GameCI (requires UNITY_LICENSE secret).
- `.gitattributes` marks common binary assets for Git LFS.
