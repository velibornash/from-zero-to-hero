using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    public string resourceType = "wood";
    public int amount = 5;
    public float lifetime = 30f;

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= lifetime)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        HUDController.AddResource(resourceType, amount);

        FloatingText.Show(transform.position, amount, new Color(0.6f, 0.4f, 0.1f));

        Destroy(gameObject);
    }
}
