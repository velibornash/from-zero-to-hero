using UnityEngine;

public class PlayerController3D : MonoBehaviour
{
    public float speed = 10f;
    CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 3.5f;
            controller.radius = 1.0f;
            controller.center = new Vector3(0, 1.75f, 0);
        }
    }

    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        var cam = Camera.main;
        if (cam == null) return;

        Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
        Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

        Vector3 move = (forward * v + right * h);
        if (move.sqrMagnitude > 0.01f)
        {
            move.Normalize();
            var displacement = move * speed * Time.deltaTime;
            displacement.y = -1f * Time.deltaTime;
            controller.Move(displacement);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(move), 0.15f);
        }
    }
}
