using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPopup : MonoBehaviour
{
    static BuildingPopup instance;

    Canvas canvas;
    GameObject overlay;
    GameObject panel;
    Text titleText;
    Text bodyText;

    public static bool IsVisible => instance != null && instance.overlay != null && instance.overlay.activeSelf;

    public static void Show(string title, string body, string iconType = "default", Vector3 worldPos = default)
    {
        Debug.Log($"BuildingPopup.Show: title='{title}' worldPos={worldPos}");
        if (instance == null)
        {
            var go = new GameObject("BuildingPopup");
            instance = go.AddComponent<BuildingPopup>();
        }
        if (instance == null)
        {
            Debug.LogError("BuildingPopup: failed to create instance!");
            return;
        }
        instance.ShowPopup(title, body, iconType, worldPos);
    }

    public static void Hide()
    {
        if (instance != null) instance.DoHide();
    }

    void Awake()
    {
        Debug.Log($"BuildingPopup.Awake: looking for canvas...");
        canvas = FindAnyObjectByType<Canvas>();
        Debug.Log($"BuildingPopup.Awake: canvas found = {canvas != null}");
        if (canvas == null) return;
        BuildUI();
    }

    void BuildUI()
    {
        // Dark backdrop — covers the full screen and handles clicks for dismiss
        overlay = new GameObject("PopupOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var oRt = overlay.AddComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.sizeDelta = Vector2.zero;
        var oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.5f);
        oImg.raycastTarget = true;  // intercept clicks
        overlay.SetActive(false);

        // Attach a click handler on the overlay itself
        var trigger = overlay.AddComponent<EventTrigger>();
        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(_ => DoHide());
        trigger.triggers.Add(entry);

        // Create panel first (it will be the parent for border + content)
        panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(540, 300);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = UIStyleHelper.MakeParchmentSprite(256, 256);
        pImg.type = Image.Type.Sliced;
        pImg.color = Color.white;
        pImg.raycastTarget = false;

        // Border is a child of panel so they move together
        var border = new GameObject("Border");
        border.transform.SetParent(panel.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = Vector2.zero;
        bRt.anchorMax = Vector2.one;
        bRt.offsetMin = new Vector2(-10, -10);
        bRt.offsetMax = new Vector2(10, 10);
        var bImg = border.AddComponent<Image>();
        bImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 14, 18);
        bImg.type = Image.Type.Sliced;
        bImg.color = Color.white;
        bImg.raycastTarget = false;

        // Title bar
        var titleBar = new GameObject("TitleBar");
        titleBar.transform.SetParent(panel.transform, false);
        var tbRt = titleBar.AddComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = Vector2.zero;
        tbRt.sizeDelta = new Vector2(0, -64);
        var tbImg = titleBar.AddComponent<Image>();
        tbImg.color = new Color(0.55f, 0.10f, 0.08f);
        tbImg.raycastTarget = false;

        AddTrim(panel, -64, true);
        AddTrim(panel, -64, false);

        // Title
        titleText = MakeText(panel, "Title", new Vector2(0, -8), new Vector2(460, 50),
            "Building Complete", 36, FontStyle.Bold,
            new Color(1f, 0.95f, 0.70f), TextAnchor.MiddleCenter);

        // Body — keep inside the panel
        bodyText = MakeText(panel, "Body", new Vector2(0, -80), new Vector2(460, 200),
            "", 18, FontStyle.Bold,
            new Color(0.35f, 0.20f, 0.05f), TextAnchor.UpperCenter);

        // Close button (mobile-friendly)
        var closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(panel.transform, false);
        var cbRt = closeBtn.AddComponent<RectTransform>();
        cbRt.anchorMin = new Vector2(1, 1);
        cbRt.anchorMax = new Vector2(1, 1);
        cbRt.pivot = new Vector2(1, 1);
        cbRt.anchoredPosition = new Vector2(-8, -8);
        cbRt.sizeDelta = new Vector2(72, 40);
        var cbImg = closeBtn.AddComponent<Image>();
        cbImg.color = new Color(0.55f, 0.10f, 0.08f);
        cbImg.raycastTarget = true;
        var cbBtn = closeBtn.AddComponent<Button>();
        cbBtn.onClick.AddListener(DoHide);

        var cbText = MakeText(closeBtn, "CloseLabel", Vector2.zero, new Vector2(72, 40),
            "CLOSE", 18, FontStyle.Bold,
            new Color(1f, 0.95f, 0.65f), TextAnchor.MiddleCenter);

        // Hint
        var hint = MakeText(panel, "Hint", new Vector2(0, -270), new Vector2(440, 24),
            "[TAB] [ESC] [X] — Close", 15, FontStyle.Bold,
            new Color(0.55f, 0.32f, 0.10f), TextAnchor.MiddleCenter);
    }

    void AddTrim(GameObject parent, float yOffset, bool top)
    {
        var trim = new GameObject("Trim");
        trim.transform.SetParent(parent.transform, false);
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

    Text MakeText(GameObject parent, string name, Vector2 pos, Vector2 size,
        string content, int fontSize, FontStyle style, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1);
        rt.anchorMax = new Vector2(0.5f, 1);
        rt.pivot = new Vector2(0.5f, 1);
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

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        // Allow Tab / Esc / X to close the popup
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            DoHide();
        // Also close on touch input (mobile)
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Defer to next frame so EventSystem gets the click first
            // The EventTrigger on overlay will handle the click
        }
    }

    public void ShowPopup(string title, string body, string iconType, Vector3 worldPos = default)
    {
        Debug.Log($"BuildingPopup.ShowPopup: title='{title}' canvas={(canvas != null ? "OK" : "NULL")} panel={(panel != null ? "OK" : "NULL")}");

        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("BuildingPopup: NO CANVAS FOUND! Cannot show popup.");
            return;
        }
        if (panel == null)
        {
            Debug.LogError("BuildingPopup: PANEL IS NULL! UI was not built.");
            return;
        }

        titleText.text = title;
        bodyText.text = body;

        // Position popup at the building's screen location (or center if no world pos)
        if (worldPos != default && panel != null)
        {
            Camera cam = Camera.main;
            if (cam != null)
            {
                Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
                // Add a small offset so the popup is above the building, not overlapping
                screenPos.y += 100f;

                RectTransform panelRt = panel.GetComponent<RectTransform>();
                RectTransform canvasRt = canvas.transform as RectTransform;
                Vector2 localPos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRt, screenPos, canvas.worldCamera, out localPos);

                // Clamp to keep panel visible on screen
                float halfW = panelRt.sizeDelta.x * 0.5f;
                float halfH = panelRt.sizeDelta.y * 0.5f;
                float maxX = canvasRt.rect.width * 0.5f - halfW - 16f;
                float maxY = canvasRt.rect.height * 0.5f - halfH - 16f;
                localPos.x = Mathf.Clamp(localPos.x, -maxX, maxX);
                localPos.y = Mathf.Clamp(localPos.y, -maxY, maxY);

                panelRt.anchoredPosition = localPos;
            }
        }

        overlay.SetActive(true);
    }

    public void DoHide()
    {
        if (overlay != null) overlay.SetActive(false);
    }
}
