using UnityEngine;

public class NPCPatrol : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float waitTime = 3f;
    public float patrolRadius = 5f;
    public bool idleOnly = false;

    Vector3 origin;
    Vector3 target;
    float timer;
    CharacterController controller;

    void Start()
    {
        origin = transform.position;
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 3f;
            controller.radius = 0.8f;
            controller.center = new Vector3(0, 1.5f, 0);
        }
        PickNewTarget();
    }

    void Update()
    {
        if (idleOnly) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0;

            if (dir.sqrMagnitude < 0.5f)
            {
                timer = waitTime + Random.Range(0f, 2f);
                PickNewTarget();
            }
            else
            {
                Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
                move.y = -1f * Time.deltaTime;
                controller.Move(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
            }
        }
    }

    void PickNewTarget()
    {
        Vector2 r = Random.insideUnitCircle * patrolRadius;
        target = origin + new Vector3(r.x, 0, r.y);
        timer = Random.Range(0f, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.2f);
        Gizmos.DrawWireSphere(Application.isPlaying ? origin : transform.position, patrolRadius);
    }
}
