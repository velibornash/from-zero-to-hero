using UnityEngine;
using System.Collections;

public class PlayerController3D : MonoBehaviour
{
    public float speed = 6f;
    public float runMultiplier = 1.5f;
    public float acceleration = 30f;
    public float deceleration = 60f;
    public float attackRadius = 4f;
    public int attackDamage = 1;
    public float attackRate = 0.35f;

    public static int maxHealth = 100;
    public static int Health = 100;
    public float regenRate = 5f;
    public float regenDelay = 1f;
    public float villageRadius = 80f;
    public static bool IsDead = false;

    CharacterController controller;
    Animator anim;
    float attackTimer;
    float lastHitTime;
    Transform modelRoot;
    float baseModelY;
    float regenAccumulator;
    Vector3 currentVelocity;
    float walkBobTimer;

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
        if (anim != null) anim.applyRootMotion = false;
        foreach (Transform child in transform)
            if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                { modelRoot = child; break; }
        if (modelRoot != null) baseModelY = modelRoot.localPosition.y;

        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        TreeController.InitializeAllTrees();
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
        HandleInteraction();
        HandleHealthRegen();
    }

    void HandleMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector2 joy = Joystick.Direction;
        h += joy.x;
        v += joy.y;
        h = Mathf.Clamp(h, -1f, 1f);
        v = Mathf.Clamp(v, -1f, 1f);

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        Vector3 desiredDir = (forward * v + right * h);
        bool wantsToMove = desiredDir.sqrMagnitude > 0.01f;

        float currentSpeed = speed;
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        if (running) currentSpeed = speed * runMultiplier;

        if (wantsToMove)
        {
            Vector3 targetVel = desiredDir.normalized * currentSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVel, acceleration * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetVel), 0.15f);
            walkBobTimer += Time.deltaTime;
        }
        else
        {
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
            if (currentVelocity.sqrMagnitude < 0.001f)
            {
                currentVelocity = Vector3.zero;
                walkBobTimer = 0f;
            }
        }

        controller.Move(currentVelocity * Time.deltaTime + Vector3.down * Time.deltaTime);

        // Idle ONLY when no keyboard/touch input AND velocity ≈ 0
        bool hasInput = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f
                     || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)
                     || Input.GetKey(KeyCode.E)
                     || Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Mouse1);
        if (anim != null)
            anim.SetFloat("Speed", (hasInput || currentVelocity.sqrMagnitude > 0.1f) ? 1f : 0f);

        bool moving = hasInput || currentVelocity.sqrMagnitude > 0.1f;
        if (modelRoot != null && moving)
        {
            float t = walkBobTimer * 12f;
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

    void HandleInteraction()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return;

        TreeController nearestTree = null;
        ResourceNode nearestNode = null;
        float bestTree = attackRadius + 1f;
        float bestNode = 4f + 1f;
        foreach (var tree in TreeController.AllTrees)
        {
            if (tree.chopped) continue;
            float d = Vector3.Distance(transform.position, tree.transform.position);
            if (d < attackRadius && d < bestTree)
            {
                bestTree = d;
                nearestTree = tree;
            }
        }
        foreach (var node in ResourceNode.AllNodes)
        {
            float d = Vector3.Distance(transform.position, node.transform.position);
            if (d < 4f && d < bestNode)
            {
                bestNode = d;
                nearestNode = node;
            }
        }
        if (nearestTree != null) nearestTree.Chop(this);
        else if (nearestNode != null) nearestNode.Harvest(this);
    }

    public void ForceAttack()
    {
        if (anim != null) anim.SetTrigger("Attack");
        GetComponent<SimpleWeapon>()?.Swing();
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
        Debug.Log($"Hero takes {damage} damage! Health: {Health}/{maxHealth}");
        // Flash hero model red for visual feedback
        if (modelRoot != null) StartCoroutine(DamageFlash());
        if (Health <= 0)
        {
            Health = 0;
            Die();
        }
    }

    System.Collections.IEnumerator DamageFlash()
    {
        var rends = modelRoot.GetComponentsInChildren<Renderer>();
        foreach (var r in rends) r.material.color = Color.red;
        yield return new WaitForSeconds(0.12f);
        foreach (var r in rends) r.material.color = Color.white;
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
