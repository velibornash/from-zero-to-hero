using UnityEngine;

public class WorkerAI : MonoBehaviour
{
    public string resourceType = "wood";
    public float gatherInterval = 12f;
    public float gatherRange = 25f;
    public int gatherAmount = 2;

    float timer;
    CharacterController controller;
    Vector3 homePos;
    Transform currentTarget;
    bool gathering;
    float gatherTimer;

    void Start()
    {
        homePos = transform.position;
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 3f;
            controller.radius = 0.6f;
            controller.center = new Vector3(0, 1.5f, 0);
        }
        timer = gatherInterval * 0.5f;
    }

    void Update()
    {
        if (gathering)
        {
            gatherTimer -= Time.deltaTime;
            if (gatherTimer <= 0)
            {
                SpawnPickup();
                gathering = false;
                timer = gatherInterval;
            }
            return;
        }

        timer -= Time.deltaTime;
        if (timer <= 0 && currentTarget == null)
        {
            FindResource();
        }

        if (currentTarget != null)
        {
            Vector3 dir = currentTarget.position - transform.position;
            dir.y = 0;
            if (dir.sqrMagnitude < 2f)
            {
                gathering = true;
                gatherTimer = 2f;
                currentTarget = null;
            }
            else
            {
                Vector3 move = dir.normalized * 3f * Time.deltaTime;
                move.y = -1f * Time.deltaTime;
                controller.Move(move);
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
            }
        }
        else
        {
            ReturnHome();
        }
    }

    void FindResource()
    {
        float best = gatherRange;
        Transform bestT = null;

        if (resourceType == "wood")
        {
            foreach (var tree in TreeController.AllTrees)
            {
                if (tree.chopped) continue;
                float d = Vector3.Distance(transform.position, tree.transform.position);
                if (d < best) { best = d; bestT = tree.transform; }
            }
        }
        else
        {
            foreach (var node in ResourceNode.AllNodes)
            {
                if (node.harvested) continue;
                float d = Vector3.Distance(transform.position, node.transform.position);
                if (d < best) { best = d; bestT = node.transform; }
            }
        }

        if (bestT != null)
            currentTarget = bestT;
        else
            timer = 3f;
    }

    void ReturnHome()
    {
        Vector3 dir = homePos - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 1f)
        {
            Vector3 move = dir.normalized * 2f * Time.deltaTime;
            move.y = -1f * Time.deltaTime;
            controller.Move(move);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 0.1f);
        }
        else
        {
            timer = gatherInterval;
        }
    }

    void SpawnPickup()
    {
        Vector3 spawnPos = homePos + new Vector3(Random.Range(-2f, 2f), 0.5f, Random.Range(-2f, 2f));
        var pickup = new GameObject("WorkerPickup");
        pickup.transform.position = spawnPos;
        var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.transform.SetParent(pickup.transform, false);
        quad.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        quad.transform.localPosition = Vector3.zero;
        Destroy(quad.GetComponent<MeshCollider>());
        var mat = new Material(Shader.Find("Unlit/Transparent"));
        mat.color = resourceType == "wood"
            ? new Color(0.55f, 0.35f, 0.15f)
            : new Color(0.5f, 0.5f, 0.5f);
        quad.GetComponent<Renderer>().sharedMaterial = mat;

        var pickupComp = pickup.AddComponent<ResourcePickup>();
        pickupComp.resourceType = resourceType;
        pickupComp.amount = gatherAmount;

        var col = pickup.AddComponent<SphereCollider>();
        col.isTrigger = true;
        col.radius = 1.2f;
    }
}
