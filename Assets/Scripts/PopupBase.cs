using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// Shared base for all Travian-style popups (building complete, intro, game over).
/// Builds a parchment panel with a red banner, gold trim, body area, and
/// close button. All popups use this same structure for visual consistency.
/// </summary>
public class PopupBase : MonoBehaviour
{
    protected Canvas canvas;
    public GameObject overlay;
    public GameObject panel;
    protected Text titleText;
    protected Text bodyText;

    // Standard dimensions
    public const float PANEL_WIDTH = 600f;
    public const float PANEL_HEIGHT = 420f;
    public const float TITLE_BAR_HEIGHT = 70f;
    public const float BORDER_INSET = 10f;

    public void BuildUI(string defaultTitle, string defaultBody)
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("PopupBase: no Canvas found!");
            return;
        }

        // Dark backdrop (click to dismiss)
        overlay = new GameObject("PopupOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var oRt = overlay.AddComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.sizeDelta = Vector2.zero;
        var oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.5f);
        oImg.raycastTarget = true;
        overlay.SetActive(false);

        var trigger = overlay.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => Hide());
        trigger.triggers.Add(entry);

        // Panel (parchment background, centered on screen)
        panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = UIStyleHelper.MakeParchmentSprite(256, 256);
        pImg.type = Image.Type.Sliced;
        pImg.color = Color.white;
        pImg.raycastTarget = true;

        // Ornate gold border
        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.offsetMin = new Vector2(-BORDER_INSET, -BORDER_INSET);
        bRt.offsetMax = new Vector2(BORDER_INSET, BORDER_INSET);
        var bImg = border.AddComponent<Image>();
        bImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 14, 18);
        bImg.type = Image.Type.Sliced;
        bImg.color = Color.white;
        bImg.raycastTarget = false;

        // Red title banner (70px tall at top)
        var titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panel.transform, false);
        var tbRt = titleBar.AddComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = Vector2.zero;
        tbRt.sizeDelta = new Vector2(0, -TITLE_BAR_HEIGHT);
        var tbImg = titleBar.AddComponent<Image>();
        tbImg.color = new Color(0.55f, 0.10f, 0.08f);
        tbImg.raycastTarget = false;

        // Gold trim above and below title bar
        CreateTrim(panel.transform, 0f, true);
        CreateTrim(panel.transform, -TITLE_BAR_HEIGHT, false);

        // Title text (centered in red banner)
        var titleGo = new GameObject("Title");
        titleGo.transform.SetParent(titleBar.transform, false);
        var titleRt = titleGo.AddComponent<RectTransform>();
        titleRt.anchorMin = Vector2.zero;
        titleRt.anchorMax = Vector2.one;
        titleRt.offsetMin = Vector2.zero;
        titleRt.offsetMax = Vector2.zero;
        titleText = titleGo.AddComponent<Text>();
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 30;
        titleText.fontStyle = FontStyle.Bold;
        titleText.color = new Color(1f, 0.92f, 0.65f);
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.text = defaultTitle;
        titleText.raycastTarget = false;

        // Body area (centered between title bar and close button)
        var bodyGo = new GameObject("Body");
        bodyGo.transform.SetParent(panel.transform, false);
        var bodyRt = bodyGo.AddComponent<RectTransform>();
        bodyRt.anchorMin = new Vector2(0, 0);
        bodyRt.anchorMax = new Vector2(1, 1);
        bodyRt.offsetMin = new Vector2(20, 90);    // bottom inset: above close button
        bodyRt.offsetMax = new Vector2(-20, -TITLE_BAR_HEIGHT - 20);  // top inset: below title bar
        // Body parchment (so text shows on light background, not dark overlay)
        var bodyBgImg = bodyGo.AddComponent<Image>();
        bodyBgImg.sprite = UIStyleHelper.MakeParchmentSprite(128, 128);
        bodyBgImg.type = Image.Type.Sliced;
        bodyBgImg.color = Color.white;
        bodyBgImg.raycastTarget = false;
        // Body text (child of body so it auto-positions within parchment)
        var bodyTextGo = new GameObject("BodyText");
        bodyTextGo.transform.SetParent(bodyGo.transform, false);
        var bodyTextRt = bodyTextGo.AddComponent<RectTransform>();
        bodyTextRt.anchorMin = Vector2.zero;
        bodyTextRt.anchorMax = Vector2.one;
        bodyTextRt.offsetMin = new Vector2(10, 10);
        bodyTextRt.offsetMax = new Vector2(-10, -10);
        bodyText = bodyTextGo.AddComponent<Text>();
        bodyText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        bodyText.fontSize = 16;
        bodyText.color = new Color(0.25f, 0.12f, 0.03f);
        bodyText.alignment = TextAnchor.UpperCenter;
        bodyText.text = defaultBody;
        bodyText.raycastTarget = false;

        // Close button (centered at bottom)
        var closeGo = new GameObject("CloseButton");
        closeGo.transform.SetParent(panel.transform, false);
        var closeRt = closeGo.AddComponent<RectTransform>();
        closeRt.anchorMin = new Vector2(0.5f, 0);
        closeRt.anchorMax = new Vector2(0.5f, 0);
        closeRt.pivot = new Vector2(0.5f, 0);
        closeRt.anchoredPosition = new Vector2(0, 24);
        closeRt.sizeDelta = new Vector2(220, 44);
        var closeImg = closeGo.AddComponent<Image>();
        closeImg.color = new Color(0.55f, 0.10f, 0.08f);
        closeImg.raycastTarget = true;
        var closeBtn = closeGo.AddComponent<Button>();
        closeBtn.onClick.AddListener(Hide);
        // Close button text
        var closeTextGo = new GameObject("CloseLabel");
        closeTextGo.transform.SetParent(closeGo.transform, false);
        var closeTextRt = closeTextGo.AddComponent<RectTransform>();
        closeTextRt.anchorMin = Vector2.zero;
        closeTextRt.anchorMax = Vector2.one;
        closeTextRt.offsetMin = Vector2.zero;
        closeTextRt.offsetMax = Vector2.zero;
        var closeText = closeTextGo.AddComponent<Text>();
        closeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeText.fontSize = 20;
        closeText.fontStyle = FontStyle.Bold;
        closeText.color = new Color(1f, 0.95f, 0.65f);
        closeText.alignment = TextAnchor.MiddleCenter;
        closeText.text = "CLOSE";
        closeText.raycastTarget = false;

        // Hint text (between close button and body)
        var hintGo = new GameObject("Hint");
        hintGo.transform.SetParent(panel.transform, false);
        var hintRt = hintGo.AddComponent<RectTransform>();
        hintRt.anchorMin = new Vector2(0.5f, 0);
        hintRt.anchorMax = new Vector2(0.5f, 0);
        hintRt.pivot = new Vector2(0.5f, 0);
        hintRt.anchoredPosition = new Vector2(0, 80);
        hintRt.sizeDelta = new Vector2(440, 18);
        var hintText = hintGo.AddComponent<Text>();
        hintText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        hintText.fontSize = 13;
        hintText.fontStyle = FontStyle.Bold;
        hintText.color = new Color(0.50f, 0.28f, 0.08f);
        hintText.alignment = TextAnchor.MiddleCenter;
        hintText.text = "[TAB]  [ESC]  [X]  —  Close";
        hintText.raycastTarget = false;
    }

    void CreateTrim(Transform parent, float yOffset, bool top)
    {
        var trim = new GameObject("Trim");
        trim.transform.SetParent(parent, false);
        var tRt = trim.AddComponent<RectTransform>();
        tRt.anchorMin = new Vector2(0, 1);
        tRt.anchorMax = new Vector2(1, 1);
        tRt.pivot = new Vector2(0.5f, 1);
        tRt.anchoredPosition = new Vector2(0, top ? 0 : yOffset);
        tRt.sizeDelta = new Vector2(0, 3);
        var tImg = trim.AddComponent<Image>();
        tImg.color = new Color(0.95f, 0.78f, 0.30f);
        tImg.raycastTarget = false;
    }

    public virtual void ShowPopup(string title, string body)
    {
        if (overlay != null) overlay.SetActive(true);
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
    }

    public virtual void HidePopup()
    {
        if (overlay != null) overlay.SetActive(false);
    }
}
