using UnityEngine;
using UnityEngine.UI;

public class ElderPopup : MonoBehaviour
{
    public static ElderPopup Instance;
    GameObject panel;
    Text bodyText;
    Image elderImage;
    float hideTimer;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (panel != null && panel.activeSelf && hideTimer > 0)
        {
            hideTimer -= Time.deltaTime;
            if (hideTimer <= 0)
                panel.SetActive(false);
        }
    }

    public void Show(string message, float duration = 4f)
    {
        if (panel == null) CreateUI();
        bodyText.text = message;
        panel.SetActive(true);
        hideTimer = duration;
    }

    void CreateUI()
    {
        Canvas canvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
        if (canvas == null) return;

        panel = new GameObject("ElderPopup");
        panel.transform.SetParent(canvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0);
        panelRt.anchorMax = new Vector2(0.5f, 0);
        panelRt.pivot = new Vector2(0.5f, 0);
        panelRt.anchoredPosition = new Vector2(0, 20);
        panelRt.sizeDelta = new Vector2(500, 120);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.08f, 0.04f, 0.92f);

        var elderGo = new GameObject("ElderPortrait");
        elderGo.transform.SetParent(panel.transform, false);
        var elderRt = elderGo.AddComponent<RectTransform>();
        elderRt.anchorMin = new Vector2(0, 0.5f);
        elderRt.anchorMax = new Vector2(0, 0.5f);
        elderRt.pivot = new Vector2(0, 0.5f);
        elderRt.anchoredPosition = new Vector2(10, 0);
        elderRt.sizeDelta = new Vector2(80, 80);
        elderImage = elderGo.AddComponent<Image>();
        elderImage.color = Color.white;
        elderImage.raycastTarget = false;

        var elderTex = Resources.Load<Texture2D>("Chapters/elder");
        if (elderTex != null)
        {
            var sprite = Sprite.Create(elderTex, new Rect(0, 0, elderTex.width, elderTex.height), Vector2.one * 0.5f);
            elderImage.sprite = sprite;
        }
        else
        {
            elderImage.color = new Color(0.6f, 0.5f, 0.3f);
        }

        var textGo = new GameObject("Message");
        textGo.transform.SetParent(panel.transform, false);
        var textRt = textGo.AddComponent<RectTransform>();
        textRt.anchorMin = new Vector2(0, 0);
        textRt.anchorMax = new Vector2(1, 1);
        textRt.offsetMin = new Vector2(100, 10);
        textRt.offsetMax = new Vector2(-15, -10);
        bodyText = textGo.AddComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize = 18;
        bodyText.fontStyle = FontStyle.Bold;
        bodyText.color = new Color(1f, 0.92f, 0.7f);
        bodyText.alignment = TextAnchor.MiddleLeft;
        bodyText.text = "";
        bodyText.raycastTarget = false;
    }
}
