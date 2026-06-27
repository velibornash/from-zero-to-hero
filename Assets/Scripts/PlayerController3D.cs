using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float speed = 10f;
    public float attackRadius = 2.2f;
    public int attackDamage = 1;
    public float attackRate = 0.35f;

    CharacterController controller;
    Animator anim;
    float attackTimer;
    Transform modelRoot;
    float baseModelY;

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

        anim = GetComponentInChildren<Animator>();
        // Find the visual model root (first child with a renderer)
        foreach (Transform child in transform)
            if (child.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                { modelRoot = child; break; }
        if (modelRoot != null) baseModelY = modelRoot.localPosition.y;

        // Remove the ugly square self-shadow under the hero; buildings still cast shadows.
        foreach (var rend in GetComponentsInChildren<Renderer>())
            rend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    void Update()
    {
        HandleMovement();
        HandleAttack();
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

        // Procedural walk bob fallback (when animation clips are null/sliding)
        if (modelRoot != null)
        {
            float bob = moving ? Mathf.Sin(Time.time * 14f) * 0.12f : 0f;
            var lp = modelRoot.localPosition;
            lp.y = baseModelY + bob;
            modelRoot.localPosition = lp;
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
            enemy.TakeDamage(attackDamage);
            attacked = true;
        }

        if (attacked)
        {
            attackTimer = 0f;
            GetComponent<SimpleWeapon>()?.Swing();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f);
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
