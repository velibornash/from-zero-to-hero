using UnityEngine;

public class Combat : MonoBehaviour
{
    public float maxHp = 50f;
    public float damagePerSec = 10f;
    public float range = 2.5f;
    public Transform hpBar;

    float hp;
    bool isPlayer;
    bool isDead;
    Renderer[] rends;
    Color[] origColors;
    float flashTimer;
    float damageAccum;

    void Start()
    {
        hp = maxHp;
        isPlayer = CompareTag("Player");
        rends = GetComponentsInChildren<Renderer>();
        origColors = new Color[rends.Length];
        for (int i = 0; i < rends.Length; i++)
            origColors[i] = rends[i].material.color;
    }

    void Update()
    {
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            if (flashTimer <= 0)
                for (int i = 0; i < rends.Length; i++)
                    rends[i].material.color = origColors[i];
        }

        if (isDead) return;

        if (hp <= 0f)
        {
            isDead = true;
            HUDController.PushEvent($"{gameObject.name} defeated!");
            Destroy(gameObject, 0.5f);
            return;
        }

        var target = FindTarget();
        if (target != null)
        {
            Vector3 dir = target.transform.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.15f);

            float dmg = damagePerSec * Time.deltaTime;
            target.TakeDamage(dmg);
        }
    }

    Combat FindTarget()
    {
        Combat best = null;
        float bestDist = range + 1f;
        var all = FindObjectsByType<Combat>();
        foreach (var c in all)
        {
            if (c == this || c.isDead) continue;
            if (isPlayer && c.isPlayer) continue;
            if (!isPlayer && !c.isPlayer) continue;

            float d = Vector3.Distance(transform.position, c.transform.position);
            if (d < range && d < bestDist)
            {
                best = c;
                bestDist = d;
            }
        }
        return best;
    }

    public void TakeDamage(float amount)
    {
        hp -= amount;
        if (hpBar != null)
            hpBar.localScale = new Vector3(Mathf.Clamp01(hp / maxHp), 1, 1);

        flashTimer = 0.1f;
        foreach (var r in rends)
            r.material.color = Color.red;

        damageAccum += amount;
        if (damageAccum >= 5f)
        {
            FloatingText.Show(transform.position + Vector3.up * 0.5f, Mathf.RoundToInt(damageAccum), isPlayer ? Color.red : new Color(1f, 0.8f, 0.2f));
            damageAccum = 0;
        }
    }
}
