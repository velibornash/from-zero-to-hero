using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceNode : MonoBehaviour
{
    public static List<ResourceNode> AllNodes = new List<ResourceNode>();

    public string resourceType = "stone";
    public int amount = 5;
    public GameObject altPrefab;

    bool harvested;

    void Awake()
    {
        AllNodes.Add(this);
    }

    void OnDestroy()
    {
        AllNodes.Remove(this);
    }

    public bool CanHarvest(float dist)
    {
        return !harvested && dist < 4f;
    }

    public void Harvest(PlayerController3D hero)
    {
        if (harvested) return;
        harvested = true;
        StartCoroutine(HarvestSequence(hero));
    }

    IEnumerator HarvestSequence(PlayerController3D hero)
    {
        for (int i = 0; i < 3; i++)
        {
            hero.ForceAttack();
            yield return new WaitForSeconds(0.3f);
        }

        Vector3 pos = transform.position;
        float scale = transform.localScale.x;

        SmokePuff(pos);

        var mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (var mr in mrs) mr.enabled = false;
        var cols = GetComponentsInChildren<Collider>();
        foreach (var col in cols) col.enabled = false;

        SpawnPickup(pos, scale);
    }

    void SpawnPickup(Vector3 pos, float scale)
    {
        Vector3 offset = new Vector3(Random.Range(-1.5f, 1.5f), 0.3f, Random.Range(-1.5f, 1.5f));

        var go = GameObject.CreatePrimitive(PrimitiveType.Quad);
        go.name = resourceType + "_pickup";
        Destroy(go.GetComponent<MeshCollider>());
        go.transform.position = pos + offset;
        go.transform.localScale = Vector3.one * scale * 3f;

        string texName = resourceType == "stone" ? "stone_pile_collectable"
                       : resourceType == "wood" ? "wood_pile_collectable"
                       : "wheat_pile_collectable";
        var tex = Resources.Load<Texture2D>("HUDIcons/" + texName);
        if (tex != null)
        {
            tex = TextureHelper.ChromaKey(tex);
            var mat = new Material(Shader.Find("Unlit/Transparent"));
            mat.mainTexture = tex;
            go.GetComponent<Renderer>().material = mat;
        }
        go.AddComponent<Billboard>();

        var colGo = new GameObject("PickupTrigger");
        colGo.transform.position = pos + offset;
        var col = colGo.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 2f;
        var pickup = colGo.AddComponent<ResourcePickup>();
        pickup.resourceType = resourceType;
        pickup.amount = amount;

        go.transform.SetParent(colGo.transform, true);
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
            puff.name = "NodeSmoke";
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
}
