using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using NeonShift.Core;
using NeonShift.Items;
using NeonShift.Gameplay;
using NeonShift.Infra;
using NeonShift.Bootstrap;
using NeonShift.Interactions;

public static class SceneAutoBuilder
{
    private const string SceneFolder = "Assets/_Project/Scenes";
    private const string PrefabFolder = "Assets/_Project/Prefabs";
    private const string ScenePath = SceneFolder + "/GameScene.unity";

    [MenuItem("Tools/Build Graybox Scene")]
    public static void BuildGrayboxScene()
    {
        EnsureFolders();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Camera
        var camGO = new GameObject("Main Camera");
        var cam = camGO.AddComponent<Camera>();
        camGO.tag = "MainCamera";
        cam.orthographic = false;
        camGO.transform.position = new Vector3(2f, 2.5f, -8f);
        camGO.transform.rotation = Quaternion.Euler(10f, 15f, 0f);

        // Light
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // GameRoot and core components
        var root = new GameObject("GameRoot");
        var gm = root.AddComponent<GameManager>();
        var tier = root.AddComponent<FlowTierProvider>();
        var tow = root.AddComponent<TugOfWaste>();
        var pool = root.AddComponent<ItemPool>();
        var spawner = root.AddComponent<CenterDropSpawner>();
        var conveyor = root.AddComponent<ConveyorRider>();
        var setup = root.AddComponent<SceneSetup>();
        var gcTracker = root.AddComponent<GcSanityTracker>();

        // Spawn root
        var spawnRoot = new GameObject("SpawnRoot").transform;
        spawnRoot.position = new Vector3(-3f, 1f, 0f);

        // Prefabs
        var recyclePrefab = CreateItemPrefab("Item_Recycle", ItemType.Recycle, new Color(0.2f, 0.8f, 0.2f));
        var compostPrefab = CreateItemPrefab("Item_Compost", ItemType.Compost, new Color(0.6f, 0.4f, 0.2f));
        var trashPrefab = CreateItemPrefab("Item_Trash", ItemType.Trash, new Color(0.5f, 0.5f, 0.5f));
        var bombPrefab = CreateItemPrefab("Item_Bomb", ItemType.Bomb, new Color(0.8f, 0.2f, 0.2f));

        // Configure ItemPool entries by prewarming
        pool.Prewarm(40);

        // Link GameManager refs
        gm.Tier = tier;
        gm.Spawner = spawner;
        gm.Pressure = tow;

        // Wire SceneSetup
        SerializedObject so = new SerializedObject(setup);
        so.FindProperty("gm").objectReferenceValue = gm;
        so.FindProperty("spawner").objectReferenceValue = spawner;
        so.FindProperty("conveyor").objectReferenceValue = conveyor;
        so.FindProperty("tier").objectReferenceValue = tier;
        so.FindProperty("spawnRoot").objectReferenceValue = spawnRoot;
        so.FindProperty("pool").objectReferenceValue = pool;
        so.ApplyModifiedPropertiesWithoutUndo();

        // Bins at X=+9, Z lanes spaced by ~1.2
        CreateBin("Bin_Recycle", new Vector3(9f, 0f, -1.2f), ItemType.Recycle, tier, gm);
        CreateBin("Bin_Compost", new Vector3(9f, 0f, 0f), ItemType.Compost, tier, gm);
        CreateBin("Bin_Trash", new Vector3(9f, 0f, 1.2f), ItemType.Trash, tier, gm);

        // Save scene
        EditorSceneManager.SaveScene(scene, ScenePath, true);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Graybox scene built and saved at {ScenePath}");
    }

    private static void EnsureFolders()
    {
        if (!AssetDatabase.IsValidFolder("Assets/_Project")) AssetDatabase.CreateFolder("Assets", "_Project");
        if (!AssetDatabase.IsValidFolder(SceneFolder)) AssetDatabase.CreateFolder("Assets/_Project", "Scenes");
        if (!AssetDatabase.IsValidFolder(PrefabFolder)) AssetDatabase.CreateFolder("Assets/_Project", "Prefabs");
    }

    private static GameObject CreateItemPrefab(string name, ItemType type, Color color)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        var col = go.GetComponent<BoxCollider>();
        col.isTrigger = false;
        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = true;
        var rider = go.AddComponent<ConveyorRider>();
        var gi = go.AddComponent<GrabbableItem>();
        gi.Type = type;
        var rend = go.GetComponent<Renderer>();
        rend.sharedMaterial = GetOrCreateUnlitMaterial(color);

        string path = PrefabFolder + "/" + name + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return prefab;
    }

    private static Material GetOrCreateUnlitMaterial(Color color)
    {
        const string matPath = PrefabFolder + "/_Graybox.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (!mat)
        {
            mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            AssetDatabase.CreateAsset(mat, matPath);
        }
        mat.color = color;
        EditorUtility.SetDirty(mat);
        return mat;
    }

    private static void CreateBin(string name, Vector3 pos, ItemType accepts, FlowTierProvider tier, GameManager gm)
    {
        var bin = new GameObject(name);
        bin.transform.position = pos;
        var box = bin.AddComponent<BoxCollider>();
        box.isTrigger = true;
        var zone = bin.AddComponent<BinZone>();
        zone.Accepts = accepts;
        // Wire references
        SerializedObject so = new SerializedObject(zone);
        so.FindProperty("_tier").objectReferenceValue = tier;
        so.FindProperty("_gm").objectReferenceValue = gm;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    [InitializeOnLoadMethod]
    private static void BuildIfMissing()
    {
        if (!File.Exists(ScenePath))
        {
            // Delay call to allow domain load
            EditorApplication.delayCall += () =>
            {
                if (!File.Exists(ScenePath)) BuildGrayboxScene();
            };
        }
    }
}
