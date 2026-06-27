using UnityEngine;
using System.Collections.Generic;

public class VillageWalls : MonoBehaviour
{
    public GameObject fencePrefab;
    public float fenceHeight = 2.5f;

    static readonly Vector3[] corners = new[] {
        new Vector3(-70f, 0f, -50f),
        new Vector3( 70f, 0f, -50f),
        new Vector3( 70f, 0f,  50f),
        new Vector3(-70f, 0f,  50f),
    };

    bool[] towersBuilt = new bool[4];
    bool[] fencesBuilt = new bool[4];
    List<GameObject> fenceSegments = new List<GameObject>();
    GameObject gate;
    GameObject gateBlock;

    void Start()
    {
        SlotManager.SlotBuilt += OnSlotBuilt;
    }

    void OnDestroy()
    {
        SlotManager.SlotBuilt -= OnSlotBuilt;
    }

    void OnSlotBuilt(int index)
    {
        int towerIdx = index - 2;
        if (towerIdx < 0 || towerIdx >= 4) return;
        towersBuilt[towerIdx] = true;

        if (towerIdx == 0 || towerIdx == 1)
        {
            if (towersBuilt[0] && towersBuilt[1])
                BuildSouthFence();
        }
        CheckBuildSide(1, 2, 1);
        CheckBuildSide(2, 3, 2);
        CheckBuildSide(3, 0, 3);
    }

    void CheckBuildSide(int a, int b, int sideIdx)
    {
        if (!towersBuilt[a] || !towersBuilt[b]) return;
        if (fencesBuilt[sideIdx]) return;
        fencesBuilt[sideIdx] = true;
        BuildFence(corners[a], corners[b]);
    }

    void BuildSouthFence()
    {
        float leftX = corners[0].x;
        float rightX = corners[1].x;
        float z = corners[0].z;
        float gateHalf = 4f;

        BuildFence(new Vector3(leftX, 0, z), new Vector3(-gateHalf, 0, z));
        BuildFence(new Vector3(gateHalf, 0, z), new Vector3(rightX, 0, z));

        if (gate == null) SetupGate();
    }

    void BuildFence(Vector3 from, Vector3 to)
    {
        if (fencePrefab == null) return;

        Vector3 dir = to - from;
        float length = dir.magnitude;
        if (length < 0.5f) return;
        float yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg - 90f;

        // Place fence segments with overlap to prevent enemies from passing through
        int count = Mathf.Max(1, Mathf.CeilToInt(length / 1.0f));
        for (int i = 0; i < count; i++)
        {
            float t = (i + 0.5f) / count;
            Vector3 pos = Vector3.Lerp(from, to, t);
            pos.y = 0;

            var seg = Instantiate(fencePrefab, pos, Quaternion.Euler(0, yaw, 0));
            seg.transform.localScale = new Vector3(1f, fenceHeight, 1f);
            seg.name = "FenceSeg";

            // Solid box collider so nothing passes through fences
            var col = seg.AddComponent<BoxCollider>();
            col.isTrigger = false;
            col.size = new Vector3(1.4f, fenceHeight, 1.0f);
            col.center = new Vector3(0, fenceHeight * 0.5f, 0);

            fenceSegments.Add(seg);
        }
    }

    void SetupGate()
    {
        if (fencePrefab == null) return;
        float z = corners[0].z;

        var postMat = new Material(Shader.Find("Standard"));
        postMat.color = new Color(0.4f, 0.25f, 0.12f);
        foreach (float offset in new[] { -4f, 4f })
        {
            var post = GameObject.CreatePrimitive(PrimitiveType.Cube);
            post.name = "GatePost";
            post.transform.position = new Vector3(offset, 1.5f * fenceHeight, z);
            post.transform.localScale = new Vector3(0.6f, 3f * fenceHeight, 0.6f);
            post.GetComponent<Renderer>().sharedMaterial = postMat;
            Object.DestroyImmediate(post.GetComponent<BoxCollider>());
            fenceSegments.Add(post);
        }

        // Gate bar — starts OPEN (high), closes when enemies are near
        gate = GameObject.CreatePrimitive(PrimitiveType.Cube);
        gate.name = "GateBar";
        gate.transform.position = new Vector3(0f, 5f, z);
        gate.transform.localScale = new Vector3(8f, 0.3f, 0.3f);
        gate.GetComponent<Renderer>().sharedMaterial = postMat;
        Object.DestroyImmediate(gate.GetComponent<BoxCollider>());

        // Gate block — non-trigger physics collider that blocks ALL entities
        gateBlock = new GameObject("GateBlock");
        gateBlock.transform.position = new Vector3(0f, 1f, z);
        gateBlock.transform.localScale = new Vector3(8f, 2f, 0.5f);
        var gateBlockCol = gateBlock.AddComponent<BoxCollider>();
        gateBlockCol.isTrigger = false;
        // Try to tag, but don't fail if tag doesn't exist
        try { gateBlock.tag = "Gate"; } catch { }
    }

    void Update()
    {
        if (gate == null || gateBlock == null) return;

        var player = FindAnyObjectByType<PlayerController3D>();
        if (player == null) return;

        float dist = Vector3.Distance(
            new Vector3(player.transform.position.x, 0, player.transform.position.z),
            new Vector3(gate.transform.position.x, 0, gate.transform.position.z));

        bool heroNear = dist < 6f;

        // Hero near → gate opens (bar raises, collider deactivated)
        // Hero far → gate closes (bar lowers, collider activated)
        float targetY = heroNear ? 5f : 0.5f;
        Vector3 pos = gate.transform.position;
        pos.y = Mathf.Lerp(pos.y, targetY, Time.deltaTime * 5f);
        gate.transform.position = pos;

        gateBlock.SetActive(!heroNear);
    }
}
