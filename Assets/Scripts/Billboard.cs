using UnityEngine;
using UnityEngine.UI;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        var dir = cam.transform.position - transform.position;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        var c = GetComponent<Canvas>();
        if (c != null && c.worldCamera == null)
            c.worldCamera = cam;
    }
}
