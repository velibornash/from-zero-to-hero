using UnityEngine;

public class TowerShooter : MonoBehaviour
{
    public float range = 18f;
    public float fireRate = 0.8f;
    public int damage = 1;
    public float projectileSpeed = 22f;
    public int upgradeCost = 10;
    public float projectileSpawnHeight = 12f;
    public Transform firePoint;

    public Color projectileColor = Color.white;

    public bool isUpgraded;
    float fireTimer;

    // Static template shared across all TowerShooters
    static GameObject s_projectileTemplate;

    void Start()
    {
        if (s_projectileTemplate == null)
            s_projectileTemplate = CreateProjectileTemplate();
        Debug.Log($"TowerShooter: started at {transform.position}, range={range}, fireRate={fireRate}");
    }

    void Update()
    {
        fireTimer -= Time.deltaTime;

        // Always face the closest enemy for visual feedback
        var closest = FindClosestEnemy();
        if (closest != null)
        {
            Vector3 dir = closest.transform.position - transform.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRot = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 6f * Time.deltaTime);
            }
        }

        if (fireTimer > 0f) return;
        if (closest == null) return;

        Fire(closest);
        fireTimer = fireRate;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, range);
    }

    Enemy FindClosestEnemy()
    {
        var enemies = FindObjectsByType<Enemy>();
        Enemy closest = null;
        float closestDist = range;
        foreach (var e in enemies)
        {
            float d = Vector3.Distance(transform.position, e.transform.position);
            if (d < closestDist) { closestDist = d; closest = e; }
        }
        return closest;
    }

    void Fire(Enemy target)
    {
        if (s_projectileTemplate == null)
            s_projectileTemplate = CreateProjectileTemplate();

        var spawnPos = firePoint != null ? firePoint.position : transform.position + Vector3.up * projectileSpawnHeight;
        var proj = Instantiate(s_projectileTemplate, spawnPos, Quaternion.identity);
        proj.SetActive(true);
        proj.hideFlags = HideFlags.None;
        var rend = proj.GetComponent<Renderer>();
        if (rend != null) rend.sharedMaterial = MakeMat(projectileColor);
        var p = proj.GetComponent<Projectile>();
        if (p != null) p.Init(target.transform, damage, projectileSpeed);

        // Muzzle flash effect
        SpawnMuzzleFlash(spawnPos);

        Debug.Log($"TowerShooter: fired projectile at {target.name} from {spawnPos}");
    }

    void SpawnMuzzleFlash(Vector3 pos)
    {
        // Tiny subtle flash sphere that expands and fades quickly
        var flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        Object.DestroyImmediate(flash.GetComponent<SphereCollider>());
        flash.transform.position = pos;
        flash.transform.localScale = Vector3.one * 0.3f;

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(1f, 0.9f, 0.5f, 0.6f);
        mat.EnableKeyword("_EMISSION");
        mat.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.4f) * 1.5f);
        flash.GetComponent<Renderer>().sharedMaterial = mat;

        // Small smoke puffs
        for (int i = 0; i < 2; i++)
        {
            var smoke = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Object.DestroyImmediate(smoke.GetComponent<SphereCollider>());
            smoke.name = "MuzzleSmoke";
            float angle = i * Mathf.PI;
            smoke.transform.position = pos + new Vector3(
                Mathf.Cos(angle) * 0.3f, Random.Range(-0.1f, 0.1f), Mathf.Sin(angle) * 0.3f);
            smoke.transform.localScale = Vector3.one * 0.25f;
            var smokeMat = new Material(Shader.Find("Standard"));
            smokeMat.color = new Color(0.6f, 0.55f, 0.45f, 0.5f);
            smokeMat.SetFloat("_Mode", 3);
            smokeMat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            smokeMat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            smokeMat.SetInt("_ZWrite", 0);
            smokeMat.EnableKeyword("_ALPHABLEND_ON");
            smokeMat.renderQueue = 3000;
            smoke.GetComponent<Renderer>().sharedMaterial = smokeMat;
            StartCoroutine(AnimateSmokePuff(smoke.transform, smokeMat));
        }

        StartCoroutine(AnimateFlash(flash.transform, mat));
    }

    System.Collections.IEnumerator AnimateFlash(Transform t, Material mat)
    {
        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one * 2.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.localScale = Vector3.Lerp(startScale, endScale, p);
            var c = mat.color;
            c.a = 1f - p;
            mat.color = c;
            var em = mat.GetColor("_EmissionColor");
            em.a = 3f * (1f - p);
            mat.SetColor("_EmissionColor", em);
            yield return null;
        }
        Destroy(t.gameObject);
        Destroy(mat);
    }

    System.Collections.IEnumerator AnimateSmokePuff(Transform t, Material mat)
    {
        float duration = 0.6f;
        float elapsed = 0f;
        Vector3 startScale = t.localScale;
        Vector3 endScale = startScale * 2.5f;
        Vector3 startPos = t.position;
        Vector3 drift = new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, Random.Range(-0.3f, 0.3f));
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / duration;
            t.localScale = Vector3.Lerp(startScale, endScale, p);
            t.position = startPos + drift * p;
            var c = mat.color;
            c.a = 0.7f * (1f - p);
            mat.color = c;
            yield return null;
        }
        Destroy(t.gameObject);
        Destroy(mat);
    }

    void OnMouseDown()
    {
        // Mage spawning is now handled via dedicated Mage tile slots
    }

    public void ForceUpgrade()
    {
        // No-op: Mage spawning is now handled via dedicated Mage tile slots
    }

    void SpawnMage()
    {
        // No-op: Mage spawning is now handled via dedicated Mage tile slots
        // See BuildSlot.SpawnMageOnTile
    }

    GameObject CreateProjectileTemplate()
    {
        var proj = new GameObject("ProjectileTemplate");
        proj.transform.localScale = Vector3.one * 0.5f;
        proj.SetActive(false);
        proj.hideFlags = HideFlags.HideAndDontSave;

        var col = proj.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 0.5f;

        var rb = proj.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // Bright glowing visual
        var visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Visual";
        visual.transform.SetParent(proj.transform, false);
        visual.transform.localScale = Vector3.one;
        Object.DestroyImmediate(visual.GetComponent<SphereCollider>());

        // Set the visual to be emissive and bright
        var visualMat = new Material(Shader.Find("Standard"));
        visualMat.color = Color.white;
        visualMat.EnableKeyword("_EMISSION");
        visualMat.SetColor("_EmissionColor", Color.white * 2f);
        visual.GetComponent<Renderer>().sharedMaterial = visualMat;

        proj.AddComponent<Projectile>();
        proj.AddComponent<Light>();  // Add a point light to make the projectile glow

        DontDestroyOnLoad(proj);
        return proj;
    }

    Material MakeMat(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }
}
