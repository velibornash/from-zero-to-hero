using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public string resourceType = "wood";
    public int amount = 5;
    public float lifetime = 30f;
    public float readyDelay = 1.5f;

    float timer;
    bool ready;

    void Start()
    {
        Invoke(nameof(MakeReady), readyDelay);
    }

    void MakeReady()
    {
        ready = true;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!ready) return;
        if (!other.CompareTag("Player")) return;

        HUDController.AddResource(resourceType, amount);

        Color c = resourceType == "wood" ? new Color(0.6f, 0.4f, 0.1f)
               : resourceType == "stone" ? new Color(0.5f, 0.5f, 0.5f)
               : new Color(0.8f, 0.8f, 0.2f);
        FloatingText.Show(transform.position + Vector3.up * 1f, amount, c);

        Destroy(gameObject);
    }
}
