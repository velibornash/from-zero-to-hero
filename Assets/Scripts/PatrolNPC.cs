using UnityEngine;

public class PatrolNPC : MonoBehaviour
{
    public Transform waypointA;
    public Transform waypointB;
    public float moveSpeed = 4f;
    public float pauseTime = 2f;

    Animator anim;
    Transform target;
    float pauseTimer;
    Vector3 currentVelocity;

    void Start()
    {
        anim = GetComponentInChildren<Animator>();
        target = waypointA;
        pauseTimer = 0f;
    }

    void Update()
    {
        if (target == null || waypointA == null || waypointB == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        float dist = dir.magnitude;

        if (pauseTimer > 0f)
        {
            pauseTimer -= Time.deltaTime;
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, 20f * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;
            if (anim != null) anim.SetFloat("Speed", currentVelocity.magnitude / moveSpeed);
            return;
        }

        if (dist > 1.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            Vector3 targetVel = dir.normalized * moveSpeed;
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetVel, 20f * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;
            if (anim != null) anim.SetFloat("Speed", Mathf.Clamp01(currentVelocity.magnitude / moveSpeed));
        }
        else
        {
            target = target == waypointA ? waypointB : waypointA;
            pauseTimer = pauseTime;
            currentVelocity = Vector3.MoveTowards(currentVelocity, Vector3.zero, 20f * Time.deltaTime);
            transform.position += currentVelocity * Time.deltaTime;
            if (anim != null) anim.SetFloat("Speed", currentVelocity.magnitude / moveSpeed);
        }
    }
}
