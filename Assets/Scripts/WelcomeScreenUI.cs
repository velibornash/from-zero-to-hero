using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class WelcomeScreenUI : MonoBehaviour
{
    GameObject panel;

    void Start()
    {
        if (PlayerPrefs.GetInt("HasSeenWelcome", 0) == 1) return;
        CreateUI();
    }

    void CreateUI()
    {
        Canvas canvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
        if (canvas == null) return;

        bool mob = PopupBase.IsMobile;

        panel = new GameObject("WelcomeScreen");
        panel.transform.SetParent(canvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        var overlay = panel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.8f);
        overlay.raycastTarget = true;

        // Content container
        float pw = mob ? 360f : 520f;
        float ph = mob ? 360f : 400f;

        var box = new GameObject("Box");
        box.transform.SetParent(panel.transform, false);
        var boxRt = box.AddComponent<RectTransform>();
        boxRt.anchorMin = new Vector2(0.5f, 0.5f);
        boxRt.anchorMax = new Vector2(0.5f, 0.5f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = Vector2.zero;
        boxRt.sizeDelta = new Vector2(pw, ph);

        var boxImg = box.AddComponent<Image>();
        boxImg.color = new Color(0.12f, 0.08f, 0.04f, 0.95f);

        // Title
        MakeText(box.transform, "Title", new Vector2(0, -30), new Vector2(pw - 30, 50),
            "WELCOME", 32, FontStyle.Bold,
            new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);

        // Welcome text
        float textY = -110;
        float textH = mob ? 160f : 180f;
        var intro = MakeText(box.transform, "Intro", new Vector2(0, textY), new Vector2(pw - 40, textH),
            "Welcome, hero.\n\nBuild your village, gather resources, and defend against the creatures of the valley. An elder will guide you on your journey.",
            mob ? 14 : 16, FontStyle.Normal,
            new Color(1f, 0.92f, 0.8f), TextAnchor.UpperCenter);

        // Dismiss button
        var btnGo = new GameObject("StartBtn");
        btnGo.transform.SetParent(box.transform, false);
        var btnRt = btnGo.AddComponent<RectTransform>();
        btnRt.anchorMin = new Vector2(0.5f, 0);
        btnRt.anchorMax = new Vector2(0.5f, 0);
        btnRt.pivot = new Vector2(0.5f, 0);
        btnRt.anchoredPosition = new Vector2(0, 20);
        btnRt.sizeDelta = new Vector2(160, 44);
        var btnImg = btnGo.AddComponent<Image>();
        btnImg.color = new Color(0.75f, 0.55f, 0.1f);
        var btn = btnGo.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(Dismiss);

        var btnLabel = new GameObject("Label");
        btnLabel.transform.SetParent(btnGo.transform, false);
        var btnLblRt = btnLabel.AddComponent<RectTransform>();
        btnLblRt.anchorMin = Vector2.zero;
        btnLblRt.anchorMax = Vector2.one;
        btnLblRt.offsetMin = Vector2.zero;
        btnLblRt.offsetMax = Vector2.zero;
        var btnText = btnLabel.AddComponent<Text>();
        btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        btnText.fontSize = 18;
        btnText.fontStyle = FontStyle.Bold;
        btnText.color = Color.white;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.text = "BEGIN";
    }

    void Dismiss()
    {
        PlayerPrefs.SetInt("HasSeenWelcome", 1);
        PlayerPrefs.Save();
        panel.SetActive(false);
        StartCoroutine(ShowTeslaIntro());
    }

    IEnumerator ShowTeslaIntro()
    {
        yield return new WaitForSeconds(0.3f);
        if (ElderPopup.Instance != null)
        {
            ElderPopup.Instance.Show(
                "I am Nikola Tesla. I will guide you through this world and help you with knowledge along the way.",
                5f);
        }
        else
        {
            Debug.LogWarning("[Welcome] ElderPopup.Instance is null — cannot show Tesla introduction.");
        }
    }

    Text MakeText(Transform parent, string name, Vector2 pos, Vector2 size,
        string content, int fontSize, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }
}
