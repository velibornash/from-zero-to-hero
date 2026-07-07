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
            if (anim != null) anim.SetFloat("Speed", 0f);
            return;
        }

        if (dist > 1.5f)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
            transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
            if (anim != null) anim.SetFloat("Speed", 1f);
        }
        else
        {
            target = target == waypointA ? waypointB : waypointA;
            pauseTimer = pauseTime;
            if (anim != null) anim.SetFloat("Speed", 0f);
        }
    }
}
