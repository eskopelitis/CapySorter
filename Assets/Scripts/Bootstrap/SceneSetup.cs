using UnityEngine;
using NeonShift.Core;
using NeonShift.Infra;

namespace NeonShift.Bootstrap
{
    public class SceneSetup : MonoBehaviour
    {
        [SerializeField] GameManager gm;
        [SerializeField] CenterDropSpawner spawner;
        [SerializeField] ConveyorRider conveyor;
        [SerializeField] FlowTierProvider tier;
        [SerializeField] Transform spawnRoot;
        [SerializeField] ItemPool pool;

        void Start()
        {
            int rr = Screen.currentResolution.refreshRate;
            int target = rr >= 100 ? 120 : 60;
            Application.targetFrameRate = target; QualitySettings.vSyncCount = 0;
            Debug.Log($"frame_rate_selected: {target} (refresh={rr})");
            if (conveyor) conveyor.SetSpeed(1.0f);
            if (spawner) spawner.Init(12345, 10f, tier, spawnRoot, pool);
            if (gm) gm.StartMatch(GameMode.Bo3);
        }
    }
}
