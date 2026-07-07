using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ActionMenu : PopupBase
{
    static ActionMenu instance;
    static float lastCloseTime;

    static Sprite goldSprite, woodSprite, foodSprite, stoneSprite;

    Image healthFill;
    Text healthLabel;
    Text goldVal, woodVal, stoneVal, foodVal;

    public static bool IsVisible => instance != null && instance.overlay != null && instance.overlay.activeSelf;

    static void LoadIcons()
    {
        if (goldSprite != null) return;
        goldSprite = LoadIcon("HUDIcons/gold_icon");
        woodSprite = LoadIcon("HUDIcons/wood_icon");
        foodSprite = LoadIcon("HUDIcons/wheat_icon");
        stoneSprite = LoadIcon("HUDIcons/stone_icon");
    }

    static Sprite LoadIcon(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
        {
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

    Text MakeStatText(Transform parent, string name, Vector2 pos, Vector2 size,
        int fontSize, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = FontStyle.Bold;
        text.color = color;
        text.alignment = anchor;
        text.text = "";
        text.raycastTarget = false;
        return text;
    }

    Image MakeStatIcon(Transform parent, Sprite sprite, Vector2 pos, float size)
    {
        var go = new GameObject("Icon");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = pos;
        rt.sizeDelta = new Vector2(size, size);
        var img = go.AddComponent<Image>();
        img.sprite = sprite;
        img.color = Color.white;
        img.raycastTarget = false;
        return img;
    }

    void BuildStatsUI()
    {
        if (panel == null) return;
        LoadIcons();

        // Stats container — sits inside panel, below title bar
        var statsGo = new GameObject("StatsBar");
        statsGo.transform.SetParent(panel.transform, false);
        var sRt = statsGo.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0, 1);
        sRt.anchorMax = new Vector2(1, 1);
        sRt.pivot = new Vector2(0.5f, 1);
        sRt.anchoredPosition = new Vector2(0, -TITLE_BAR_HEIGHT - 8);
        sRt.sizeDelta = new Vector2(-20, 100);

        // Health bar
        var hbGo = new GameObject("HPBar");
        hbGo.transform.SetParent(statsGo.transform, false);
        var hbRt = hbGo.AddComponent<RectTransform>();
        hbRt.anchorMin = new Vector2(0.5f, 1);
        hbRt.anchorMax = new Vector2(0.5f, 1);
        hbRt.pivot = new Vector2(0.5f, 1);
        hbRt.anchoredPosition = new Vector2(0, -8);
        hbRt.sizeDelta = new Vector2(240, 24);

        var hbBg = hbGo.AddComponent<Image>();
        hbBg.color = new Color(0.15f, 0.05f, 0.05f);
        hbBg.raycastTarget = false;

        var hbFillGo = new GameObject("Fill");
        hbFillGo.transform.SetParent(hbGo.transform, false);
        var hbFillRt = hbFillGo.AddComponent<RectTransform>();
        hbFillRt.anchorMin = Vector2.zero;
        hbFillRt.anchorMax = Vector2.one;
        hbFillRt.offsetMin = Vector2.zero;
        hbFillRt.offsetMax = Vector2.zero;
        healthFill = hbFillGo.AddComponent<Image>();
        healthFill.color = new Color(0.3f, 0.85f, 0.3f);
        healthFill.raycastTarget = false;
        healthFill.type = Image.Type.Filled;
        healthFill.fillMethod = Image.FillMethod.Horizontal;
        healthFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthFill.fillAmount = 1f;

        healthLabel = MakeStatText(hbGo.transform, "Label",
            new Vector2(0, 0), new Vector2(240, 24),
            14, new Color(1f, 0.95f, 0.55f), TextAnchor.MiddleCenter);

        // Resources row
        float rx = 30f;
        const float iconS = 40f;

        MakeStatIcon(statsGo.transform, goldSprite, new Vector2(rx, -46), iconS);
        rx += iconS + 4;
        goldVal = MakeStatText(statsGo.transform, "GoldVal",
            new Vector2(rx, -50), new Vector2(80, 32),
            22, new Color(1f, 0.95f, 0.40f), TextAnchor.MiddleLeft);
        rx += 80 + 8;

        MakeStatIcon(statsGo.transform, woodSprite, new Vector2(rx, -46), iconS);
        rx += iconS + 4;
        woodVal = MakeStatText(statsGo.transform, "WoodVal",
            new Vector2(rx, -50), new Vector2(80, 32),
            22, new Color(0.95f, 0.82f, 0.50f), TextAnchor.MiddleLeft);
        rx += 80 + 8;

        MakeStatIcon(statsGo.transform, stoneSprite, new Vector2(rx, -46), iconS);
        rx += iconS + 4;
        stoneVal = MakeStatText(statsGo.transform, "StoneVal",
            new Vector2(rx, -50), new Vector2(80, 32),
            22, new Color(0.75f, 0.75f, 0.75f), TextAnchor.MiddleLeft);
        rx += 80 + 8;

        MakeStatIcon(statsGo.transform, foodSprite, new Vector2(rx, -46), iconS);
        rx += iconS + 4;
        foodVal = MakeStatText(statsGo.transform, "FoodVal",
            new Vector2(rx, -50), new Vector2(80, 32),
            22, new Color(1f, 0.90f, 0.35f), TextAnchor.MiddleLeft);

        // Move body text down to make room
        var bodyGo = panel.transform.Find("Body");
        if (bodyGo != null)
        {
            var bodyRt = bodyGo.GetComponent<RectTransform>();
            if (bodyRt != null)
            {
                bodyRt.offsetMin = new Vector2(28, 90);
                bodyRt.offsetMax = new Vector2(-28, -TITLE_BAR_HEIGHT - 108);
            }
        }
    }

    void UpdateStats()
    {
        if (healthFill != null)
        {
            int hp = PlayerController3D.Health;
            int maxHp = PlayerController3D.maxHealth;
            float pct = (float)hp / Mathf.Max(1, maxHp);
            healthFill.fillAmount = pct;
            if (pct > 0.6f) healthFill.color = new Color(0.3f, 0.85f, 0.3f, 1f);
            else if (pct > 0.3f) healthFill.color = new Color(0.95f, 0.7f, 0.2f, 1f);
            else healthFill.color = new Color(0.9f, 0.25f, 0.2f, 1f);
            if (healthLabel != null) healthLabel.text = $"HP {hp}/{maxHp}";
        }
        if (goldVal != null) goldVal.text = $"{HUDController.Gold}";
        if (woodVal != null) woodVal.text = $"{HUDController.Wood}";
        if (stoneVal != null) stoneVal.text = $"{HUDController.Stone}";
        if (foodVal != null) foodVal.text = $"{HUDController.Food}";
    }

    string BuildObjectiveText()
    {
        var slots = Object.FindObjectsByType<BuildSlot>(FindObjectsInactive.Include);
        string next = "";
        int built = 0, total = 0;
        foreach (var s in slots)
        {
            if (s.data == null) continue;
            total++;
            if (s.IsBuilt) { built++; continue; }
            if (s.state == BuildSlot.State.Available && string.IsNullOrEmpty(next))
                next = $"{s.data.slotName} — {s.data.cost} gold";
        }
        string prog = $"Buildings: {built}/{total}";
        string obj = string.IsNullOrEmpty(next) ? "All buildings complete!" : $"Next: {next}";
        return $"{obj}\n{prog}";
    }

    public static void Toggle()
    {
        if (Time.time - lastCloseTime < 0.3f) return;
        if (IsVisible) { Hide(); return; }
        if (BuildingPopup.IsVisible) return;

        var canvas = FindMainCanvas();
        if (canvas == null) return;

        if (instance == null || instance.overlay == null)
        {
            if (instance != null) Object.DestroyImmediate(instance.gameObject);
            var go = new GameObject("ActionMenu");
            instance = go.AddComponent<ActionMenu>();
            instance.BuildUI("GAME ACTIONS", "", canvas);
            instance.BuildStatsUI();
        }

        instance.UpdateStats();
        instance.bodyText.text = instance.BuildObjectiveText() + "\n\nControls:\n[WASD]  Move\n[TAB]  Toggle\n[ESC] [X]  Close";

        instance.bodyText.fontSize = 18;
        instance.bodyText.fontStyle = FontStyle.Bold;
        instance.bodyText.resizeTextForBestFit = false;
        instance.bodyText.resizeTextMinSize = 12;
        instance.bodyText.resizeTextMaxSize = 22;
        instance.bodyText.color = new Color(1f, 0.92f, 0.55f);
        instance.bodyText.lineSpacing = 1.3f;
        instance.bodyText.alignment = TextAnchor.UpperCenter;

        if (instance.panel != null)
        {
            var pRt = instance.panel.GetComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT + 80);
        }

        instance.ShowPopup("GAME ACTIONS", instance.bodyText.text);
    }

    static string buildProgress
    {
        get
        {
            var slots = Object.FindObjectsByType<BuildSlot>(FindObjectsInactive.Include);
            int built = 0, total = 0;
            foreach (var s in slots)
            {
                if (s.data == null) continue;
                total++;
                if (s.IsBuilt) built++;
            }
            return $"Buildings: {built}/{total}";
        }
    }

    public static void Hide()
    {
        if (instance != null) { lastCloseTime = Time.time; instance.HidePopup(); }
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            lastCloseTime = Time.time;
            HidePopup();
        }
    }

    static Canvas FindMainCanvas()
    {
        var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in all)
            if (c.name == "Canvas" || c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        return all.Length > 0 ? all[0] : null;
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }
}
