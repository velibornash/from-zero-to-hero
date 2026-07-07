using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HUDController : MonoBehaviour
{
    public static int Gold = 10;
    public static int Wood = 0;
    public static int Food = 0;
    public static int Stone = 0;

    public static void AddResource(string type, int amount)
    {
        switch (type)
        {
            case "gold": Gold += amount; break;
            case "wood": Wood += amount; break;
            case "food": Food += amount; break;
            case "stone": Stone += amount; break;
        }
    }
    public static int Day = 1;
    public const string ChapterName = "Chapter I: The Awakening";
    static bool hasWon;

    Text goldText, woodText, foodText, stoneText, dayText, eventText;
    List<string> events = new List<string>();
    List<float> eventTimestamps = new List<float>();
    const float EVENT_LIFETIME = 8f;
    static HUDController instance;

    Sprite goldSprite, woodSprite, foodSprite, stoneSprite, ribbonBgSprite;

    void Awake() { instance = this; }

    void Start()
    {
        Debug.Log("HUDController.Start()");
        ResetState();
        goldSprite = LoadIcon("HUDIcons/gold_icon");
        woodSprite = LoadIcon("HUDIcons/wood_icon");
        foodSprite = LoadIcon("HUDIcons/wheat_icon");
        stoneSprite = LoadIcon("HUDIcons/stone_icon");
        var ribbonTex = Resources.Load<Texture2D>("HUDIcons/ribbon_bg");
        if (ribbonTex != null)
        {
            ribbonTex.filterMode = FilterMode.Bilinear;
            ribbonBgSprite = Sprite.Create(ribbonTex, new Rect(0, 0, ribbonTex.width, ribbonTex.height), new Vector2(0.5f, 0.5f));
        }
        BuildHUD();
        PushEvent("Welcome to From Zero To Hero");
    }

    Sprite LoadIcon(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
        {
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }
        return null;
    }

    static Font GetFont()
    {
        foreach (var n in new[] { "Arial", "Helvetica", "Liberation Sans", "Segoe UI", "Ubuntu" })
        {
            var f = Font.CreateDynamicFontFromOSFont(n, 16);
            if (f != null) return f;
        }
        var ff = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (ff == null) ff = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return ff;
    }

    Text MakeText(Transform parent, string name, Vector2 anchoredPos, Vector2 size,
        string content, int fontSize, FontStyle fontStyle, Color color, TextAnchor anchor)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;
        var text = go.AddComponent<Text>();
        text.font = GetFont();
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        text.raycastTarget = false;
        return text;
    }

    void BuildHUD()
    {
        bool mob = PopupBase.IsMobile;
        float iconSize = mob ? 57f : 76f;
        float labelY = mob ? -85f : -100f;
        float valW = mob ? 58f : 86f;
        int valFont = mob ? 27 : 38;
        int fontSize = mob ? 19 : 30;
        int reportsFont = mob ? 17 : 23;

        // Ribbon
        float ribbonH = mob ? 140f : 200f;
        float ribbonW = mob ? 0.88f : 0.78f;
        float margin = (1f - ribbonW) * 0.5f;

        var topBar = new GameObject("TopBar");
        topBar.transform.SetParent(transform, false);
        var barRt = topBar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(margin, 1);
        barRt.anchorMax = new Vector2(1f - margin, 1);
        barRt.pivot = new Vector2(0.5f, 1);
        barRt.offsetMin = new Vector2(0, -ribbonH);
        barRt.offsetMax = new Vector2(0, 0);

        var barImg = topBar.AddComponent<Image>();
        if (ribbonBgSprite != null)
        {
            barImg.sprite = ribbonBgSprite;
            barImg.type = Image.Type.Simple;
            barImg.color = Color.white;
            barImg.preserveAspect = false;
        }
        else
        {
            barImg.color = new Color(0.38f, 0.06f, 0.04f);
        }
        barImg.raycastTarget = true;

        // Calculate total content width for centering
        float gap = mob ? 8f : 14f;
        float sepW = 2f;
        float chapterW = mob ? 290f : 400f;
        float totalW = (iconSize + valW + gap) * 4f + gap + sepW + gap + chapterW;
        float startX = (1920f - totalW) * 0.5f - (mob ? 90f : 150f);
        float x = startX;

        goldText = BuildResourceSlot(topBar.transform, ref x, goldSprite, "10", iconSize, labelY,
            valW, valFont, new Color(1f, 0.95f, 0.40f));
        x += gap;
        woodText = BuildResourceSlot(topBar.transform, ref x, woodSprite, "0", iconSize, labelY,
            valW, valFont, new Color(0.95f, 0.82f, 0.50f));
        x += gap;
        stoneText = BuildResourceSlot(topBar.transform, ref x, stoneSprite, "0", iconSize, labelY,
            valW, valFont, new Color(0.75f, 0.75f, 0.75f));
        x += gap;
        foodText = BuildResourceSlot(topBar.transform, ref x, foodSprite, "0", iconSize, labelY,
            valW, valFont, new Color(1f, 0.90f, 0.35f));
        x += gap;

        MakeVerticalSeparator(topBar.transform, x);
        x += gap;

        dayText = MakeText(topBar.transform, "Chapter", new Vector2(x, labelY), new Vector2(chapterW, 36),
            ChapterName, fontSize, FontStyle.Bold,
            new Color(1f, 0.95f, 0.55f), TextAnchor.MiddleLeft);
        dayText.resizeTextForBestFit = true;
        dayText.resizeTextMinSize = 10;
        dayText.resizeTextMaxSize = fontSize;

        // Reports panel (top right)
        float rw = mob ? 220f : 350f;
        float rh = mob ? 200f : 300f;
        var evPanel = UIStyleHelper.MakeOrnatePanel(transform, (int)rw, (int)rh);
        var evRt = evPanel.GetComponent<RectTransform>();
        evRt.anchorMin = new Vector2(1, 1);
        evRt.anchorMax = new Vector2(1, 1);
        evRt.pivot = new Vector2(1, 1);
        evRt.anchoredPosition = new Vector2(-18, -(ribbonH + 16));
        evRt.sizeDelta = new Vector2(rw, rh);

        float leftMargin = mob ? 32f : 50f;
        var hdr = MakeText(evPanel.transform, "Header", new Vector2(leftMargin, -28), new Vector2(rw - leftMargin * 2, 30),
            "REPORTS", mob ? 22 : 30, FontStyle.Bold,
            new Color(0.95f, 0.75f, 0.20f), TextAnchor.MiddleLeft);
        hdr.resizeTextForBestFit = true;
        hdr.resizeTextMinSize = 12;
        hdr.resizeTextMaxSize = mob ? 28 : 36;

        eventText = MakeText(evPanel.transform, "Events", new Vector2(leftMargin, -66), new Vector2(rw - leftMargin * 2, rh - 88),
            "", reportsFont, FontStyle.Bold,
            new Color(1f, 0.90f, 0.60f), TextAnchor.UpperLeft);
    }



    Text BuildResourceSlot(Transform parent, ref float x, Sprite sprite, string initialValue,
        float iconSize, float labelY, float valW, int valFont, Color valueColor)
    {
        var iconObj = new GameObject("Icon");
        iconObj.transform.SetParent(parent, false);
        var iconRt = iconObj.AddComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 1);
        iconRt.anchorMax = new Vector2(0, 1);
        iconRt.pivot = new Vector2(0, 1);
        iconRt.anchoredPosition = new Vector2(x, labelY + 2f);
        iconRt.sizeDelta = new Vector2(iconSize, iconSize);
        var iconImg = iconObj.AddComponent<Image>();
        iconImg.sprite = sprite;
        iconImg.color = Color.white;
        iconImg.raycastTarget = false;
        x += iconSize;

        var text = MakeText(parent, "Value_" + initialValue, new Vector2(x, labelY - 2f), new Vector2(valW, 36),
            initialValue, valFont, FontStyle.Bold, valueColor, TextAnchor.MiddleLeft);
        x += valW;
        return text;
    }

    void MakeVerticalSeparator(Transform parent, float x)
    {
        bool mob = PopupBase.IsMobile;
        var sep = new GameObject("Sep");
        sep.transform.SetParent(parent, false);
        var rt = sep.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, mob ? -22 : -38);
        rt.sizeDelta = new Vector2(2, mob ? 75 : 100);
        var img = sep.AddComponent<Image>();
        img.color = new Color(0.55f, 0.40f, 0.18f, 0.7f);
        img.raycastTarget = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ActionMenu.Toggle();
        if (Input.GetKeyDown(KeyCode.F1)) PopupBase.ToggleMobile();

        if (goldText != null) goldText.text = $"{Gold}";
        if (woodText != null) woodText.text = $"{Wood}";
        if (stoneText != null) stoneText.text = $"{Stone}";
        if (foodText != null) foodText.text = $"{Food}";
        if (dayText != null) dayText.text = ChapterName;



        // Win condition check
        if (!hasWon && Gold >= 300)
        {
            var slots = FindObjectsByType<BuildSlot>(FindObjectsInactive.Include);
            int[] required = { 0, 1, 2, 3, 4, 5 };
            if (required.All(i => slots.Any(s => s.slotIndex == i && s.IsBuilt)))
            {
                hasWon = true;
                BuildingPopup.Show("VICTORY — Chapter I Complete",
                    "The valley is yours.\n\n" +
                    "The church stands tall against the sky. The Serbian\n" +
                    "banner flies proud over every roof. Four watchtowers\n" +
                    "guard the corners of your domain.\n\n" +
                    "With 300 gold in the treasury, the people prosper.\n" +
                    "Wolves retreat into the deep woods. Barbarians\n" +
                    "flee beyond the hills.\n\n" +
                    "\"From zero to hero\" — the old saying lives.\n\n" +
                    "But beyond the mountains, a greater threat stirs.\n" +
                    "The Gathering Storm approaches...\n\n" +
                    "— Chapter II awaits —",
                    "default", Vector3.zero, QuitGame);
            }
        }

        // Auto-fade events after EVENT_LIFETIME seconds
        if (events.Count > 0)
        {
            bool changed = false;
            while (events.Count > 0 && Time.time - eventTimestamps[eventTimestamps.Count - 1] > EVENT_LIFETIME)
            {
                events.RemoveAt(events.Count - 1);
                eventTimestamps.RemoveAt(eventTimestamps.Count - 1);
                changed = true;
            }
            if (changed && eventText != null)
                eventText.text = string.Join("\n\n", events);
        }
    }

    public static void PushEvent(string msg)
    {
        if (instance == null) return;
        instance.events.Insert(0, msg);
        instance.eventTimestamps.Insert(0, Time.time);
        if (instance.events.Count > 5)
        {
            instance.events.RemoveAt(instance.events.Count - 1);
            instance.eventTimestamps.RemoveAt(instance.eventTimestamps.Count - 1);
        }
        if (instance.eventText != null)
            instance.eventText.text = string.Join("\n\n", instance.events);
    }

    public static void ResetState()
    {
        Gold = 10;
        Wood = 0;
        Stone = 0;
        Food = 0;
        Day = 1;
        hasWon = false;
    }

    static void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
