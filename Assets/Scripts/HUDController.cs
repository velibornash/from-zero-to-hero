using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

// HUD icons from game-icons.net under CC BY 3.0:
// gold-nuggets by Delapouite (gold), log by Delapouite (wood), wheat by Lorc (food)

public class HUDController : MonoBehaviour
{
    public static int Gold = 50;
    public static int Wood = 0;
    public static int Food = 0;
    public static int Day = 1;

    Text goldText, woodText, foodText, dayText, eventText;
    List<string> events = new List<string>();
    static HUDController instance;

    Sprite goldSprite, woodSprite, foodSprite;

    void Awake() { instance = this; }

    void Start()
    {
        goldSprite = LoadIcon("HUDIcons/gold_icon");
        woodSprite = LoadIcon("HUDIcons/wood_icon");
        foodSprite = LoadIcon("HUDIcons/wheat_icon");
        BuildHUD();
        PushEvent("Welcome to From Zero To Hero");
    }

    Sprite LoadIcon(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return null;
    }

    void BuildHUD()
    {
        var topBar = new GameObject("TopBar");
        topBar.transform.SetParent(transform, false);
        var barRt = topBar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0, 1);
        barRt.anchorMax = new Vector2(1, 1);
        barRt.pivot = new Vector2(0.5f, 1);
        barRt.offsetMin = new Vector2(0, -68);
        barRt.offsetMax = new Vector2(0, 0);
        var barImg = topBar.AddComponent<Image>();
        barImg.color = new Color(0.015f, 0.015f, 0.04f, 0.9f);
        barImg.raycastTarget = false;

        // Separator line under bar
        var sep = new GameObject("BarSeparator");
        sep.transform.SetParent(topBar.transform, false);
        var sepRt = sep.AddComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0, 0);
        sepRt.anchorMax = new Vector2(1, 0);
        sepRt.pivot = new Vector2(0.5f, 0);
        sepRt.offsetMin = new Vector2(0, -2);
        sepRt.offsetMax = new Vector2(0, 0);
        var sepImg = sep.AddComponent<Image>();
        sepImg.color = new Color(0.6f, 0.5f, 0.2f, 0.6f);
        sepImg.raycastTarget = false;

        float x = 12f;
        float iconSize = 30f;
        float labelY = -14f;

        // Gold
        BuildIcon(topBar.transform, ref x, goldSprite, iconSize, labelY,
            new Color(0.35f, 0.22f, 0.05f), new Color(1f, 0.84f, 0f));
        goldText = BuildValue(topBar.transform, ref x, "50",
            new Color(1f, 0.9f, 0.2f), 26);
        x += 18f;

        // Wood
        BuildIcon(topBar.transform, ref x, woodSprite, iconSize, labelY,
            new Color(0.2f, 0.12f, 0.03f), new Color(0.65f, 0.45f, 0.15f));
        woodText = BuildValue(topBar.transform, ref x, "0",
            new Color(0.85f, 0.75f, 0.55f), 22);
        x += 18f;

        // Food
        BuildIcon(topBar.transform, ref x, foodSprite, iconSize, labelY,
            new Color(0.15f, 0.15f, 0.05f), new Color(1f, 0.85f, 0.2f));
        foodText = BuildValue(topBar.transform, ref x, "0",
            new Color(1f, 0.85f, 0.3f), 22);
        x += 18f;

        // Day (right)
        dayText = MakeText(topBar.transform, "", new Vector2(-16, -8), new Vector2(130, 40),
            new Color(0.7f, 0.8f, 0.95f), 20, FontStyle.Bold, TextAnchor.MiddleRight);
        dayText.GetComponent<RectTransform>().anchorMin = new Vector2(1, 0);
        dayText.GetComponent<RectTransform>().anchorMax = new Vector2(1, 1);
        dayText.GetComponent<RectTransform>().pivot = new Vector2(1, 0.5f);

        // Event panel
        var evPanel = new GameObject("EventPanel");
        evPanel.transform.SetParent(transform, false);
        var evRt = evPanel.AddComponent<RectTransform>();
        evRt.anchorMin = new Vector2(1, 0.5f);
        evRt.anchorMax = new Vector2(1, 0.5f);
        evRt.pivot = new Vector2(1, 0.5f);
        evRt.anchoredPosition = new Vector2(-8, 0);
        evRt.sizeDelta = new Vector2(180, 240);
        var evImg = evPanel.AddComponent<Image>();
        evImg.color = new Color(0.015f, 0.015f, 0.04f, 0.72f);
        evImg.raycastTarget = false;

        var evSep = new GameObject("EventSep");
        evSep.transform.SetParent(evPanel.transform, false);
        var esRt = evSep.AddComponent<RectTransform>();
        esRt.anchorMin = new Vector2(0, 1);
        esRt.anchorMax = new Vector2(1, 1);
        esRt.pivot = new Vector2(0.5f, 1);
        esRt.anchoredPosition = new Vector2(0, -26);
        esRt.sizeDelta = new Vector2(-8, -2);
        var esImg = evSep.AddComponent<Image>();
        esImg.color = new Color(0.6f, 0.5f, 0.2f, 0.4f);
        esImg.raycastTarget = false;

        MakeText(evPanel.transform, "Events", new Vector2(10, -8), new Vector2(160, 18),
            new Color(0.5f, 0.6f, 0.75f), 13, FontStyle.Bold, TextAnchor.UpperLeft);

        eventText = MakeText(evPanel.transform, "", new Vector2(10, -34), new Vector2(160, 200),
            new Color(0.7f, 0.8f, 0.9f), 12, FontStyle.Normal, TextAnchor.UpperLeft);
    }

    void BuildIcon(Transform parent, ref float x, Sprite sprite, float size, float y,
        Color bgColor, Color tint)
    {
        var bg = new GameObject("IconBg");
        bg.transform.SetParent(parent, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = new Vector2(0, 1);
        bgRt.anchorMax = new Vector2(0, 1);
        bgRt.pivot = new Vector2(0, 1);
        bgRt.anchoredPosition = new Vector2(x, y);
        bgRt.sizeDelta = new Vector2(size, size);
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = bgColor;
        bgImg.raycastTarget = false;

        if (sprite != null)
        {
            var sym = new GameObject("IconSprite");
            sym.transform.SetParent(bg.transform, false);
            var sRt = sym.AddComponent<RectTransform>();
            sRt.anchorMin = Vector2.zero;
            sRt.anchorMax = Vector2.one;
            sRt.sizeDelta = Vector2.zero;
            var sImg = sym.AddComponent<Image>();
            sImg.sprite = sprite;
            sImg.color = tint;
            sImg.raycastTarget = false;
        }

        x += size + 6;
    }

    Text BuildValue(Transform parent, ref float x, string initialValue, Color color, int fontSize)
    {
        var val = MakeText(parent, initialValue, new Vector2(x, -10), new Vector2(80, 36),
            color, fontSize, FontStyle.Bold, TextAnchor.MiddleLeft);
        x += 80;
        return val;
    }

    Text MakeText(Transform parent, string content, Vector2 pos, Vector2 size, Color color,
        int fontSize, FontStyle fontStyle, TextAnchor anchor)
    {
        var go = new GameObject("Text");
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
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        return text;
    }

    void Update()
    {
        if (goldText != null) goldText.text = $"{Gold}";
        if (woodText != null) woodText.text = $"{Wood}";
        if (foodText != null) foodText.text = $"{Food}";
        if (dayText != null) dayText.text = $"\u2600 Day {Day}";
    }

    public static void PushEvent(string msg)
    {
        if (instance == null) return;
        instance.events.Insert(0, msg);
        if (instance.events.Count > 5)
            instance.events.RemoveAt(instance.events.Count - 1);
        if (instance.eventText != null)
            instance.eventText.text = string.Join("\n\n", instance.events);
    }
}
