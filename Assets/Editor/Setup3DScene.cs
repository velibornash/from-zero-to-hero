using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public class Setup3DScene
{
    const string Village = "Assets/3D/Village";
    const string NaturePrefabs = "Assets/Polytope Studio/Lowpoly_Environments/Prefabs";
    const string Kayak = "Assets/3D/KayKit/KayKit_Adventurers_2.0_FREE";

    [MenuItem("Tools/Setup 3D Scene")]
    public static void Build()
    {
        Cleanup();
        CreateGround();
        PlaceVillage();
        PlaceNatureFeatures();
        PlaceForest();
        PlaceNPCs();
        PlacePlayer();
        CreateBuildZones();
        SetupUI();
        SetupCamera();
        SetupLights();
        SetupPlayerSettings();

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(scene.path))
            EditorSceneManager.SaveScene(scene);
        else
            EditorSceneManager.SaveScene(scene, "Assets/Main.unity");

        Debug.Log("Scene saved! Press Play to see the village.");
    }

    public static GameObject CreateInfoPanel()
    {
        var canvasObj = GameObject.Find("InfoPanelCanvas");
        if (canvasObj != null) return canvasObj;
        canvasObj = new GameObject("InfoPanelCanvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<ObjectClickInfo>();
        return canvasObj;
    }

    static void Cleanup()
    {
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
        var go = GameObject.CreatePrimitive(PrimitiveType.Plane);
        go.name = "Ground";
        go.transform.localScale = new Vector3(30, 1, 28);
        var tex = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/3D/NatureLite/T_Landscape_Grass.png");
        var mat = new Material(Shader.Find("Standard"));
        if (tex != null) { mat.mainTexture = tex; mat.mainTextureScale = new Vector2(200, 200); }
        else { mat.color = new Color(0.25f, 0.5f, 0.18f); }
        go.GetComponent<Renderer>().material = mat;
    }

    static void PlaceVillage()
    {
        float left = -70f, right = 70f, bottom = -50f, top = 50f;

        PlaceFence(left, right, bottom, top);
        PlaceChurch();
        PlaceCornerTowers(left, right, bottom, top);
        PlaceHouses();
        PlaceWatermill();
    }

    static void PlaceFence(float left, float right, float bottom, float top)
    {
        var fencePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
            "Assets/Polytope Studio/Lowpoly_Village/Prefabs/Modular/Fence/PT_Modular_Fence_Wood_01.prefab");
        if (fencePrefab == null) { Debug.LogWarning("Fence prefab not found!"); return; }

        int count = 0;
        float step = 2f, fy = 0;
        float gateLeft = -5f, gateRight = 5f;
        const float fenceScaleY = 3f;

        for (float x = left + 1f; x <= right - 1f; x += step)
        {
            if (x >= gateLeft && x <= gateRight) continue;
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
            post.GetComponent<Renderer>().material = postMat;
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
        AddBuildingLabel(church, "Church");
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
            AddBuildingLabel(house, "House " + (char)('A' + placed));
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
        AddBuildingLabel(wm, "Watermill");
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

    static void AddBuildingLabel(GameObject building, string name)
    {
        float topY = 3f;
        var rends = building.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            var b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            topY = b.max.y + 1f;
        }
        var label = new GameObject("BuildingLabel");
        label.transform.SetParent(building.transform);
        label.transform.position = new Vector3(building.transform.position.x, topY, building.transform.position.z);
        var tm = label.AddComponent<TextMesh>();
        tm.text = name;
        tm.fontSize = 36;
        tm.fontStyle = FontStyle.Bold;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = Color.black;
        label.transform.localScale = new Vector3(0.08f, 0.08f, 0.08f);
        label.AddComponent<Billboard>();
    }

    static void AddBuildZoneLabel(GameObject zone, int cost)
    {
        var label = new GameObject("ZoneCostLabel");
        label.transform.SetParent(zone.transform);
        label.transform.localPosition = new Vector3(0, 0.7f, 0);
        var tm = label.AddComponent<TextMesh>();
        tm.text = $"\u25C6 {cost}g";
        tm.fontSize = 28;
        tm.alignment = TextAlignment.Center;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.color = new Color(1f, 0.85f, 0.2f);
        label.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        label.AddComponent<Billboard>();

        var arrow = new GameObject("Arrow");
        arrow.transform.SetParent(zone.transform);
        arrow.transform.localPosition = new Vector3(0, 1.5f, 0);
        var aTm = arrow.AddComponent<TextMesh>();
        aTm.text = "\u25BC";
        aTm.fontSize = 24;
        aTm.alignment = TextAlignment.Center;
        aTm.anchor = TextAnchor.MiddleCenter;
        aTm.color = new Color(1f, 0.55f, 0f);
        arrow.transform.localScale = new Vector3(0.06f, 0.06f, 0.06f);
        arrow.AddComponent<Billboard>();
    }

    static void PlaceNatureFeatures()
    {
        var flagPole = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        flagPole.name = "FlagPole";
        flagPole.transform.position = new Vector3(-2f, 3f, -48f);
        flagPole.transform.localScale = new Vector3(0.12f, 5f, 0.12f);
        flagPole.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f);

        string[] stripeColors = { "FF0000", "0033CC", "FFFFFF" };
        for (int i = 0; i < 3; i++)
        {
            var stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "FlagStripe_" + stripeColors[i];
            stripe.transform.position = new Vector3(-1f, 5.2f + i * 0.5f, -48f);
            stripe.transform.localScale = new Vector3(2.5f, 0.5f, 0.05f);
            Color c;
            ColorUtility.TryParseHtmlString("#" + stripeColors[i], out c);
            stripe.GetComponent<Renderer>().material.color = c;
        }

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
            stalk.GetComponent<Renderer>().material = wheatMat;
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
                tile.GetComponent<Renderer>().material = pathMat;
            }
        }

        BuildProceduralRiver();

        var pond = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pond.name = "Pond";
        pond.transform.position = new Vector3(-90f, 0.02f, -60f);
        pond.transform.localScale = new Vector3(8f, 0.05f, 8f);
        var pondMat = MakeWaterMaterial(0.15f, 0.45f, 0.7f, 0.85f);
        pond.GetComponent<Renderer>().material = pondMat;
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
        m.renderQueue = 3000;
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.85f);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", 0.0f);
        if (m.HasProperty("_SpecColor")) m.SetColor("_SpecColor", new Color(0.6f, 0.8f, 1f, 1f));
        return m;
    }

    static void BuildProceduralRiver()
    {
        var river = new GameObject("River");
        var matShallow = MakeWaterMaterial(0.28f, 0.62f, 0.78f, 0.78f);
        var matDeep = MakeWaterMaterial(0.10f, 0.32f, 0.52f, 0.92f);

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

            DestroyImmediate(seg.GetComponent<MeshCollider>());

            var rend = seg.GetComponent<Renderer>();
            float t = (float)i / (path.Length - 2);
            float depthT = 1f - Mathf.Abs(t * 2f - 1f);
            rend.material = depthT > 0.4f ? matDeep : matShallow;

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
                bank.GetComponent<Renderer>().material = bankMat;
                bank.GetComponent<MeshCollider>().enabled = true;
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
            stone.GetComponent<Renderer>().material = stoneMat;
            DestroyImmediate(stone.GetComponent<SphereCollider>());
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
            reed.GetComponent<Renderer>().material = reedMat;
            DestroyImmediate(reed.GetComponent<BoxCollider>());
        }

        Debug.Log("River: procedural, 6 segments, banks, stones, reeds (RiverFlow animates UV).");
    }

    static void PlaceForest()
    {
        var pine = AssetDatabase.LoadAssetAtPath<GameObject>(NaturePrefabs + "/Trees/PT_Pine_Tree_03_green.prefab");
        if (pine == null) return;

        float[][] treePositions = new float[][] {
            new float[] { -110f, -90f },
            new float[] { -125f, -80f },
            new float[] { -95f, -105f },
            new float[] { -130f, -105f },
            new float[] { -85f, -120f },
            new float[] { -115f, -115f },
            new float[] { -140f, -95f },
            new float[] { -100f, -130f },
            new float[] { -120f, -125f },
            new float[] { -135f, -115f },
        };

        foreach (var pos in treePositions)
        {
            var go = (GameObject)Object.Instantiate(pine);
            go.transform.position = new Vector3(pos[0], 0, pos[1]);
            go.transform.localScale = Vector3.one * Random.Range(3f, 5f);
            go.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        }
    }

    static void PlaceNPCs()
    {
        string basePath = Kayak + "/Characters/fbx";
        var npcDefs = new[] {
            ("Barbarian", new Vector3(-15f, 0, -10f)),
            ("Mage", new Vector3(15f, 0, -10f)),
            ("Ranger", new Vector3(10f, 0, 15f)),
            ("Rogue", new Vector3(-10f, 0, 15f))
        };

        foreach (var (name, pos) in npcDefs)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(basePath + "/" + name + ".fbx");
            if (prefab == null) continue;

            var go = (GameObject)Object.Instantiate(prefab);
            go.name = name;
            go.transform.position = pos;
            go.transform.localScale = Vector3.one * 1.8f;

            var patrol = go.AddComponent<NPCPatrol>();
            patrol.idleOnly = false;
            patrol.patrolRadius = 8f;
            patrol.moveSpeed = 1.5f;
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
    }

    static void CreateBuildZones()
    {
        var spots = new Vector3[] {
            new Vector3(12f, 0f, -35f),
            new Vector3(-75f, 0f, 0f),
            new Vector3(75f, 0f, 0f),
            new Vector3(0f, 0f, -55f)
        };

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(0.2f, 0.55f, 0.9f, 0.3f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);

        foreach (var pos in spots)
        {
            var zone = GameObject.CreatePrimitive(PrimitiveType.Cube);
            zone.name = "BuildZone";
            zone.transform.position = pos;
            zone.transform.localScale = new Vector3(6f, 2f, 6f);
            zone.GetComponent<BoxCollider>().isTrigger = true;
            zone.GetComponent<Renderer>().material = mat;

            var bz = zone.AddComponent<BuildZone>();
            bz.cost = 15;
            AddBuildZoneLabel(zone, bz.cost);
        }
    }

    static void SetupUI()
    {
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        canvasObj.AddComponent<HUDController>();
        canvasObj.AddComponent<HUDInfoPanel>();
        canvasObj.AddComponent<MinimapController>();
        CreateInfoPanel();
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
        cam.orthographicSize = 30;
        cam.transform.position = new Vector3(0, 15, -30);
        cam.transform.rotation = Quaternion.Euler(30, 0, 0);
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.45f, 0.7f, 0.88f);

        var follow = cam.gameObject.AddComponent<CameraFollow3D>();
        follow.baseDist = 25f;
        follow.currentDist = 25f;
        follow.pitch = 30f;
    }

    static void SetupLights()
    {
        if (Object.FindAnyObjectByType<Light>() != null) return;

        var lightObj = new GameObject("Directional Light");
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.transform.rotation = Quaternion.Euler(50, -30, 0);
        light.intensity = 1.3f;
        light.shadows = LightShadows.Soft;
    }

    static void SetupPlayerSettings()
    {
        PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
        PlayerSettings.defaultScreenWidth = 1920;
        PlayerSettings.defaultScreenHeight = 1080;
        PlayerSettings.runInBackground = true;
        PlayerSettings.resizableWindow = true;
    }
}
