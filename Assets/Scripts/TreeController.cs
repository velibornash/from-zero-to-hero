using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeController : MonoBehaviour
{
    public static List<TreeController> AllTrees = new List<TreeController>();

    public int woodAmount = 5;
    public bool chopped { get; private set; }

    static GameObject _stumpPrefab;
    static GameObject _logsPrefab;

    void Awake()
    {
        AllTrees.Add(this);
        if (_stumpPrefab == null)
            _stumpPrefab = Resources.Load<GameObject>("PT_Pine_Tree_03_green_cut");
        if (_logsPrefab == null)
            _logsPrefab = Resources.Load<GameObject>("PT_Pine_Tree_03_logs");
        if (_stumpPrefab == null)
            Debug.LogWarning("TreeController: stump prefab not found in Resources");
        if (_logsPrefab == null)
            Debug.LogWarning("TreeController: logs prefab not found in Resources");
    }

    void OnDestroy()
    {
        AllTrees.Remove(this);
    }

    public void Chop(PlayerController3D hero)
    {
        if (chopped) return;
        chopped = true;
        StartCoroutine(ChopSequence(hero));
    }

    IEnumerator ChopSequence(PlayerController3D hero)
    {
        // 3 swings with delay
        for (int i = 0; i < 3; i++)
        {
            hero.ForceAttack();
            yield return new WaitForSeconds(0.3f);
        }

        Vector3 pos = transform.position;
        float scale = transform.localScale.x;

        SmokePuff(pos);

        var mr = GetComponent<MeshRenderer>();
        if (mr != null) mr.enabled = false;

        if (_stumpPrefab != null)
        {
            var stump = Instantiate(_stumpPrefab, pos, transform.rotation);
            stump.transform.localScale = Vector3.one * scale;
        }

        if (_logsPrefab != null)
        {
            StartCoroutine(SpawnPickup(pos, scale));
        }
    }

    IEnumerator SpawnPickup(Vector3 pos, float scale)
    {
        yield return new WaitForSeconds(0.6f);
        Vector3 offset = new Vector3(Random.Range(-2f, 2f), 0.3f, Random.Range(-2f, 2f));
        var logs = Instantiate(_logsPrefab, pos + offset, Quaternion.identity);
        logs.transform.localScale = Vector3.one * scale * 0.8f;
        logs.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        var pickup = logs.AddComponent<ResourcePickup>();
        pickup.resourceType = "wood";
        pickup.amount = woodAmount;
        var col = logs.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;
    }

    void SmokePuff(Vector3 pos)
    {
        var smokeMat = new Material(Shader.Find("Standard"));
        smokeMat.color = new Color(0.95f, 0.95f, 0.9f, 0.8f);
        smokeMat.SetFloat("_Mode", 3);
        smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        smokeMat.SetInt("_ZWrite", 0);
        smokeMat.DisableKeyword("_ALPHABLEND_ON");
        smokeMat.DisableKeyword("_ALPHATEST_ON");
        smokeMat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        smokeMat.EnableKeyword("_ALPHABLEND_ON");
        smokeMat.renderQueue = 3000;

        for (int i = 0; i < 20; i++)
        {
            var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            puff.name = "TreeSmoke";
            Destroy(puff.GetComponent<SphereCollider>());
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float rad = Random.Range(0.3f, 2.5f);
            puff.transform.position = pos + new Vector3(
                Mathf.Cos(angle) * rad,
                Random.Range(0.2f, 1.5f),
                Mathf.Sin(angle) * rad);
            float s = Random.Range(0.6f, 2f);
            puff.transform.localScale = Vector3.one * s;
            puff.GetComponent<Renderer>().sharedMaterial = smokeMat;
            StartCoroutine(AnimateSmoke(puff.transform));
        }
    }

    IEnumerator AnimateSmoke(Transform t)
    {
        Vector3 startScale = t.localScale;
        Vector3 drift = new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(1f, 2.5f), Random.Range(-0.8f, 0.8f));
        float duration = 2.5f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float p = elapsed / duration;
            t.localScale = Vector3.Lerp(startScale, startScale * 3f, p);
            t.position += drift * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        Destroy(t.gameObject);
    }

    public static void InitializeAllTrees()
    {
        int count = 0;
        // First pass: find trees already with TreeController (from Setup3DScene)
        var existing = Object.FindObjectsByType<TreeController>(FindObjectsInactive.Include);
        count = existing.Length;

        // Second pass: find scene objects by name that don't have it yet
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include);
        foreach (var t in all)
        {
            if (t == null || t.gameObject == null) continue;
            if (t.name == "ForestTree" || t.name == "NorthTree")
            {
                if (t.GetComponent<TreeController>() == null)
                {
                    t.gameObject.AddComponent<TreeController>();
                    count++;
                }
            }
        }
        Debug.Log($"TreeController: initialized {count} trees");
    }
}
