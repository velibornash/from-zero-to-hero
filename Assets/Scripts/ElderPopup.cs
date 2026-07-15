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

        bool mob = PopupBase.IsMobile;
        float pw = mob ? 320f : 500f;
        float ph = mob ? 90f : 120f;
        float portraitSize = mob ? 56f : 80f;
        float padding = mob ? 6f : 10f;
        int fontSize = mob ? 14 : 18;

        panel = new GameObject("ElderPopup");
        panel.transform.SetParent(canvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = new Vector2(0.5f, 0);
        panelRt.anchorMax = new Vector2(0.5f, 0);
        panelRt.pivot = new Vector2(0.5f, 0);
        panelRt.anchoredPosition = new Vector2(0, 20);
        panelRt.sizeDelta = new Vector2(pw, ph);

        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.08f, 0.04f, 0.92f);

        var elderGo = new GameObject("ElderPortrait");
        elderGo.transform.SetParent(panel.transform, false);
        var elderRt = elderGo.AddComponent<RectTransform>();
        elderRt.anchorMin = new Vector2(0, 0.5f);
        elderRt.anchorMax = new Vector2(0, 0.5f);
        elderRt.pivot = new Vector2(0, 0.5f);
        elderRt.anchoredPosition = new Vector2(padding, 0);
        elderRt.sizeDelta = new Vector2(portraitSize, portraitSize);
        elderImage = elderGo.AddComponent<Image>();
        elderImage.color = Color.white;
        elderImage.raycastTarget = false;

        var elderTex = Resources.Load<Texture2D>("Chapters/tesla");
        if (elderTex == null)
            elderTex = Resources.Load<Texture2D>("Chapters/elder");
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
        textRt.offsetMin = new Vector2(portraitSize + padding * 2, padding);
        textRt.offsetMax = new Vector2(-padding, -padding);
        bodyText = textGo.AddComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize = fontSize;
        bodyText.fontStyle = FontStyle.Bold;
        bodyText.color = new Color(1f, 0.92f, 0.7f);
        bodyText.alignment = TextAnchor.MiddleLeft;
        bodyText.text = "";
        bodyText.raycastTarget = false;
        var outline = textGo.AddComponent<Outline>();
        outline.effectColor = new Color(0, 0, 0, 0.7f);
        outline.effectDistance = new Vector2(1, 1);
    }
}
