using UnityEngine;

public class Billboard : MonoBehaviour
{
    void LateUpdate()
    {
        var cam = Camera.main;
        if (cam == null) return;
        transform.forward = cam.transform.forward;
    }
}
