using UnityEngine;

public class BuildPrototype : MonoBehaviour
{
    void Start()
    {
        BuildScene();
    }

    [ContextMenu("Build / Rebuild Scene")]
    public void BuildScene()
    {
        var root = GameObject.Find("_Prototype");
        if (root != null)
        {
            if (Application.isPlaying) Destroy(root);
            else DestroyImmediate(root);
        }

        var parent = new GameObject("_Prototype").transform;

        SetupCamera();
        SetupLighting(parent);
        CreateTiles(parent);
        CreateBuilding(parent);
        CreateHero(parent);
    }

    #region Setup

    void SetupCamera()
    {
        Camera cam;
        if (Camera.main != null) cam = Camera.main;
        else
        {
            var go = new GameObject("Main Camera");
            cam = go.AddComponent<Camera>();
            go.tag = "MainCamera";
        }
        cam.transform.SetPositionAndRotation(new Vector3(14, 11, 14), Quaternion.identity);
        cam.transform.LookAt(new Vector3(7.7f, 0.5f, 7.7f));
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.55f, 0.65f, 0.90f);
    }

    void SetupLighting(Transform parent)
    {
        var lightObj = GameObject.Find("Directional Light");
        if (lightObj == null)
        {
            lightObj = new GameObject("Directional Light");
            lightObj.transform.parent = parent;
            lightObj.AddComponent<Light>();
        }
        lightObj.transform.rotation = Quaternion.Euler(35, 50, 0);
        var l = lightObj.GetComponent<Light>();
        l.type = LightType.Directional;
        l.intensity = 2.2f;
        l.shadows = LightShadows.Soft;
    }

    #endregion

    #region Helpers

    Material MakeMat(Color c)
    {
        var m = new Material(Shader.Find("Standard"));
        m.color = c;
        m.SetFloat("_Metallic", 0f);
        m.SetFloat("_Glossiness", 0.3f);
        return m;
    }

    GameObject MakePrim(PrimitiveType type, Vector3 pos, Vector3 scale, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(type);
        go.transform.SetParent(parent);
        go.transform.position = pos;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().material = mat;
        if (!Application.isPlaying) DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }

    GameObject MakeMesh(Mesh mesh, Material mat, Transform parent)
    {
        var go = new GameObject("CustomMesh");
        go.transform.parent = parent;
        go.AddComponent<MeshFilter>().mesh = mesh;
        go.AddComponent<MeshRenderer>().material = mat;
        return go;
    }

    #endregion

    #region Tiles

    void CreateTiles(Transform parent)
    {
        var pal = new System.Collections.Generic.Dictionary<string, Color>
        {
            {"grass", new Color(0.20f, 0.55f, 0.10f)},
            {"dirt",  new Color(0.60f, 0.35f, 0.12f)},
            {"stone", new Color(0.45f, 0.45f, 0.45f)},
            {"water", new Color(0.10f, 0.30f, 0.60f)},
            {"forest",new Color(0.08f, 0.35f, 0.05f)},
        };
        var kinds = new[] {"grass","grass","dirt","grass","water","stone","forest","grass","dirt"};

        for (int x = 0; x < 7; x++)
        {
            for (int z = 0; z < 7; z++)
            {
                var k = kinds[(x * 3 + z * 7) % kinds.Length];
                if (x == 3 && z == 3) k = "dirt";
                MakePrim(PrimitiveType.Cube,
                    new Vector3(x * 2.2f + 1.1f, 0.15f, z * 2.2f + 1.1f),
                    new Vector3(2.0f, 0.3f, 2.0f),
                    MakeMat(pal[k]), parent);
            }
        }
    }

    #endregion

    #region Building

    void CreateBuilding(Transform parent)
    {
        var building = new GameObject("Building").transform;
        building.parent = parent;
        building.position = new Vector3(1.1f, 0, 11.0f);

        var wood = MakeMat(new Color(0.48f, 0.28f, 0.10f));
        var roofC = MakeMat(new Color(0.60f, 0.18f, 0.12f));
        var doorC = MakeMat(new Color(0.30f, 0.18f, 0.06f));
        var winC  = MakeMat(new Color(0.55f, 0.70f, 0.85f));

        // Walls
        MakePrim(PrimitiveType.Cube, Vector3.up * 0.8f, new Vector3(2.0f, 1.6f, 2.0f), wood, building);

        // Gable roof
        MakeMesh(BuildRoofMesh(), roofC, building);

        // Door
        MakePrim(PrimitiveType.Cube, new Vector3(0, 0.35f, -1.01f),
            new Vector3(0.4f, 0.7f, 0.02f), doorC, building);

        // Windows
        foreach (var wx in new[] {-0.6f, 0.6f})
            MakePrim(PrimitiveType.Cube, new Vector3(wx, 0.8f, -1.01f),
                new Vector3(0.3f, 0.3f, 0.02f), winC, building);
    }

    Mesh BuildRoofMesh()
    {
        var verts = new Vector3[]
        {
            new(-1.3f, 2.8f, 0),    // 0
            new( 1.3f, 2.8f, 0),    // 1
            new(-1.3f, 1.6f, -1.3f),// 2
            new( 1.3f, 1.6f, -1.3f),// 3
            new(-1.3f, 1.6f,  1.3f),// 4
            new( 1.3f, 1.6f,  1.3f),// 5
        };
        var tris = new int[]
        {
            0,1,2, 1,3,2,   // front slope
            0,1,4, 1,5,4,   // back slope
            0,2,4,          // left gable
            1,5,3,          // right gable
        };
        var mesh = new Mesh { vertices = verts, triangles = tris };
        mesh.RecalculateNormals();
        return mesh;
    }

    #endregion

    #region Hero

    void CreateHero(Transform parent)
    {
        var hero = new GameObject("Hero").transform;
        hero.parent = parent;
        hero.position = new Vector3(7.7f, 0, 7.7f);

        var skin    = MakeMat(new Color(0.95f, 0.78f, 0.60f));
        var tunic   = MakeMat(new Color(0.12f, 0.28f, 0.58f));
        var boot    = MakeMat(new Color(0.28f, 0.16f, 0.06f));
        var metal   = MakeMat(new Color(0.72f, 0.72f, 0.80f));
        var gold    = MakeMat(new Color(0.85f, 0.62f, 0.10f));
        var shieldC = MakeMat(new Color(0.65f, 0.12f, 0.12f));
        var beltC   = MakeMat(new Color(0.32f, 0.20f, 0.06f));
        var capeC   = MakeMat(new Color(0.70f, 0.18f, 0.12f));
        var hatC    = MakeMat(new Color(0.75f, 0.12f, 0.08f));

        // Body (capsule: default h=2 r=0.5 → scale to h=1.0 r=0.3)
        var body = MakePrim(PrimitiveType.Capsule, new Vector3(0, 0.65f, 0),
            new Vector3(0.6f, 0.5f, 0.6f), tunic, hero);
        body.transform.localScale = new Vector3(0.6f, 0.5f, 0.6f);

        // Head
        MakePrim(PrimitiveType.Sphere, new Vector3(0, 1.2f, 0),
            Vector3.one * 0.44f, skin, hero);

        // Cone hat
        MakeMesh(BuildConeMesh(0.26f, 0.38f, 10), hatC, hero).transform.localPosition = new Vector3(0, 1.35f, 0);

        // Arms
        MakePrim(PrimitiveType.Cylinder, new Vector3(-0.35f, 0.90f, 0),
            new Vector3(0.12f, 0.45f, 0.12f), skin, hero);
        MakePrim(PrimitiveType.Cylinder, new Vector3( 0.35f, 0.90f, 0),
            new Vector3(0.12f, 0.45f, 0.12f), skin, hero);

        // Legs (boots)
        MakePrim(PrimitiveType.Cylinder, new Vector3(-0.15f, 0.14f, 0),
            new Vector3(0.18f, 0.28f, 0.18f), boot, hero);
        MakePrim(PrimitiveType.Cylinder, new Vector3( 0.15f, 0.14f, 0),
            new Vector3(0.18f, 0.28f, 0.18f), boot, hero);

        // Belt
        MakePrim(PrimitiveType.Cylinder, new Vector3(0, 0.45f, 0),
            new Vector3(0.62f, 0.02f, 0.62f), beltC, hero);

        // Cape
        MakePrim(PrimitiveType.Cube, new Vector3(0, 0.50f, -0.28f),
            new Vector3(0.50f, 0.50f, 0.02f), capeC, hero);

        // Sword
        var sword = new GameObject("Sword").transform;
        sword.parent = hero;
        sword.localPosition = new Vector3(0.50f, 0.45f, 0);
        MakePrim(PrimitiveType.Cube, new Vector3(0, 0.45f, 0),
            new Vector3(0.04f, 0.55f, 0.012f), metal, sword);
        // Sword tip
        MakeMesh(BuildSwordTipMesh(0.10f), metal, sword).transform.localPosition = Vector3.zero;
        // Guard
        MakePrim(PrimitiveType.Cube, new Vector3(0, 0.18f, 0),
            new Vector3(0.10f, 0.03f, 0.02f), gold, sword);
        // Handle
        MakePrim(PrimitiveType.Cylinder, new Vector3(0, 0.07f, 0),
            new Vector3(0.04f, 0.12f, 0.04f), MakeMat(new Color(0.22f, 0.10f, 0.04f)), sword);

        // Shield
        var shield = MakePrim(PrimitiveType.Cube, new Vector3(-0.42f, 0.60f, 0),
            new Vector3(0.04f, 0.40f, 0.28f), shieldC, hero);
        // Gold cross emblem
        MakePrim(PrimitiveType.Cube, new Vector3(-0.42f, 0.60f, 0.142f),
            new Vector3(0.04f, 0.22f, 0.003f), gold, hero);
        MakePrim(PrimitiveType.Cube, new Vector3(-0.42f, 0.60f, 0.142f),
            new Vector3(0.003f, 0.08f, 0.18f), gold, hero);
    }

    Mesh BuildConeMesh(float radius, float height, int segs)
    {
        var verts = new Vector3[segs + 1];
        var tris = new int[segs * 3];
        verts[0] = Vector3.up * height;
        for (int i = 0; i < segs; i++)
        {
            float a = (float)i / segs * Mathf.PI * 2;
            verts[i + 1] = new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius);
        }
        for (int i = 0; i < segs; i++)
        {
            tris[i * 3] = 0;
            tris[i * 3 + 1] = i + 1;
            tris[i * 3 + 2] = (i + 1) % segs + 1;
        }
        var mesh = new Mesh { vertices = verts, triangles = tris };
        mesh.RecalculateNormals();
        return mesh;
    }

    Mesh BuildSwordTipMesh(float height)
    {
        float bladeTop = 0.45f + 0.275f;
        var verts = new Vector3[]
        {
            new(0, bladeTop + height, 0),
            new(-0.02f, bladeTop, 0),
            new( 0.02f, bladeTop, 0),
        };
        var tris = new int[] { 0, 1, 2 };
        var mesh = new Mesh { vertices = verts, triangles = tris };
        mesh.RecalculateNormals();
        return mesh;
    }

    #endregion
}
