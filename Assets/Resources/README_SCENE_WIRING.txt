Capy Custodian: Neon Shift - Graybox Scene Wiring

Physics: Using simple 3D for graybox (Rigidbody + Collider). Gravity can be 0.

Steps:
1) Create Assets/_Project/Scenes/GameScene.unity
2) Create GameRoot and add components: GameManager, FlowTierProvider, TugOfWaste, CenterDropSpawner, ConveyorRider, SceneSetup, ItemPool.
3) Create Empty "SpawnRoot" at X=-3.0, Y=0.0 (assign to SceneSetup + Spawner Init).
4) Create 3 Bin objects at X=+9.0 with BoxCollider (isTrigger) and BinZone (Accepts = Recycle, Compost, Trash). Separate by Y: +1.2, 0, -1.2.
5) Create 4 item prefabs (cube placeholders) with Collider + Rigidbody (no gravity) + ConveyorRider + GrabbableItem(Type set).
6) Belt length ~10u; lane separation ~1.2u.
7) Assign references in SceneSetup (gm, spawner, conveyor, tier, spawnRoot, pool).
8) Press Play to run a 90s round and see analytics logs.
