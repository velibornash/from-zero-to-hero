using UnityEngine;

public class FloatAndFade : MonoBehaviour
{
    public float duration = 1.2f;
    public float riseHeight = 2f;

    float elapsed;
    Vector3 startPos;
    TextMesh tm;

    void Start()
    {
        startPos = transform.position;
        tm = GetComponent<TextMesh>();
        if (tm != null)
        {
            // Larger, brighter text with stronger shadow for visibility over the world
            tm.fontSize = 48;
            tm.fontStyle = FontStyle.Bold;
            tm.characterSize = 0.25f;
        }
        Destroy(gameObject, duration + 0.1f);
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        float p = Mathf.Clamp01(elapsed / duration);
        transform.position = startPos + Vector3.up * p * riseHeight;
        if (tm != null)
        {
            var c = tm.color;
            c.a = 1f - p;
            tm.color = c;
        }
    }
}
