using UnityEngine;

public class SimpleWalker : MonoBehaviour
{
    public Transform body;
    public Transform[] legs;
    public float bobAmount = 0.08f;
    public float bobSpeed = 12f;
    public float legSwing = 25f;

    Vector3 bodyBasePos;
    Quaternion[] legBaseRots;

    void Start()
    {
        if (body != null) bodyBasePos = body.localPosition;
        legBaseRots = new Quaternion[legs.Length];
        for (int i = 0; i < legs.Length; i++)
            if (legs[i] != null) legBaseRots[i] = legs[i].localRotation;
    }

    void Update()
    {
        float t = Time.time * bobSpeed;

        if (body != null)
            body.localPosition = bodyBasePos + Vector3.up * Mathf.Sin(t * 2f) * bobAmount;

        for (int i = 0; i < legs.Length; i++)
        {
            if (legs[i] == null) continue;
            float phase = (i % 2 == 0) ? t : t + Mathf.PI;
            legs[i].localRotation = legBaseRots[i] * Quaternion.Euler(Mathf.Sin(phase) * legSwing, 0f, 0f);
        }
    }
}
