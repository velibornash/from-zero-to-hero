using UnityEngine;

public class ProceduralWalk : MonoBehaviour
{
    public float bobHeight = 0.15f;
    public float bobSpeed = 10f;
    public float swayAngle = 3f;

    Vector3 basePos;
    Quaternion baseRot;

    void Start()
    {
        basePos = transform.localPosition;
        baseRot = transform.localRotation;
    }

    void Update()
    {
        float t = Time.time * bobSpeed;
        transform.localPosition = basePos + Vector3.up * Mathf.Sin(t * 2f) * bobHeight;
        transform.localRotation = baseRot * Quaternion.Euler(0f, 0f, Mathf.Sin(t) * swayAngle);
    }

    public void SetSpeed(float speed)
    {
        bobSpeed = Mathf.Lerp(0f, 12f, speed);
        bobHeight = Mathf.Lerp(0f, 0.2f, speed);
        swayAngle = Mathf.Lerp(0f, 4f, speed);
        enabled = speed > 0.01f;
    }
}
