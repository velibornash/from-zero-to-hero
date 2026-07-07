using UnityEngine;

public class CameraFollow3D : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 4f;
    public float baseDist = 45f;
    public float minDist = 15f;
    public float maxDist = 55f;

    public float pitch = 55f;
    public float yaw = 0f;
    public float currentDist = 45f;

    void Start()
    {
        if (target == null)
        {
            var player = Object.FindAnyObjectByType<PlayerController3D>();
            if (player != null) target = player.transform;
        }
        currentDist = baseDist;

        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 30f;
        RenderSettings.fogEndDistance = 55f;
        RenderSettings.fogColor = new Color(0.20f, 0.40f, 0.15f);
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

        // Mobile touch controls — only on the RIGHT half of the screen
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            // Ignore touches on the left half (joystick area) and UI
            if (t.position.x > Screen.width * 0.5f && t.phase == TouchPhase.Moved)
            {
                yaw += t.deltaPosition.x * 0.3f;
                pitch = Mathf.Clamp(pitch - t.deltaPosition.y * 0.2f, 15f, 80f);
            }
        }
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);
            // Only respond if at least one touch is on the right half
            bool rightSide = t1.position.x > Screen.width * 0.5f || t2.position.x > Screen.width * 0.5f;
            if (rightSide)
            {
                Vector2 prevDelta = (t1.position - t1.deltaPosition) - (t2.position - t2.deltaPosition);
                Vector2 currDelta = t1.position - t2.position;
                float prevMag = prevDelta.magnitude;
                float currMag = currDelta.magnitude;
                if (Mathf.Abs(currMag - prevMag) > 0.5f)
                {
                    currentDist = Mathf.Clamp(currentDist - (currMag - prevMag) * 0.1f, minDist, maxDist);
                }
            }
        }

        Vector3 dir = Quaternion.Euler(pitch, yaw, 0) * new Vector3(0, 0, -currentDist);
        Vector3 desired = target.position + dir;
        transform.position = Vector3.Lerp(transform.position, desired, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 0.5f);
    }
}
