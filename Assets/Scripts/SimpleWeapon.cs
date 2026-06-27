using UnityEngine;

public class SimpleWeapon : MonoBehaviour
{
    public Transform weapon;
    public float swingAngle = 110f;
    public float swingSpeed = 12f;

    float swingT;
    bool swinging;
    Quaternion baseRot;

    void Start()
    {
        if (weapon != null) baseRot = weapon.localRotation;
    }

    void Update()
    {
        if (!swinging || weapon == null) return;

        swingT += Time.deltaTime * swingSpeed;
        float p = Mathf.Sin(swingT);
        weapon.localRotation = baseRot * Quaternion.Euler(0, 0, p * swingAngle);

        if (swingT >= Mathf.PI)
        {
            swinging = false;
            weapon.localRotation = baseRot;
        }
    }

    public void Swing()
    {
        swinging = true;
        swingT = 0f;
    }
}
