using UnityEngine;
using UnityEngine.UI;
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
    static Vector3? s_moveTarget;
    public static Vector3? MoveTarget { get => s_moveTarget; set => s_moveTarget = value; }
    static bool s_mobileRun;
    public static bool MobileRun { get => s_mobileRun; set => s_mobileRun = value; }
    static bool s_mobileInteract;
    public static bool MobileInteract { get => s_mobileInteract; set => s_mobileInteract = value; }

    GameObject hpBarGo;
    RectTransform hpBarRect;
    UnityEngine.UI.Image hpBarFill;
    Text hpBarText;

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

        CreateWorldHealthBar();

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
        if (!IsDead)
        {
            HandleMovement();
            HandleAttack();
            HandleInteraction();
            HandleHealthRegen();
        }
        UpdateWorldHealthBar();
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

        bool directInput = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        Vector3 desiredDir = (forward * v + right * h);
        bool wantsToMove = desiredDir.sqrMagnitude > 0.01f;

        // If there's direct input, use it and clear any move target
        if (directInput)
            s_moveTarget = null;

        float currentSpeed = speed;
        bool running = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) || s_mobileRun;
        if (running) currentSpeed = speed * runMultiplier;

        if (wantsToMove)
        {
            Vector3 targetVel = desiredDir.normalized * currentSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVel, acceleration * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetVel), 0.15f);
            walkBobTimer += Time.deltaTime;
        }
        else if (s_moveTarget.HasValue)
        {
            Vector3 toTarget = s_moveTarget.Value - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude > 0.5f)
            {
                Vector3 targetVel = toTarget.normalized * currentSpeed;
                currentVelocity = Vector3.MoveTowards(currentVelocity, targetVel, acceleration * Time.deltaTime);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(toTarget), 0.15f);
                walkBobTimer += Time.deltaTime;
            }
            else
            {
                s_moveTarget = null;
                currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, deceleration * Time.deltaTime);
                if (currentVelocity.sqrMagnitude < 0.001f)
                {
                    currentVelocity = Vector3.zero;
                    walkBobTimer = 0f;
                }
            }
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

        // Speed proportional to actual velocity => animation matches body movement, no sliding
        float speedNorm = currentVelocity.magnitude / speed;
        if (anim != null)
        {
            anim.SetFloat("Speed", Mathf.Clamp01(speedNorm));
            anim.speed = Mathf.Max(0.3f, speedNorm);
        }

        bool moving = currentVelocity.sqrMagnitude > 0.1f;
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
        bool ePressed = Input.GetKeyDown(KeyCode.E) || s_mobileInteract;
        s_mobileInteract = false;
        if (!ePressed) return;

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
        if (IsDead) return;
        IsDead = true;

        // Stop movement and animation
        currentVelocity = Vector3.zero;
        if (anim != null)
        {
            anim.SetFloat("Speed", 0f);
            anim.speed = 1f;
        }
        if (controller != null)
        {
            controller.Move(Vector3.down * Time.deltaTime);
        }
        if (modelRoot != null)
        {
            var lp = modelRoot.localPosition;
            lp.y = baseModelY;
            modelRoot.localPosition = lp;
            modelRoot.localRotation = Quaternion.identity;
        }

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

    void CreateWorldHealthBar()
    {
        Canvas mainCanvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                mainCanvas = c;
                break;
            }
        }
        if (mainCanvas == null) return;

        hpBarGo = new GameObject("HeroHPBar");
        hpBarGo.transform.SetParent(mainCanvas.transform, false);
        hpBarRect = hpBarGo.AddComponent<RectTransform>();
        hpBarRect.anchorMin = new Vector2(0.5f, 0.5f);
        hpBarRect.anchorMax = new Vector2(0.5f, 0.5f);
        hpBarRect.pivot = new Vector2(0.5f, 0.5f);
        hpBarRect.sizeDelta = new Vector2(140, 20);

        var bg = new GameObject("BG");
        bg.transform.SetParent(hpBarGo.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.1f, 0.05f, 0.05f, 0.7f);

        var fill = new GameObject("Fill");
        fill.transform.SetParent(hpBarGo.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = Vector2.zero;
        fillRt.anchorMax = Vector2.one;
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        hpBarFill = fill.AddComponent<Image>();
        hpBarFill.color = new Color(0.0f, 0.55f, 0.0f);
        hpBarFill.type = Image.Type.Filled;
        hpBarFill.fillMethod = Image.FillMethod.Horizontal;

        var label = new GameObject("Label");
        label.transform.SetParent(hpBarGo.transform, false);
        var lRt = label.AddComponent<RectTransform>();
        lRt.anchorMin = Vector2.zero;
        lRt.anchorMax = Vector2.one;
        lRt.offsetMin = Vector2.zero;
        lRt.offsetMax = Vector2.zero;
        hpBarText = label.AddComponent<Text>();
        hpBarText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hpBarText.fontSize = 14;
        hpBarText.fontStyle = FontStyle.Bold;
        hpBarText.color = Color.white;
        hpBarText.alignment = TextAnchor.MiddleCenter;
        hpBarText.resizeTextForBestFit = true;
        hpBarText.resizeTextMinSize = 8;
        hpBarText.resizeTextMaxSize = 16;
        hpBarText.text = "";
        hpBarText.raycastTarget = false;
        label.AddComponent<Outline>();
        var oe = label.GetComponent<Outline>();
        oe.effectColor = Color.black;
        oe.effectDistance = new Vector2(1, 1);
    }

    void UpdateWorldHealthBar()
    {
        if (hpBarFill == null || hpBarText == null || hpBarRect == null) return;

        float pct = (float)Health / Mathf.Max(1, maxHealth);
        hpBarFill.fillAmount = pct;
        if (pct > 0.6f) hpBarFill.color = new Color(0.0f, 0.55f, 0.0f);
        else if (pct > 0.3f) hpBarFill.color = new Color(1f, 0.7f, 0.2f);
        else hpBarFill.color = new Color(1f, 0.2f, 0.2f);
        hpBarText.text = $"{Health}/{maxHealth}";

        var cam = Camera.main;
        if (cam == null) return;
        Vector3 headPos = transform.position + Vector3.up * 7.5f;
        Vector3 screenPos = cam.WorldToScreenPoint(headPos);

        hpBarGo.SetActive(screenPos.z > 0);
        if (screenPos.z <= 0) return;

        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            hpBarRect.parent as RectTransform,
            screenPos,
            null,
            out localPos);
        hpBarRect.anchoredPosition = localPos + new Vector2(0, 24);
    }
}
