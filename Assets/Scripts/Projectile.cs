using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 20f;
    public int damage = 1;
    public float lifetime = 5f;
    public Color trailColor = new Color(0.95f, 0.78f, 0.30f);

    Transform target;
    Vector3 lastTargetPos;
    float trailTimer;

    public void Init(Transform t, int dmg, float spd)
    {
        target = t;
        damage = dmg;
        speed = spd;
        if (target != null) lastTargetPos = target.position;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (target != null) lastTargetPos = target.position;

        Vector3 dir = lastTargetPos - transform.position;
        float dist = dir.magnitude;
        if (dist < 0.5f)
        {
            HitTarget();
            return;
        }

        transform.position += dir.normalized * speed * Time.deltaTime;
        transform.rotation = Quaternion.LookRotation(dir);

        // Spawn a small smoke trail every few frames
        trailTimer -= Time.deltaTime;
        if (trailTimer <= 0f)
        {
            trailTimer = 0.04f;
            SpawnTrailPuff();
        }
    }

    void SpawnTrailPuff()
    {
        var puff = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        puff.name = "TrailPuff";
        puff.transform.position = transform.position;
        puff.transform.localScale = Vector3.one * 0.18f;
        Object.Destroy(puff.GetComponent<SphereCollider>());

        var mat = new Material(Shader.Find("Standard"));
        mat.color = new Color(trailColor.r, trailColor.g, trailColor.b, 0.7f);
        mat.SetFloat("_Mode", 3);
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        puff.GetComponent<Renderer>().sharedMaterial = mat;

        Destroy(puff, 0.35f);
    }

    void OnTriggerEnter(Collider other)
    {
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;
        enemy.TakeDamage(damage);
        HitTarget();
    }

    void HitTarget()
    {
        Destroy(gameObject);
    }
}
