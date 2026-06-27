using UnityEngine;

public class SimpleBob : MonoBehaviour
{
    public float bobAmount = 0.08f;
    public float bobSpeed = 10f;
    public float tiltAmount = 3f;

    Vector3 basePos;
    float lastX;

    void Start()
    {
        basePos = transform.localPosition;
    }

    void Update()
    {
        float dx = transform.position.x - lastX;
        lastX = transform.position.x;

        bool moving = Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.1f;
        if (moving)
        {
            float t = Time.time * bobSpeed;
            transform.localPosition = basePos + Vector3.up * Mathf.Abs(Mathf.Sin(t)) * bobAmount;
            transform.localRotation = Quaternion.Euler(0, 0, -dx * tiltAmount);
        }
        else
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePos, 0.2f);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, 0.2f);
        }
    }
}
