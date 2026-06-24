using UnityEngine;
using UnityEngine.UI;

public class FloatingText : MonoBehaviour
{
    float timer;
    Text label;
    Color startColor;

    void Start()
    {
        label = GetComponentInChildren<Text>();
        if (label != null) startColor = label.color;
    }

    void Update()
    {
        timer += Time.deltaTime;
        transform.position += Vector3.up * 1.8f * Time.deltaTime;

        if (label != null)
            label.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Clamp01(1f - timer / 1.2f));

        if (timer >= 1.2f)
            Destroy(gameObject);
    }

    public static void Show(Vector3 position, int amount, Color color)
    {
        var go = new GameObject("FloatingDamage");
        go.transform.position = position + Vector3.up * 2f;

        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(3, 1);

        var textGo = new GameObject("Text");
        textGo.transform.SetParent(go.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.sizeDelta = new Vector2(3, 1);

        var text = textGo.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 100;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = TextAnchor.MiddleCenter;
        text.text = amount.ToString();

        go.AddComponent<FloatingText>();
    }
}
