using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.IO;

public class Setup3DScene
{
    const string Village = "Assets/3D/Village";
    const string NaturePrefabs = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs";
    const string Kayak = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE";

    const string OutputScenePath = "Assets/Village.unity";
    const string OldScenePath = "Assets/Main.unity";

    [MenuItem("Tools/Build 3D Scene")]
    public static void Build()
    {
        ForceCleanCachedScene();
        EnsureGoldIcon();
        CreateGround();
        PlaceVillage();
        PlaceNatureFeatures();
        PlaceForest();
        PlaceDecorations();
        SetupEnemySpawner();
        SetupVillageWalls();
        PlacePlayer();
        CreateSlots();
        SetupUI();
        SetupCamera();
        SetupLights();
        SetupPlayerSettings();

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.SaveScene(scene, OutputScenePath);

        // Make sure the standalone build always uses the generated Village scene.
        EditorBuildSettings.scenes = new[] {
            new EditorBuildSettingsScene(OutputScenePath, true)
        };

        // Do NOT call OpenScene at the end - it can cause Unity to load stale cached content.
        // The scene is already active and saved.

        Debug.Log($"Scene saved to {OutputScenePath}! Press Play to test the slots.");
    }

    [MenuItem("Tools/Clean Cache")]
    public static void CleanCache()
    {
        ForceCleanCachedScene();
        Debug.Log("Cache cleaned. Run 'Build 3D Scene' to regenerate.");
    }

    static void ForceCleanCachedScene()
    {
        // AGGRESSIVE CLEAN: destroy EVERYTHING in the current scene first
        // This handles cases where stale objects from a previous session linger in memory.
        var allRoots = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene().GetRootGameObjects();
        foreach (var go in allRoots)
        {
            if (go != null) Object.DestroyImmediate(go);
        }

        // Create a brand new empty active scene
        var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene);
        EditorSceneManager.SetActiveScene(newScene);

        // Delete the saved scene file so Unity cannot reload the old one
        foreach (var path in new[] { OutputScenePath, OldScenePath })
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Deleted old {path}");
            }

            var metaPath = path + ".meta";
            if (File.Exists(metaPath))
            {
                File.Delete(metaPath);
                Debug.Log($"Deleted old {metaPath}");
            }
        }

        // Delete cached prefabs and materials so they are rebuilt with proper shaders.
        string[] deleteDirs = { "Assets/Prefabs", "Assets/Materials", "Assets/Data/Slots", "Assets/Textures" };
        foreach (var dir in deleteDirs)
        {
            if (Directory.Exists(dir))
            {
                Directory.Delete(dir, true);
                Debug.Log($"Deleted {dir} (will be recreated)");
            }
            string meta = dir + ".meta";
            if (File.Exists(meta))
            {
                File.Delete(meta);
                Debug.Log($"Deleted {meta}");
            }
        }

        AssetDatabase.Refresh();
    }

    static void Cleanup()
    {
        var river = GameObject.Find("River");
        if (river != null) Object.DestroyImmediate(river);

        var all = Object.FindObjectsByType<GameObject>(FindObjectsInactive.Include);
        foreach (var go in all)
        {
            if (go == null) continue;
            if (go.transform.parent != null) continue;
            Object.DestroyImmediate(go);
        }
        Debug.Log("Cleanup: destroyed all GameObjects.");
    }

    static void CreateGround()
    {
        // Use a very thick cube so the orthographic camera never sees past it.
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Ground";
        go.transform.localScale = new Vector3(1600f, 1200f, 1600f);
        go.transform.position = new Vector3(0f, -600f, 0f);

        string matPath = "Assets/Materials/Ground.mat";
        var mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (mat == null)
        {
            Directory.CreateDirectory("Assets/Materials");
            mat = new Material(Shader.Find("Standard"));
            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D/NatureLite/T_Landscape_Grass.png");
            if (tex != null)
            {
                mat.mainTexture = tex;
                mat.mainTextureScale = new Vector2(200f, 200f);
            }
            else
            {
                mat.color = new Color(0.25f, 0.5f, 0.18f);
            }
            AssetDatabase.CreateAsset(mat, matPath);
        }
        go.GetComponent<Renderer>().sharedMaterial = mat;
    }

    static void PlaceVillage()
    {
        // Fence removed for now to keep the Hierarchy clean while debugging slots.
        // Houses and watermill are now buildable slots, not pre-placed.
    }

    static void PlaceFence(float left, float right, float bottom, float top)
    {
        var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab");
        if (fencePrefab == null) { Debug.LogWarning("Fence prefab not found!"); return; }

        int count = 0;
        float step = 2f, fy = 0;
        float gateLeft = -8f, gateRight = 8f;
        float gateBuffer = 2.5f;
        const float fenceScaleY = 3f;

        for (float x = left + 1f; x <= gateLeft - gateBuffer; x += step)
        {
            var seg = (GameObject)Object.Instantiate(fencePrefab);
            seg.transform.position = new Vector3(x, fy, bottom);
            seg.transform.localScale = new Vector3(1f, fenceScaleY, 1f);
            count++;
        }
        for (float x = gateRight + gateBuffer; x <= right - 1f; x += step)
        {
            var seg = (GameObject)Object.Instantiate(fencePrefab);
            seg.transform.position = new Vector3(x, fy, bottom);
            seg.transform.localScale = new Vector3(1f, fenceScaleY, 1f);
            count++;
        }

        var postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.4f, 0.25f, 0.12f);
        foreach (float gx in new[] { gateLeft, gateRight })
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "GatePost";
            post.transform.position = new Vector3(gx, 1.2f * fenceScaleY, bottom);
            post.transform.localScale = new Vector3(0.35f, 2.4f * fenceScaleY, 0.35f);
            post.GetComponent<Renderer>().sharedMaterial = postMat;
        }

        for (float z = bottom + 1f; z <= top - 1f; z += step)
        {
            var seg = (GameObject)Object.Instantiate(fencePrefab);
            seg.transform.position = new Vector3(right, fy, z);
            seg.transform.rotation = Quaternion.Euler(0, 90, 0);
            seg.transform.localScale = new Vector3(1f, fenceScaleY, 1f);
            count++;
        }
        for (float x = left + 1f; x <= right - 1f; x += step)
        {
            var seg = (GameObject)Object.Instantiate(fencePrefab);
            seg.transform.position = new Vector3(x, fy, top);
            seg.transform.rotation = Quaternion.Euler(0, 180, 0);
            seg.transform.localScale = new Vector3(1f, fenceScaleY, 1f);
            count++;
        }
        for (float z = bottom + 1f; z <= top - 1f; z += step)
        {
            var seg = (GameObject)Object.Instantiate(fencePrefab);
            seg.transform.position = new Vector3(left, fy, z);
            seg.transform.rotation = Quaternion.Euler(0, -90, 0);
            seg.transform.localScale = new Vector3(1f, fenceScaleY, 1f);
            count++;
        }
        Debug.Log($"Fence: {count} segments + 2 gate posts (gate {gateLeft} to {gateRight}), height x{fenceScaleY}.");
    }

    static void PlaceChurch()
    {
        var churchPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Village + "/building_church_green.fbx");
        if (churchPrefab == null) { Debug.LogWarning("Church FBX not found!"); return; }

        var church = (GameObject)Object.Instantiate(churchPrefab);
        church.transform.position = new Vector3(0f, 0, 0f);
        church.transform.localScale = Vector3.one * 8f;
        AddColliders(church);
        Debug.Log("Church placed from FBX.");
    }

    static void PlaceCornerTowers(float left, float right, float bottom, float top)
    {
        var towerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Village + "/building_tower_A_green.fbx");
        if (towerPrefab == null) { Debug.LogWarning("Tower FBX not found!"); return; }

        var corners = new Vector3[] {
            new Vector3(left, 0, bottom),
            new Vector3(right, 0, bottom),
            new Vector3(right, 0, top),
            new Vector3(left, 0, top),
        };
        foreach (var pos in corners)
        {
            var tower = (GameObject)Object.Instantiate(towerPrefab);
            tower.transform.position = pos;
            tower.transform.localScale = new Vector3(3.5f, 6f, 3.5f);
            tower.transform.rotation = Quaternion.Euler(0, 45, 0);
            AddColliders(tower);
        }
        Debug.Log($"Placed {corners.Length} corner towers from FBX.");
    }

    static void PlaceHouses()
    {
        var housePaths = new[] {
            Village + "/building_home_A_green.fbx",
            Village + "/building_home_B_green.fbx",
            Village + "/building_tavern_green.fbx",
            Village + "/building_market_green.fbx",
            Village + "/building_barracks_green.fbx",
            Village + "/building_blacksmith_green.fbx",
            Village + "/building_well_green.fbx",
            Village + "/building_lumbermill_green.fbx",
        };

        var houses = new List<GameObject>();
        foreach (var p in housePaths)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(p);
            if (prefab != null) houses.Add(prefab);
        }

        if (houses.Count == 0) { Debug.LogWarning("No house prefabs found!"); return; }

        var positions = new (Vector3 pos, float rotY)[] {
            (new Vector3(-20f, 0, -15f), 0f),
            (new Vector3(20f, 0, -15f), 0f),
            (new Vector3(-25f, 0, 0f), 0f),
            (new Vector3(25f, 0, 0f), 0f),
            (new Vector3(-20f, 0, 15f), 0f),
            (new Vector3(20f, 0, 15f), 0f),
            (new Vector3(-10f, 0, 25f), 0f),
            (new Vector3(10f, 0, 25f), 0f),
            (new Vector3(0f, 0, -25f), 0f),
            (new Vector3(-35f, 0, -20f), 0f),
        };

        int placed = 0;
        foreach (var (pos, rotY) in positions)
        {
            if (placed >= houses.Count) break;
            var house = (GameObject)Object.Instantiate(houses[placed]);
            house.transform.position = pos;
            house.transform.localScale = Vector3.one * 6f;
            house.transform.rotation = Quaternion.Euler(0, rotY, 0);
            AddColliders(house);
            placed++;
        }
        Debug.Log($"Placed {placed} houses from assets.");
    }

    static void PlaceWatermill()
    {
        var wmPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(Village + "/building_watermill_green.fbx");
        if (wmPrefab == null) { Debug.LogWarning("Watermill FBX not found!"); return; }

        var wm = (GameObject)Object.Instantiate(wmPrefab);
        wm.transform.position = new Vector3(78f, 0, 0f);
        wm.transform.localScale = Vector3.one * 6f;
        wm.transform.rotation = Quaternion.Euler(0, -90f, 0);
        AddColliders(wm);
        Debug.Log("Watermill placed from FBX.");
    }

    static void AddColliders(GameObject go)
    {
        var filters = go.GetComponentsInChildren<MeshFilter>();
        foreach (var f in filters)
        {
            if (f.sharedMesh == null) continue;
            var mc = f.gameObject.AddComponent<MeshCollider>();
            mc.sharedMesh = f.sharedMesh;
            mc.convex = false;
        }
    }

    static Texture2D CreateSerbianFlagTexture()
    {
        int w = 256, h = 170;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        Color red = new Color(0.9f, 0.08f, 0.08f);
        Color blue = new Color(0.05f, 0.18f, 0.55f);
        Color white = Color.white;

        // Serbian tricolor: red top, blue middle, white bottom.
        // Texture y=0 is bottom-left, so invert the order.
        for (int y = 0; y < h; y++)
        {
            Color stripe = y < h / 3 ? white : (y < 2 * h / 3 ? blue : red);
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, stripe);
        }

        // Stylized coat of arms near the hoist (left side), as on the Serbian state flag
        int cx = w / 4, cy = h / 2;
        int shieldW = 44, shieldH = 52;
        for (int y = cy - shieldH / 2; y < cy + shieldH / 2; y++)
            for (int x = cx - shieldW / 2; x < cx + shieldW / 2; x++)
                tex.SetPixel(x, y, red);

        int crossThick = 6;
        for (int y = cy - shieldH / 2; y < cy + shieldH / 2; y++)
            for (int x = cx - crossThick / 2; x < cx + crossThick / 2; x++)
                tex.SetPixel(x, y, white);
        for (int x = cx - shieldW / 2; x < cx + shieldW / 2; x++)
            for (int y = cy - crossThick / 2; y < cy + crossThick / 2; y++)
                tex.SetPixel(x, y, white);

        // Four stylized ocila (small white crosses)
        int ox = shieldW / 4, oy = shieldH / 4;
        int[][] ocila = {
            new[] { cx - ox, cy + oy },
            new[] { cx + ox, cy + oy },
            new[] { cx - ox, cy - oy },
            new[] { cx + ox, cy - oy }
        };
        foreach (var o in ocila)
        {
            for (int dx = -3; dx <= 3; dx++) tex.SetPixel(o[0] + dx, o[1], white);
            for (int dy = -3; dy <= 3; dy++) tex.SetPixel(o[0], o[1] + dy, white);
        }

        tex.Apply();
        return tex;
    }

    static Texture2D EnsureSerbianFlagAsset()
    {
        string path = "Assets/Textures/SerbianFlag.png";
        var existing = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (existing != null) return existing;

        var tex = CreateSerbianFlagTexture();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();
        return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
    }

    static void PlaceNatureFeatures()
    {
        var wheatMat = new Material(Shader.Find("Standard"));
        wheatMat.color = new Color(0.85f, 0.7f, 0.15f);
        for (int i = 0; i < 40; i++)
        {
            float wx = Random.Range(75f, 95f);
            float wz = Random.Range(-40f, 0f);
            var stalk = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            stalk.name = "WheatStalk";
            stalk.transform.position = new Vector3(wx, 0.3f, wz);
            stalk.transform.localScale = new Vector3(0.04f, 0.3f, 0.04f);
            stalk.GetComponent<Renderer>().sharedMaterial = wheatMat;
        }

        var flowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Flowers/PT_Poppy_02.prefab");
        if (flowerPrefab != null)
        {
            for (int i = 0; i < 20; i++)
            {
                float fx = Random.Range(-120f, 120f);
                float fz = Random.Range(-110f, 110f);
                if (Mathf.Abs(fx) < 75f && Mathf.Abs(fz) < 55f) continue;
                var f = (GameObject)Object.Instantiate(flowerPrefab);
                f.transform.position = new Vector3(fx, 0, fz);
                f.transform.localScale = Vector3.one * Random.Range(0.5f, 1.2f);
            }
        }

        var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Generic_Rock_01.prefab");
        if (rockPrefab != null)
        {
            for (int i = 0; i < 15; i++)
            {
                float rx = Random.Range(80f, 140f) * (Random.value > 0.5f ? 1 : -1);
                float rz = Random.Range(60f, 120f) * (Random.value > 0.5f ? 1 : -1);
                var r = (GameObject)Object.Instantiate(rockPrefab);
                r.transform.position = new Vector3(rx, 0, rz);
                r.transform.localScale = Vector3.one * Random.Range(2f, 5f);
            }
        }

        var orePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Rocks/PT_Ore_Rock_01.prefab");
        if (orePrefab != null)
        {
            for (int i = 0; i < 8; i++)
            {
                float rx = Random.Range(-145f, -85f);
                float rz = Random.Range(-135f, -75f);
                var r = (GameObject)Object.Instantiate(orePrefab);
                r.transform.position = new Vector3(rx, 0, rz);
                r.transform.localScale = Vector3.one * Random.Range(1.5f, 3f);
            }
        }

        var pathMat = new Material(Shader.Find("Standard"));
        pathMat.color = new Color(0.5f, 0.35f, 0.2f);
        for (float px = -1f; px <= 1f; px += 2f)
        {
            for (float pz = -50f; pz <= 0f; pz += 1.5f)
            {
                var tile = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tile.name = "PathTile";
                tile.transform.position = new Vector3(px, 0.01f, pz);
                tile.transform.localScale = new Vector3(0.9f, 0.02f, 0.9f);
                tile.GetComponent<Renderer>().sharedMaterial = pathMat;
            }
        }

        // River removed for now — it was clipping into the right edge of the screen.
        // BuildProceduralRiver();

        var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pond.name = "Pond";
        pond.transform.position = new Vector3(-90f, 0.02f, -60f);
        pond.transform.localScale = new Vector3(8f, 0.05f, 8f);
        var pondMat = MakeWaterMaterial(0.15f, 0.45f, 0.7f, 0.85f);
        pond.GetComponent<Renderer>().sharedMaterial = pondMat;
    }

    static Material MakeWaterMaterial(float r, float g, float b, float a)
    {
        var shader = Shader.Find("Standard");
        var m = new Material(shader);
        m.color = new Color(r, g, b, a);
        m.SetFloat("_Mode", 3);
        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        m.SetInt("_ZWrite", 0);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.85f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.0f);
        if (m.HasProperty("_SpecColor")) m.SetColor("_SpecColor", new Color(0.6f, 0.8f, 1f, 1f));
        return m;
    }

    static void BuildProceduralRiver()
    {
        var river = new GameObject("River");

        // Use the Polytope Studio water material with waves/foam instead of a flat blue plane.
        var waterMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Polytope Studio/Lowpoly_Environments/Sources/Materials/PT_Water_mat.mat");
        if (waterMat == null)
        {
            Debug.LogWarning("PT_Water_mat not found, falling back to flat water.");
            waterMat = MakeWaterMaterial(0.28f, 0.62f, 0.78f, 0.78f);
        }

        var bankMat = new Material(Shader.Find("Standard"));
        bankMat.color = new Color(0.45f, 0.36f, 0.22f);

        var stoneMat = new Material(Shader.Find("Standard"));
        stoneMat.color = new Color(0.45f, 0.45f, 0.42f);

        var path = new (Vector3 pos, float width)[] {
            (new Vector3( 95f, 0.05f,  60f), 5.5f),
            (new Vector3( 88f, 0.05f,  40f), 5.0f),
            (new Vector3( 82f, 0.05f,  20f), 5.0f),
            (new Vector3( 80f, 0.05f,   0f), 5.5f),
            (new Vector3( 82f, 0.05f, -20f), 5.0f),
            (new Vector3( 88f, 0.05f, -40f), 5.0f),
            (new Vector3( 95f, 0.05f, -60f), 5.5f),
        };

        var segNames = new[] { "Seg_60", "Seg_40", "Seg_20", "Seg_00", "Seg_-20", "Seg_-40", "Seg_-60" };

        for (int i = 0; i < path.Length - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];

            var seg = GameObject.CreatePrimitive(PrimitiveType.Plane);
            seg.name = "River." + segNames[i];
            seg.transform.SetParent(river.transform, false);

            var mid = (a.pos + b.pos) * 0.5f;
            float length = Vector3.Distance(a.pos, b.pos);
            float width = (a.width + b.width) * 0.5f;

            seg.transform.position = new Vector3(mid.x, 0.05f, mid.z);
            seg.transform.localScale = new Vector3(width / 10f, 1f, length / 10f);
            Vector3 dir = (b.pos - a.pos).normalized;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            seg.transform.rotation = Quaternion.Euler(0f, yaw, 0f);

            Object.DestroyImmediate(seg.GetComponent<MeshCollider>());

            var rend = seg.GetComponent<Renderer>();
            rend.sharedMaterial = waterMat;

            var flow = seg.AddComponent<RiverFlow>();
            flow.flowDirection = new Vector2(Mathf.Sin(yaw * Mathf.Deg2Rad), Mathf.Cos(yaw * Mathf.Deg2Rad));
            flow.speed = 0.12f;
            flow.tiling = new Vector2(width / 4f, length / 4f);
        }

        for (int i = 0; i < path.Length - 1; i++)
        {
            var a = path[i];
            var b = path[i + 1];
            Vector3 dir = (b.pos - a.pos).normalized;
            Vector3 perp = new Vector3(-dir.z, 0, dir.x);
            float w = (a.width + b.width) * 0.5f * 0.5f;
            float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

            for (int side = -1; side <= 1; side += 2)
            {
                var bank = GameObject.CreatePrimitive(PrimitiveType.Cube);
                bank.name = "Bank." + segNames[i] + (side > 0 ? ".N" : ".S");
                bank.transform.SetParent(river.transform, false);

                var mid = (a.pos + b.pos) * 0.5f + perp * side * (w + 0.6f);
                float length = Vector3.Distance(a.pos, b.pos) + 1.2f;
                bank.transform.position = new Vector3(mid.x, 0.18f, mid.z);
                bank.transform.localScale = new Vector3(1.6f, 0.6f, length);
                bank.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
                bank.GetComponent<Renderer>().sharedMaterial = bankMat;
            }
        }

        var stonePositions = new Vector3[] {
            new Vector3( 86f, 0.3f,  35f), new Vector3( 92f, 0.3f,  25f), new Vector3( 84f, 0.3f, -10f),
            new Vector3( 86f, 0.3f, -32f), new Vector3( 93f, 0.3f, -45f), new Vector3( 81f, 0.3f,  45f),
            new Vector3( 90f, 0.3f, -55f), new Vector3( 85f, 0.3f,  55f),
        };
        foreach (var sp in stonePositions)
        {
            var stone = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            stone.name = "RiverStone";
            stone.transform.SetParent(river.transform, false);
            stone.transform.position = sp;
            stone.transform.localScale = Vector3.one * Random.Range(0.5f, 1.1f);
            stone.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            stone.GetComponent<Renderer>().sharedMaterial = stoneMat;
            Object.DestroyImmediate(stone.GetComponent<SphereCollider>());
        }

        var reedMat = new Material(Shader.Find("Standard"));
        reedMat.color = new Color(0.35f, 0.55f, 0.15f);
        for (int i = 0; i < 30; i++)
        {
            int pi = Random.Range(0, path.Length - 1);
            var a = path[pi];
            var b = path[pi + 1];
            Vector3 dir = (b.pos - a.pos).normalized;
            Vector3 perp = new Vector3(-dir.z, 0, dir.x);
            float t = Random.value;
            Vector3 center = Vector3.Lerp(a.pos, b.pos, t);
            float w = Mathf.Lerp(a.width, b.width, t) * 0.55f;
            int side = Random.value > 0.5f ? 1 : -1;
            var reed = GameObject.CreatePrimitive(PrimitiveType.Cube);
            reed.name = "Reed";
            reed.transform.SetParent(river.transform, false);
            reed.transform.position = center + perp * side * w;
            reed.transform.localScale = new Vector3(0.08f, 1.2f, 0.08f);
            reed.transform.rotation = Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(0, 360), Random.Range(-15f, 15f));
            reed.GetComponent<Renderer>().sharedMaterial = reedMat;
            Object.DestroyImmediate(reed.GetComponent<BoxCollider>());
        }

        Debug.Log("River: procedural, 6 segments, banks, stones, reeds (RiverFlow animates UV).");
    }

    static void PlaceForest()
    {
        var pine = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/Trees/PT_Pine_Tree_03_green.prefab");
        if (pine == null) return;

        // Dense forest on the west (left) side — this is where enemies emerge.
        for (int i = 0; i < 55; i++)
        {
            float x = Random.Range(-160f, -80f);
            float z = Random.Range(-140f, 140f);
            // Leave a small opening near the village approach.
            if (x > -100f && Mathf.Abs(z) < 30f) continue;

            var go = (GameObject)Object.Instantiate(pine);
            go.transform.position = new Vector3(x, 0, z);
            go.transform.localScale = Vector3.one * Random.Range(2.5f, 5f);
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            go.name = "ForestTree";
        }

        // A smaller grove to the north for visual framing.
        for (int i = 0; i < 15; i++)
        {
            float x = Random.Range(-60f, 60f);
            float z = Random.Range(100f, 150f);
            var go = (GameObject)Object.Instantiate(pine);
            go.transform.position = new Vector3(x, 0, z);
            go.transform.localScale = Vector3.one * Random.Range(2.5f, 4.5f);
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            go.name = "NorthTree";
        }
    }

    static void PlaceNPCs()
    {
        // Friendly NPCs removed — the village starts empty until the player builds it.
    }

    static void SetupEnemySpawner()
    {
        var spawnerObj = new GameObject("EnemySpawner");
        var spawner = spawnerObj.AddComponent<EnemySpawner>();

        spawner.wolfPrefab = EnsureWolfPrefab();
        spawner.barbarianPrefab = EnsureBarbarianPrefab();

        // Spawn points closer to the village so enemies arrive faster
        var points = new Transform[4];
        var positions = new Vector3[] {
            new Vector3(-90f, 0f, -60f),
            new Vector3(-105f, 0f,   0f),
            new Vector3(-90f, 0f,  60f),
            new Vector3(-80f, 0f,  80f)
        };
        for (int i = 0; i < positions.Length; i++)
        {
            var p = new GameObject("SpawnPoint_" + i).transform;
            p.position = positions[i];
            p.SetParent(spawnerObj.transform);
            points[i] = p;
        }
        spawner.spawnPoints = points;
        spawner.enemiesPerWave = 3;
        spawner.waveInterval = 6f;
        spawner.maxConcurrentEnemies = 12;
    }

    static void SetupVillageWalls()
    {
        string fencePath = "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab";
        var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(fencePath);
        if (fencePrefab == null)
        {
            Debug.LogWarning("Fence prefab not found, VillageWalls disabled.");
            return;
        }

        var wallsObj = new GameObject("VillageWalls");
        var walls = wallsObj.AddComponent<VillageWalls>();
        walls.fencePrefab = fencePrefab;
        walls.fenceHeight = 2.5f;
    }

    static GameObject EnsureBarbarianPrefab()
    {
        string path = "Assets/Prefabs/BarbarianEnemy.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Materials");

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Kayak + "/Characters/fbx/Barbarian.fbx");
        if (prefab == null)
        {
            Debug.LogWarning("Barbarian FBX not found, enemy spawner will skip barbarians.");
            return null;
        }

        var go = (GameObject)Object.Instantiate(prefab);
        go.name = "BarbarianEnemy";
        go.transform.localScale = Vector3.one * 2.8f;
        go.transform.position = new Vector3(0, 0.3f, 0);

        // Assign animator controller so the barbarian walks instead of sliding.
        var barbAnimator = go.GetComponentInChildren<Animator>();
        if (barbAnimator != null)
        {
            barbAnimator.runtimeAnimatorController = EnsureBarbarianAnimatorController();
            barbAnimator.applyRootMotion = false;
        }

        // Replace FBX materials with a saved Standard material to avoid pink shader issues.
        string matPath = "Assets/Materials/BarbarianEnemy.mat";
        var barbarianMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (barbarianMat == null)
        {
            barbarianMat = new Material(Shader.Find("Standard"));
            barbarianMat.color = new Color(0.55f, 0.32f, 0.22f);
            AssetDatabase.CreateAsset(barbarianMat, matPath);
        }
        foreach (var rend in go.GetComponentsInChildren<Renderer>())
            rend.sharedMaterial = barbarianMat;

        var cap = go.AddComponent<CapsuleCollider>();
        cap.height = 2f;
        cap.radius = 0.5f;
        cap.center = new Vector3(0, 1f, 0);

        // TODO: Add barbarian weapon from a free asset (check Downloads folder)
        var enemy = go.AddComponent<Enemy>();
        enemy.maxHealth = 4;
        enemy.goldReward = 8;
        enemy.moveSpeed = 5.5f;
        enemy.enemyName = "Barbarian";

        PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject EnsureWolfPrefab()
    {
        string path = "Assets/Prefabs/Wolf.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Prefabs");

        // Prefer the imported Wolf pack if the user has it in the project.
        string importedPath = "Assets/Wolf/LRP(Built-in)/Wolf/Prefab/Wolf_LRP.prefab";
        var imported = AssetDatabase.LoadAssetAtPath<GameObject>(importedPath);
        if (imported != null)
        {
            var wolf = (GameObject)Object.Instantiate(imported);
            wolf.name = "Wolf";
            wolf.transform.localScale = Vector3.one * 3.5f;
            wolf.transform.position = new Vector3(0, 0.2f, 0);

            // Replace the broken demo Animator Controller (no parameters, auto-cycles)
            // with our proper controller that supports Speed/Death parameters.
            var wolfAnimator = wolf.GetComponentInChildren<Animator>();
            if (wolfAnimator != null)
            {
                wolfAnimator.runtimeAnimatorController = EnsureWolfAnimatorController();
                wolfAnimator.applyRootMotion = false;
            }

            // The imported prefab usually has its own collider; if not, add one.
            if (wolf.GetComponentInChildren<Collider>() == null)
            {
                var cap = wolf.AddComponent<CapsuleCollider>();
                cap.height = 1.6f;
                cap.radius = 0.4f;
                cap.center = new Vector3(0, 0.8f, 0);
            }

            var enemy = wolf.AddComponent<Enemy>();
            enemy.maxHealth = 2;
            enemy.goldReward = 5;
            enemy.moveSpeed = 6.5f;
            enemy.enemyName = "Wolf";

            PrefabUtility.SaveAsPrefabAsset(wolf, path);
            Object.DestroyImmediate(wolf);
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }

        // Fallback procedural wolf if the imported pack is missing.
        Directory.CreateDirectory("Assets/Materials");
        string matPath = "Assets/Materials/Wolf.mat";
        var wolfMat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (wolfMat == null)
        {
            wolfMat = new Material(Shader.Find("Standard"));
            wolfMat.color = new Color(0.45f, 0.42f, 0.38f);
            AssetDatabase.CreateAsset(wolfMat, matPath);
        }

        var wolf2 = new GameObject("Wolf");
        wolf2.transform.localScale = Vector3.one * 3.5f;

        // Body — oriented along +Z so the wolf actually runs forward.
        var body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(wolf2.transform);
        body.transform.localPosition = new Vector3(0f, 0.65f, 0f);
        body.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        body.transform.localScale = new Vector3(0.55f, 1.1f, 0.55f);
        body.GetComponent<Renderer>().sharedMaterial = wolfMat;
        Object.DestroyImmediate(body.GetComponent<CapsuleCollider>());

        // Head
        var head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(wolf2.transform);
        head.transform.localPosition = new Vector3(0f, 0.85f, 1.1f);
        head.transform.localScale = Vector3.one * 0.45f;
        head.GetComponent<Renderer>().sharedMaterial = wolfMat;
        Object.DestroyImmediate(head.GetComponent<SphereCollider>());

        // Ears
        for (int side = -1; side <= 1; side += 2)
        {
            var ear = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ear.name = "Ear";
            ear.transform.SetParent(wolf2.transform);
            ear.transform.localPosition = new Vector3(side * 0.18f, 1.15f, 1.25f);
            ear.transform.localRotation = Quaternion.Euler(-15f, 0f, side * 10f);
            ear.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);
            ear.GetComponent<Renderer>().sharedMaterial = wolfMat;
            Object.DestroyImmediate(ear.GetComponent<CapsuleCollider>());
        }

        // Tail
        var tail = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        tail.name = "Tail";
        tail.transform.SetParent(wolf2.transform);
        tail.transform.localPosition = new Vector3(0f, 0.75f, -1.1f);
        tail.transform.localRotation = Quaternion.Euler(-70f, 0f, 0f);
        tail.transform.localScale = new Vector3(0.12f, 0.55f, 0.12f);
        tail.GetComponent<Renderer>().sharedMaterial = wolfMat;
        Object.DestroyImmediate(tail.GetComponent<CapsuleCollider>());

        // Legs
        var legs = new Transform[4];
        int legIndex = 0;
        for (int x = -1; x <= 1; x += 2)
        {
            for (int z = -1; z <= 1; z += 2)
            {
                var leg = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                leg.name = "Leg";
                leg.transform.SetParent(wolf2.transform);
                leg.transform.localPosition = new Vector3(x * 0.25f, 0.25f, z * 0.55f);
                leg.transform.localScale = new Vector3(0.12f, 0.5f, 0.12f);
                leg.GetComponent<Renderer>().sharedMaterial = wolfMat;
                Object.DestroyImmediate(leg.GetComponent<CapsuleCollider>());
                legs[legIndex++] = leg.transform;
            }
        }

        // Collider
        var cap2 = wolf2.AddComponent<CapsuleCollider>();
        cap2.direction = 2; // Z-axis
        cap2.height = 2.4f;
        cap2.radius = 0.45f;
        cap2.center = new Vector3(0, 0.65f, 0);

        var enemy2 = wolf2.AddComponent<Enemy>();
        enemy2.maxHealth = 2;
        enemy2.goldReward = 5;
        enemy2.moveSpeed = 4.5f;
        enemy2.enemyName = "Wolf";

        var walker = wolf2.AddComponent<SimpleWalker>();
        walker.body = body.transform;
        walker.legs = legs;

        PrefabUtility.SaveAsPrefabAsset(wolf2, path);
        Object.DestroyImmediate(wolf2);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static AnimationClip LoadKayKitClip(string fbxPath, string clipName)
    {
        var assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
        string log = $"Searching for '{clipName}' in {fbxPath}: ";
        AnimationClip firstClip = null;
        foreach (var obj in assets)
        {
            if (obj is AnimationClip clip)
            {
                if (firstClip == null) firstClip = clip;
                log += $"[{clip.name}] ";
                if (clip.name == clipName) { Debug.Log(log + "FOUND exact match."); return clip; }
            }
        }
        if (firstClip != null)
        {
            // contains match
            foreach (var obj in assets)
            {
                if (obj is AnimationClip clip && clip.name.ToLower().Contains(clipName.ToLower()))
                { Debug.Log(log + "FOUND contains match: " + clip.name); return clip; }
            }
            Debug.Log(log + $"No match for '{clipName}', using first clip: {firstClip.name}");
            return firstClip;
        }

        // No clips at all in this FBX – search all KayKit FBX files as fallback
        Debug.LogWarning(log + $"NO clips found in {fbxPath}. Searching ALL KayKit FBX files...");
        // Also generate procedural clips as last-resort fallback (see GenerateProceduralClip)
        string kayakDir = "Assets/3D/KayKit";
        string[] fbxFiles = System.IO.Directory.GetFiles(Application.dataPath + "/3D/KayKit",
            "*.fbx", System.IO.SearchOption.AllDirectories);
        foreach (string fullPath in fbxFiles)
        {
            string relPath = "Assets" + fullPath.Substring(Application.dataPath.Length);
            var fbxAssets = AssetDatabase.LoadAllAssetsAtPath(relPath);
            foreach (var obj in fbxAssets)
            {
                if (obj is AnimationClip clip)
                {
                    if (clip.name.ToLower().Contains(clipName.ToLower()))
                    { Debug.Log($"Found '{clipName}' in {relPath}: {clip.name}"); return clip; }
                }
            }
        }
        // Absolute last resort: return any clip from any KayKit FBX
        foreach (string fullPath in fbxFiles)
        {
            string relPath = "Assets" + fullPath.Substring(Application.dataPath.Length);
            var fbxAssets = AssetDatabase.LoadAllAssetsAtPath(relPath);
            foreach (var obj in fbxAssets)
            {
                if (obj is AnimationClip clip)
                { Debug.LogWarning($"Fallback clip from {relPath}: {clip.name}"); return clip; }
            }
        }
        Debug.LogError($"NO animation clips found in ANY KayKit FBX file!");
        return null;
    }

    static AnimatorController EnsureHeroAnimatorController()
    {
        string path = "Assets/Animations/HeroAnimator.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Animations");

        string movementFbx = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";
        string generalFbx = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx";

        var walkClip = LoadKayKitClip(movementFbx, "Walking_A");
        var idleClip = LoadKayKitClip(generalFbx, "Idle_A");

        var controller = new AnimatorController();
        controller.name = "HeroAnimator";
        AssetDatabase.CreateAsset(controller, path);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddLayer("Base Layer");

        var rootStateMachine = controller.layers[0].stateMachine;

        var idleState = rootStateMachine.AddState("Idle");
        if (idleClip != null) idleState.motion = idleClip;

        var walkState = rootStateMachine.AddState("Walk");
        if (walkClip != null) walkState.motion = walkClip;

        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.duration = 0.1f;

        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.duration = 0.1f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created AnimatorController at {path} (Walk: {walkClip?.name}, Idle: {idleClip?.name})");
        return controller;
    }

    static AnimatorController EnsureBarbarianAnimatorController()
    {
        string path = "Assets/Animations/BarbarianAnimator.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Animations");

        string movementFbx = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_MovementBasic.fbx";
        string generalFbx = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE/Animations/fbx/Rig_Medium/Rig_Medium_General.fbx";

        var walkClip = LoadKayKitClip(movementFbx, "Walking_A");
        var idleClip = LoadKayKitClip(generalFbx, "Idle_A");
        var deathClip = LoadKayKitClip(generalFbx, "Death_A");

        var controller = new AnimatorController();
        controller.name = "BarbarianAnimator";
        AssetDatabase.CreateAsset(controller, path);

        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        controller.AddLayer("Base Layer");

        var rootStateMachine = controller.layers[0].stateMachine;

        var idleState = rootStateMachine.AddState("Idle");
        if (idleClip != null) idleState.motion = idleClip;

        var walkState = rootStateMachine.AddState("Walk");
        if (walkClip != null) walkState.motion = walkClip;

        var attackState = rootStateMachine.AddState("Attack");
        if (walkClip != null) attackState.motion = walkClip; // Use walk clip as attack placeholder

        var deathState = rootStateMachine.AddState("Death");
        if (deathClip != null) deathState.motion = deathClip;

        var idleToWalk = idleState.AddTransition(walkState);
        idleToWalk.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        idleToWalk.duration = 0.1f;

        var walkToIdle = walkState.AddTransition(idleState);
        walkToIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        walkToIdle.duration = 0.1f;

        var idleToAttack = idleState.AddTransition(attackState);
        idleToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
        idleToAttack.duration = 0.15f;
        var attackToIdle = attackState.AddTransition(idleState);
        attackToIdle.duration = 0.3f;

        var anyToDeath = rootStateMachine.AddAnyStateTransition(deathState);
        anyToDeath.AddCondition(AnimatorConditionMode.If, 0f, "Death");
        anyToDeath.duration = 0.2f;

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        Debug.Log($"Created AnimatorController at {path}");
        return controller;
    }

    static AnimatorController EnsureWolfAnimatorController()
    {
        string path = "Assets/Animations/WolfAnimator.controller";
        var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Animations");

        // Try to use the imported pack's controller as a base, adding parameters.
        string importedControllerPath = "Assets/Wolf/Animations/Wolf_Controller/WolfAnimations.controller";
        var importedCtrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(importedControllerPath);
        if (importedCtrl != null)
        {
            // Clone it into our path with added parameters
            var controller = Object.Instantiate(importedCtrl);
            controller.name = "WolfAnimator";
            AssetDatabase.CreateAsset(controller, path);

            bool hasSpeed = false, hasDeath = false;
            foreach (var p in controller.parameters)
            {
                if (p.name == "Speed") hasSpeed = true;
                if (p.name == "Death") hasDeath = true;
            }
            if (!hasSpeed) controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            if (!hasDeath) controller.AddParameter("Death", AnimatorControllerParameterType.Trigger);

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log($"WolfAnimator: cloned from imported pack with added parameters.");
            return controller;
        }

        // Fallback: create from scratch using clips from the wolf model FBX
        string wolfFbx = "Assets/Wolf/Models/Wolf.fbx";
        var walkClip = LoadKayKitClip(wolfFbx, "Walk");
        if (walkClip == null)
        {
            var assets = AssetDatabase.LoadAllAssetsAtPath(wolfFbx);
            foreach (var obj in assets)
                if (obj is AnimationClip clip) { walkClip = clip; break; }
        }

        var ctrl = new AnimatorController();
        ctrl.name = "WolfAnimator";
        AssetDatabase.CreateAsset(ctrl, path);

        ctrl.AddParameter("Speed", AnimatorControllerParameterType.Float);
        ctrl.AddParameter("Death", AnimatorControllerParameterType.Trigger);
        ctrl.AddLayer("Base Layer");

        var rootSM = ctrl.layers[0].stateMachine;

        var idleSt = rootSM.AddState("Idle");
        if (walkClip != null) idleSt.motion = walkClip;

        var walkSt = rootSM.AddState("Walk");
        if (walkClip != null) walkSt.motion = walkClip;

        var deathSt = rootSM.AddState("Death");
        if (walkClip != null) deathSt.motion = walkClip;

        var i2w = idleSt.AddTransition(walkSt);
        i2w.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
        i2w.duration = 0.1f;

        var w2i = walkSt.AddTransition(idleSt);
        w2i.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");
        w2i.duration = 0.1f;

        var anyD = rootSM.AddAnyStateTransition(deathSt);
        anyD.AddCondition(AnimatorConditionMode.If, 0f, "Death");
        anyD.duration = 0.2f;

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        Debug.Log($"WolfAnimator: created from scratch (walkClip: {walkClip?.name ?? "null"})");
        return ctrl;
    }

    static void PlaceDecorations()
    {
        // A small lake on the east side, away from the village.
        var lake = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        lake.name = "Lake";
        lake.transform.position = new Vector3(95f, 0.02f, 45f);
        lake.transform.localScale = new Vector3(18f, 0.05f, 14f);
        lake.GetComponent<Renderer>().sharedMaterial = MakeWaterMaterial(0.18f, 0.48f, 0.72f, 0.8f);

        // Rocks around the lake and near the forest edge.
        var rockPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/Rocks/PT_Generic_Rock_01.prefab");
        if (rockPrefab != null)
        {
            for (int i = 0; i < 18; i++)
            {
                float rx = Random.Range(80f, 120f);
                float rz = Random.Range(20f, 80f);
                var r = (GameObject)Object.Instantiate(rockPrefab);
                r.transform.position = new Vector3(rx, 0, rz);
                r.transform.localScale = Vector3.one * Random.Range(1.5f, 3.5f);
                r.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }

        // Flowers sprinkled around the safe meadow.
        var flowerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/Flowers/PT_Poppy_02.prefab");
        if (flowerPrefab != null)
        {
            for (int i = 0; i < 45; i++)
            {
                float fx = Random.Range(-70f, 70f);
                float fz = Random.Range(-70f, 70f);
                // Keep flowers off the path and the church footprint.
                if (Mathf.Abs(fx) < 4f && fz < 5f && fz > -55f) continue;

                var f = (GameObject)Object.Instantiate(flowerPrefab);
                f.transform.position = new Vector3(fx, 0, fz);
                f.transform.localScale = Vector3.one * Random.Range(0.6f, 1.1f);
                f.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }

        // A few bushes for low cover.
        var bushPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/Bushes/PT_Bush_02.prefab");
        if (bushPrefab != null)
        {
            for (int i = 0; i < 12; i++)
            {
                float bx = Random.Range(-75f, 75f);
                float bz = Random.Range(-75f, 75f);
                if (Mathf.Abs(bx) < 6f && bz > -55f && bz < 10f) continue;
                var b = (GameObject)Object.Instantiate(bushPrefab);
                b.transform.position = new Vector3(bx, 0, bz);
                b.transform.localScale = Vector3.one * Random.Range(1f, 2f);
                b.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
            }
        }
    }

    static void PlacePlayer()
    {
        string basePath = Kayak + "/Characters/fbx";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "/Knight.fbx");
        if (prefab == null) return;

        var go = (GameObject)Object.Instantiate(prefab);
        go.name = "Knight";
        go.transform.position = new Vector3(0, 0, -55);
        go.transform.localScale = Vector3.one * 2.2f;

        go.tag = "Player";
        go.AddComponent<PlayerController3D>();

        // Assign animator controller so the hero walks instead of sliding.
        var heroAnimator = go.GetComponentInChildren<Animator>();
        if (heroAnimator != null)
        {
            heroAnimator.runtimeAnimatorController = EnsureHeroAnimatorController();
            heroAnimator.applyRootMotion = false;
        }

        // Simple sword for visual combat feedback.
        Directory.CreateDirectory("Assets/Materials");
        string swordMatPath = "Assets/Materials/HeroSword.mat";
        var swordMat = AssetDatabase.LoadAssetAtPath<Material>(swordMatPath);
        if (swordMat == null)
        {
            swordMat = new Material(Shader.Find("Standard"));
            swordMat.color = new Color(0.75f, 0.75f, 0.8f);
            AssetDatabase.CreateAsset(swordMat, swordMatPath);
        }

        var sword = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sword.name = "HeroSword";
        sword.transform.SetParent(go.transform);
        sword.transform.localPosition = new Vector3(0.65f, 1.2f, 0.4f);
        sword.transform.localRotation = Quaternion.Euler(0f, 0f, -30f);
        sword.transform.localScale = new Vector3(0.12f, 1.4f, 0.05f);
        sword.GetComponent<Renderer>().sharedMaterial = swordMat;
        Object.DestroyImmediate(sword.GetComponent<BoxCollider>());

        var weapon = go.AddComponent<SimpleWeapon>();
        weapon.weapon = sword.transform;
    }

    static void CreateSlots()
    {
        var slotManagerObj = new GameObject("SlotManager");
        slotManagerObj.AddComponent<SlotManager>();

        var churchSlot = EnsureSlotAsset("ChurchSlot", "Church", 10,
            Village + "/building_church_green.fbx",
            new Vector3(0f, 0f, 0f), Vector3.zero, Vector3.one * 8f,
            "Church built! The village has a heart.");
        CreateSlotGameObject(churchSlot, 0, true);

        var flagSlot = EnsureSlotAsset("FlagSlot", "Flag", 10,
            null,
            new Vector3(-2f, 0f, -48f), Vector3.zero, Vector3.one,
            "Flag raised! The Serbian banner flies high.");
        flagSlot.buildingPrefab = EnsureFlagPrefab();
        CreateSlotGameObject(flagSlot, 1, false);

        float left = -70f, right = 70f, bottom = -50f, top = 50f;
        var towerPositions = new Vector3[] {
            new Vector3(left, 0, bottom),
            new Vector3(right, 0, bottom),
            new Vector3(right, 0, top),
            new Vector3(left, 0, top)
        };
        for (int i = 0; i < 4; i++)
        {
            string assetName = $"TowerSlot_{i + 1}";
            var towerSlot = EnsureSlotAsset(assetName, "Tower", 10,
                Village + "/building_tower_A_green.fbx",
                towerPositions[i], new Vector3(0f, 45f, 0f), new Vector3(4f, 7.5f, 4f),
                "Tower built!");
            CreateSlotGameObject(towerSlot, i + 2, false);
        }

        // Mage tiles inside the village, near each corner tower
        Vector3[] mageOffsets = new Vector3[] {
            new Vector3( 15f, 0f,  15f),  // tower 1 (SW corner): inward
            new Vector3(-15f, 0f,  15f),  // tower 2 (SE corner): inward
            new Vector3(-15f, 0f, -15f),  // tower 3 (NE corner): inward
            new Vector3( 15f, 0f, -15f),  // tower 4 (NW corner): inward
        };
        for (int i = 0; i < 4; i++)
        {
            string assetName = $"MageSlot_{i + 1}";
            var magePos = towerPositions[i] + mageOffsets[i];
            var mageSlot = EnsureSlotAsset(assetName, "Mage Tile", 10,
                null,
                magePos, Vector3.zero, Vector3.one,
                "Mage joins the defense!");
            CreateSlotGameObject(mageSlot, 6 + i, false);
        }
    }

    static void CreateSlotGameObject(BuildSlotData data, int index, bool unlocked)
    {
        var slotObj = new GameObject("Slot_" + data.slotName);
        slotObj.transform.position = data.position;
        slotObj.transform.rotation = Quaternion.Euler(data.rotation);

        var col = slotObj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.size = new Vector3(8.5f, 4f, 8.5f);
        col.center = new Vector3(0, 2f, 0);

        var slot = slotObj.AddComponent<BuildSlot>();
        slot.Init(data, index, unlocked);
    }

    static BuildSlotData EnsureSlotAsset(string assetName, string slotName, int cost,
        string prefabPath, Vector3 pos, Vector3 rot, Vector3 scale, string message)
    {
        string path = $"Assets/Data/Slots/{assetName}.asset";
        var existing = AssetDatabase.LoadAssetAtPath<BuildSlotData>(path);
        if (existing != null)
        {
            existing.slotName = slotName;
            existing.cost = cost;
            existing.position = pos;
            existing.rotation = rot;
            existing.scale = scale;
            existing.completedMessage = message;
            if (!string.IsNullOrEmpty(prefabPath))
                existing.buildingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            EditorUtility.SetDirty(existing);
            return existing;
        }

        Directory.CreateDirectory("Assets/Data/Slots");
        var asset = ScriptableObject.CreateInstance<BuildSlotData>();
        asset.name = assetName;
        asset.slotName = slotName;
        asset.cost = cost;
        asset.position = pos;
        asset.rotation = rot;
        asset.scale = scale;
        asset.completedMessage = message;
        if (!string.IsNullOrEmpty(prefabPath))
            asset.buildingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        return asset;
    }

    static GameObject EnsureFlagPrefab()
    {
        string path = "Assets/Prefabs/Flag.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Materials");

        var flagTex = EnsureSerbianFlagAsset();

        string flagMatPath = "Assets/Materials/Flag.mat";
        var flagMat = AssetDatabase.LoadAssetAtPath<Material>(flagMatPath);
        if (flagMat == null)
        {
            flagMat = new Material(Shader.Find("Unlit/Texture"));
            flagMat.mainTexture = flagTex;
            AssetDatabase.CreateAsset(flagMat, flagMatPath);
        }

        string poleMatPath = "Assets/Materials/FlagPole.mat";
        var poleMat = AssetDatabase.LoadAssetAtPath<Material>(poleMatPath);
        if (poleMat == null)
        {
            poleMat = new Material(Shader.Find("Standard"));
            poleMat.color = new Color(0.6f, 0.6f, 0.6f);
            AssetDatabase.CreateAsset(poleMat, poleMatPath);
        }

        var flag = new GameObject("Flag");

        var pole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pole.name = "FlagPole";
        pole.transform.SetParent(flag.transform);
        pole.transform.localPosition = new Vector3(0f, 5f, 0f);
        pole.transform.localScale = new Vector3(0.2f, 10f, 0.2f);
        pole.GetComponent<Renderer>().sharedMaterial = poleMat;
        Object.DestroyImmediate(pole.GetComponent<CapsuleCollider>());

        for (int side = 0; side < 2; side++)
        {
            var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name = side == 0 ? "FlagFront" : "FlagBack";
            quad.transform.SetParent(flag.transform);
            quad.transform.localPosition = new Vector3(2.2f, 9f, 0f);
            quad.transform.localScale = new Vector3(4.5f, 3f, 1f);
            quad.transform.rotation = Quaternion.Euler(0, side * 180f, 0);
            Object.DestroyImmediate(quad.GetComponent<MeshCollider>());
            quad.GetComponent<Renderer>().sharedMaterial = flagMat;
        }

        PrefabUtility.SaveAsPrefabAsset(flag, path);
        Object.DestroyImmediate(flag);
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static void EnsureGoldIcon()
    {
        string path = "Assets/Resources/HUDIcons/gold_icon.png";
        bool existed = File.Exists(path);

        int size = 128;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0, 0, 0, 0);
        Color gold = new Color(1f, 0.84f, 0.12f);
        Color goldMid = new Color(0.95f, 0.7f, 0.08f);
        Color goldDark = new Color(0.7f, 0.45f, 0.04f);
        Color shine = new Color(1f, 0.96f, 0.6f);

        int r = size / 2 - 2;
        int cx = size / 2, cy = size / 2;
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                int dx = x - cx, dy = y - cy;
                int d2 = dx * dx + dy * dy;
                if (d2 > r * r) { tex.SetPixel(x, y, clear); continue; }

                if (d2 > (r - 4) * (r - 4)) tex.SetPixel(x, y, goldDark);
                else if (dy > size / 6 && dx < size / 6) tex.SetPixel(x, y, shine);
                else tex.SetPixel(x, y, gold);
            }
        }

        // Stylized "$" in the centre to read as coin/money
        int thick = 8;
        for (int x = cx - 10; x <= cx + 10; x++)
            for (int y = cy - thick / 2; y <= cy + thick / 2; y++)
                tex.SetPixel(x, y, goldMid);
        for (int y = cy - 22; y <= cy + 22; y++)
            for (int x = cx - thick / 2; x <= cx + thick / 2; x++)
                tex.SetPixel(x, y, goldMid);
        for (int x = cx - 14; x <= cx + 14; x++)
        {
            tex.SetPixel(x, cy + 20, goldMid);
            tex.SetPixel(x, cy - 20, goldMid);
        }

        tex.Apply();
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.Refresh();

        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.SaveAndReimport();
        }

        Debug.Log($"Generated gold coin icon at {path}");
    }

    static void SetupUI()
    {
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        // Add EventSystem for UI input (clicks, taps)
        var es = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (es == null)
        {
            var esObj = new GameObject("EventSystem");
            esObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            esObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
        canvasObj.AddComponent<HUDController>();
        canvasObj.AddComponent<HUDInfoPanel>();
        canvasObj.AddComponent<MinimapController>();
        canvasObj.AddComponent<MobileControls>();
    }

    static void SetupCamera()
    {
        var cam = Camera.main;
        if (cam == null)
        {
            var obj = new GameObject("Main Camera");
            cam = obj.AddComponent<Camera>();
            obj.tag = "MainCamera";
        }

        cam.orthographic = true;
        cam.orthographicSize = 30f;
        cam.transform.position = new Vector3(0, 28, -30);
        cam.transform.rotation = Quaternion.Euler(35, 0, 0);
        cam.farClipPlane = 300f;
        cam.nearClipPlane = 0.5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.25f, 0.5f, 0.18f);

        var follow = cam.gameObject.AddComponent<CameraFollow3D>();
        follow.baseDist = 28f;
        follow.currentDist = 28f;
        follow.minDist = 10f;
        follow.maxDist = 60f;
        follow.pitch = 35f;
        follow.yaw = 0f;
        follow.smoothSpeed = 6f;
    }

    static void SetupLights()
    {
        if (Object.FindAnyObjectByType<Light>() != null) return;

        var lightObj = new GameObject("Directional Light");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.intensity = 1.3f;
        light.shadows = LightShadows.None;
    }

    static void SetupPlayerSettings()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = true;
        PlayerSettings.resizableWindow = true;

        // Mobile-first: lock to landscape on phones/tablets
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        PlayerSettings.allowedAutorotateToLandscapeLeft = true;
        PlayerSettings.allowedAutorotateToLandscapeRight = true;
        PlayerSettings.allowedAutorotateToPortrait = false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;

        // iOS/Android common quality of life
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, "com.yourname.fromzerotohero");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.yourname.fromzerotohero");
    }
}
