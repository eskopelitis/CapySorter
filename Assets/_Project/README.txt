CapySorter Graybox - Sprint 1

Setup
- Create scene Assets/_Project/Scenes/GameScene.unity
- Add empty objects:
  * GameRoot with components: GameManager, FlowTierProvider, ItemPool, CenterDropSpawner, RivalSimulator, SceneBootstrap, TouchTossController
  * DefuseZone with DefuseZone2D (trigger collider), reference GameManager & FlowTierProvider
- Prefabs:
  * 3 Bin prefabs with BoxCollider2D (isTrigger), BinZone2D(Accepts=Recycle/Compost/Trash)
  * 4 Item prefabs with Rigidbody2D (Dynamic, gravity=0), BoxCollider2D, ConveyorRider2D, GrabbableItem(Type=...)
  * Place bins at (9, +1.2/0/-1.2)
  * Spawn lane at x=-3.0

Build
- iOS, Metal, URP 2D.
- IL2CPP, ARM64.

Tests
- Run EditMode tests in Assets/_Project/Tests/EditMode