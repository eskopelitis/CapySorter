using UnityEngine;
using CapySorter.Core;
using CapySorter.Gameplay;

namespace CapySorter.Infra
{
    public class SceneBootstrap : MonoBehaviour
    {
        [SerializeField] GameManager gm; [SerializeField] CenterDropSpawner sp; [SerializeField] ConveyorRider2D belt;

        void Start()
        {
            Application.targetFrameRate = 60; // will bump to 120 later if supported
            QualitySettings.vSyncCount = 0;
            if (belt) belt.SetSpeed(1.0f);
            gm.StartMatch(3);
        }
    }
}
