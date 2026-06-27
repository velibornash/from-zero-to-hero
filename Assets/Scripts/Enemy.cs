using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 3;
    public int goldReward = 5;
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 8f;
    public float knockbackForce = 3f;
    public int heroDamage = 12;
    public float attackInterval = 0.7f;
    public string enemyName = "Enemy";

    int currentHealth;
    Transform target;
    Animator anim;
    Collider col;
    Rigidbody rb;
    Vector3 knockbackVelocity;
    float stunTimer;
    float baseY;
    float lastAttackTime;
    bool hasAttackParam, hasDeathParam;
    Transform modelRoot;

    public float walkBobAmp = 0.25f;
    public float walkBobSpeed = 12f;

    // Stuck detection for going around obstacles (fences, etc)
    Vector3 lastStuckPos;
    float stuckTimer;

    void Start()
    {
        currentHealth = maxHealth;

        var player = FindAnyObjectByType<PlayerController3D>();
        if (player != null) target = player.transform;

        anim = GetComponentInChildren<Animator>();
        col = GetComponentInChildren<Collider>();

        foreach (Transform child in transform)
            if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                { modelRoot = child; break; }
        if (modelRoot != null) baseY = modelRoot.localPosition.y;
        else baseY = 0f;

        // Add Rigidbody so fences and other physics colliders block the enemy
        rb = GetComponent<Rigidbody>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Cache animator parameter existence to avoid spammy warnings
        if (anim != null)
        {
            foreach (var p in anim.parameters)
            {
                if (p.name == "Attack") hasAttackParam = true;
                if (p.name == "Death") hasDeathParam = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

        if (stunTimer > 0f)
        {
            stunTimer -= Time.fixedDeltaTime;
            knockbackVelocity *= 0.85f;
            rb.linearVelocity = knockbackVelocity;
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (dist > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotateSpeed * Time.fixedDeltaTime);
        }

        if (dist > 2.5f)
        {
            Vector3 moveDir = dir.normalized;
            rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, 0, moveDir.z * moveSpeed);
            if (anim != null) anim.SetFloat("Speed", 1f);
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
            if (anim != null)
            {
                anim.SetFloat("Speed", 0f);
                if (hasAttackParam) anim.SetTrigger("Attack");
            }
            // Damage the hero when in attack range (with cooldown)
            TryDamageHero();
        }
    }

    void Update()
    {
        float moveSpeedNow = rb != null ? new Vector2(rb.linearVelocity.x, rb.linearVelocity.z).magnitude : 0f;
        bool moving = moveSpeedNow > 0.1f;
        if (modelRoot != null && moving)
        {
            float t = Time.time * walkBobSpeed;
            float bob = Mathf.Sin(t) * walkBobAmp;
            float sway = Mathf.Sin(t * 0.5f) * 4f;
            var lp = modelRoot.localPosition;
            lp.y = baseY + bob;
            modelRoot.localPosition = lp;
            modelRoot.localRotation = Quaternion.Euler(0f, 0f, sway);
        }
        else if (modelRoot != null)
        {
            var lp = modelRoot.localPosition;
            lp.y = baseY;
            modelRoot.localPosition = lp;
            modelRoot.localRotation = Quaternion.identity;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        StartCoroutine(HitFlash());

        if (target != null)
        {
            Vector3 knockDir = (transform.position - target.position).normalized;
            knockbackVelocity = new Vector3(knockDir.x * knockbackForce, 0, knockDir.z * knockbackForce);
            stunTimer = 0.25f;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    System.Collections.IEnumerator HitFlash()
    {
        Vector3 baseScale = transform.localScale;
        transform.localScale = baseScale * 1.2f;
        yield return new WaitForSeconds(0.08f);
        transform.localScale = baseScale;
    }

    void TryDamageHero()
    {
        if (target == null) return;
        if (Time.time - lastAttackTime < attackInterval) return;

        // Find the hero and damage them
        var hero = target.GetComponent<PlayerController3D>();
        if (hero != null)
        {
            hero.TakeDamage(heroDamage);
            lastAttackTime = Time.time;
        }
    }

    void Die()
    {
        HUDController.Gold += goldReward;
        HUDController.PushEvent($"Defeated {enemyName}! +{goldReward} gold");
        SpawnCoinPuff();
        SpawnDeathBurst();

        if (anim != null)
        {
            if (hasDeathParam) anim.SetTrigger("Death");
            enabled = false;
            if (col != null) col.enabled = false;
            if (rb != null) rb.linearVelocity = Vector3.zero;
            Destroy(gameObject, 1.5f);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void SpawnDeathBurst()
    {
        Color burstColor = enemyName == "Wolf" ? new Color(0.45f, 0.42f, 0.38f) : new Color(0.55f, 0.32f, 0.22f);
        for (int i = 0; i < 20; i++)
        {
            // Use a custom GameObject without a collider to avoid DestroyImmediate during physics
            var chunk = new GameObject("EnemyChunk");
            chunk.transform.position = transform.position + Vector3.up * 0.8f + Random.insideUnitSphere * 1f;
            float size = Random.Range(0.12f, 0.5f);
            chunk.transform.localScale = Vector3.one * size;

            var meshFilter = chunk.AddComponent<MeshFilter>();
            meshFilter.sharedMesh = MakeCubeMesh();
            var rend = chunk.AddComponent<MeshRenderer>();
            rend.sharedMaterial = MakeBurstMat(i < 8 ? Color.Lerp(burstColor, Color.white, 0.3f) : burstColor);

            var rb = chunk.AddComponent<Rigidbody>();
            rb.linearVelocity = new Vector3(Random.Range(-6f, 6f), Random.Range(3f, 8f), Random.Range(-6f, 6f));
            rb.angularVelocity = Random.insideUnitSphere * 15f;

            Destroy(chunk, 1.5f);
        }
    }

    static Mesh s_cubeMesh;
    static Mesh MakeCubeMesh()
    {
        if (s_cubeMesh != null) return s_cubeMesh;
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        var mesh = go.GetComponent<MeshFilter>().sharedMesh;
        DestroyImmediate(go); // OK here — not during physics callback
        s_cubeMesh = mesh;
        return mesh;
    }

    Material MakeBurstMat(Color color)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        return mat;
    }

    void SpawnCoinPuff()
    {
        var puff = new GameObject("GoldPuff");
        puff.transform.position = transform.position + Vector3.up * 2f;

        var txt = puff.AddComponent<TextMesh>();
        txt.text = $"+{goldReward}";
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 64;
        txt.color = new Color(1f, 0.95f, 0.3f);
        txt.alignment = TextAlignment.Center;
        txt.anchor = TextAnchor.MiddleCenter;

        puff.AddComponent<Billboard>();
        puff.AddComponent<FloatAndFade>();
    }
}
