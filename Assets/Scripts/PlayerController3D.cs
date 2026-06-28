using UnityEngine;
using System.Collections;

public class PlayerController3D : MonoBehaviour
{
    public float speed = 10f;
    public float attackRadius = 4f;
    public int attackDamage = 1;
    public float attackRate = 0.35f;

    // Health system
    public static int maxHealth = 100;
    public static int Health = 100;
    public float regenRate = 5f;       // HP per second when in village + out of combat
    public float regenDelay = 1f;      // seconds after last hit before regen starts
    public float villageRadius = 80f;  // distance from center (0,0,0) considered "village"
    public static bool IsDead = false;

    CharacterController controller;
    Animator anim;
    float attackTimer;
    float lastHitTime;
    Transform modelRoot;
    float baseModelY;
    float regenAccumulator;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2.0f;
            controller.radius = 0.4f;
            controller.center = new Vector3(0, 1.0f, 0);
            controller.skinWidth = 0.02f;
        }

        Health = maxHealth;
        IsDead = false;
        lastHitTime = -999f;

        anim = GetComponentInChildren<Animator>();
        Debug.Log($"Hero Start: anim={(anim != null)}, controller={(anim != null ? anim.runtimeAnimatorController?.name : "null")}");
        if (anim != null) StartCoroutine(LogAnimState());
        // Find the visual model root (first child with a renderer)
        foreach (Transform child in transform)
            if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                { modelRoot = child; break; }
        if (modelRoot != null) baseModelY = modelRoot.localPosition.y;

        // Remove the ugly square self-shadow under the hero; buildings still cast shadows.
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    IEnumerator LogAnimState()
    {
        yield return new WaitForSeconds(0.5f);
        var info = anim.GetCurrentAnimatorStateInfo(0);
        var clips = anim.GetCurrentAnimatorClipInfo(0);
        string cInfo = clips.Length > 0 ? $"{clips[0].clip.name}({clips[0].clip.length:F2}s)" : "none";
        Debug.Log($"Hero anim: stateHash={info.shortNameHash}, len={info.length:F2}s, speed={info.speedMultiplier}, clip={cInfo}");
        // Check all state motions in the controller
        var ctrl = anim.runtimeAnimatorController;
        if (ctrl != null)
        {
            for (int li = 0; li < ctrl.animationClips.Length; li++)
            {
                var c = ctrl.animationClips[li];
                Debug.Log($"Hero anim clip[{li}]: {c.name} ({c.length:F2}s empty={c.empty})");
            }
        }
    }

    void Update()
    {
        if (IsDead) return;
        HandleMovement();
        HandleAttack();
        HandleHealthRegen();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        // Add joystick input for mobile
        Vector2 joy = Joystick.Direction;
        h += joy.x;
        v += joy.y;
        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        Vector3 move = (forward * v + right * h);
        bool moving = move.sqrMagnitude > 0.01f;
        if (moving)
        {
            move.Normalize();
            var displacement = move * speed * Time.deltaTime;
            displacement.y = -1f * Time.deltaTime;
            controller.Move(displacement);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 0.15f);
        }

        if (anim != null)
            anim.SetFloat("Speed", moving ? 1f : 0f);

        // Procedural walk bob — makes movement look like walking even when
        // animation clips are missing or the avatar rig doesn't match.
        if (modelRoot != null && moving)
        {
            float t = Time.time * 12f;
            float bob = Mathf.Sin(t) * 0.25f;
            float sway = Mathf.Sin(t * 0.5f) * 4f;
            var lp = modelRoot.localPosition;
            lp.y = baseModelY + bob;
            modelRoot.localPosition = lp;
            modelRoot.localRotation = Quaternion.Euler(0f, 0f, sway);
        }
        else if (modelRoot != null)
        {
            var lp = modelRoot.localPosition;
            lp.y = baseModelY;
            modelRoot.localPosition = lp;
            modelRoot.localRotation = Quaternion.identity;
        }
    }

    void HandleAttack()
    {
        attackTimer += Time.deltaTime;
        if (attackTimer < attackRate) return;

        var hits = Physics.OverlapSphere(transform.position, attackRadius, Physics.AllLayers, QueryTriggerInteraction.Collide);
        bool attacked = false;
        foreach (var hit in hits)
        {
            var enemy = hit.GetComponent<Enemy>();
            if (enemy == null) continue;
            Debug.Log($"Hero attacking {enemy.enemyName} at dist={Vector3.Distance(transform.position, hit.transform.position):F2}");
            enemy.TakeDamage(attackDamage);
            attacked = true;
        }

        if (attacked)
        {
            attackTimer = 0f;
            if (anim != null) anim.SetTrigger("Attack");
            GetComponent<SimpleWeapon>()?.Swing();
        }
    }

    void HandleHealthRegen()
    {
        if (Health >= maxHealth) return;
        if (Time.time - lastHitTime < regenDelay) return;
        if (!IsInVillage()) return;

        regenAccumulator += regenRate * Time.deltaTime;
        int regenAmount = Mathf.FloorToInt(regenAccumulator);
        if (regenAmount > 0)
        {
            regenAccumulator -= regenAmount;
            Health = Mathf.Min(maxHealth, Health + regenAmount);
        }
    }

    bool IsInVillage()
    {
        // Hero is in village if within villageRadius from center
        Vector2 xz = new Vector2(transform.position.x, transform.position.z);
        return xz.magnitude <= villageRadius;
    }

    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (damage > 0) lastHitTime = Time.time;
        Health -= damage;
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    // Reset regen timer (for restart)
    public void ResetRegen()
    {
        lastHitTime = -999f;
    }

    void Die()
    {
        IsDead = true;
        HUDController.PushEvent("Hero has fallen! The village is in ruins...");
        // Trigger game over after a short delay
        Invoke(nameof(ShowGameOver), 1.5f);
    }

    void ShowGameOver()
    {
        GameOverScreen.Show();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
