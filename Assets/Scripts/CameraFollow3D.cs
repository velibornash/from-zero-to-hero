using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 4f;
    public float baseDist = 45f;
    public float minDist = 15f;
    public float maxDist = 60f;

    public float pitch = 55f;
    float yaw;
    public float currentDist = 45f;

    void Start()
    {
        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }
        currentDist = baseDist;
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Scroll to zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            currentDist = Mathf.Clamp(currentDist - scroll * 10f, minDist, maxDist);
        }

        // Right-click drag to rotate
        if (Input.GetMouseButton(1))
        {
            yaw += Input.GetAxis("Mouse X") * 3f;
            pitch = Mathf.Clamp(pitch - Input.GetAxis("Mouse Y") * 2f, 15f, 80f);
        }

        Vector3 dir = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -currentDist);
        Vector3 desired = target.position + dir;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
