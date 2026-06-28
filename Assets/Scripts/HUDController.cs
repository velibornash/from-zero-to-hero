using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class HUDController : MonoBehaviour
{
    public static int Gold = 10;
    public static int Wood = 0;
    public static int Food = 0;
    public static int Day = 1;
    public const string ChapterName = "Chapter I: The Awakening";
    static bool hasWon;

    Text goldText, woodText, foodText, dayText, eventText;
    List<string> events = new List<string>();
    List<float> eventTimestamps = new List<float>();
    const float EVENT_LIFETIME = 8f;
    static HUDController instance;

    Sprite goldSprite, woodSprite, foodSprite;

    void Awake() { instance = this; }

    void Start()
    {
        Debug.Log("HUDController.Start()");
        ResetState();
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
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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
        // Top ribbon — Travian-style red banner with gold trim
        var topBar = new GameObject("TopBar");
        topBar.transform.SetParent(transform, false);
        var barRt = topBar.AddComponent<RectTransform>();
        barRt.anchorMin = new Vector2(0, 1);
        barRt.anchorMax = new Vector2(1, 1);
        barRt.pivot = new Vector2(0.5f, 1);
        barRt.offsetMin = new Vector2(0, -96);
        barRt.offsetMax = new Vector2(0, 0);

        var barImg = topBar.AddComponent<Image>();
        barImg.color = new Color(0.38f, 0.06f, 0.04f);
        barImg.raycastTarget = true;

        // Gold trim lines (top and bottom edges)
        for (int i = 0; i < 2; i++)
        {
            var trim = new GameObject(i == 0 ? "GoldTrimTop" : "GoldTrimBottom");
            trim.transform.SetParent(topBar.transform, false);
            var tRt = trim.AddComponent<RectTransform>();
            tRt.anchorMin = new Vector2(0, i == 0 ? 1 : 0);
            tRt.anchorMax = new Vector2(1, i == 0 ? 1 : 0);
            tRt.pivot = new Vector2(0.5f, i == 0 ? 1 : 0);
            tRt.anchoredPosition = Vector2.zero;
            tRt.sizeDelta = new Vector2(0, 4);
            var tImg = trim.AddComponent<Image>();
            tImg.color = new Color(0.90f, 0.70f, 0.18f);
            tImg.raycastTarget = false;
        }

        float x = 24f;
        const float iconSize = 60f;
        const float labelY = -18f;

        goldText = BuildResourceSlot(topBar.transform, ref x, goldSprite, "10", iconSize, labelY,
            new Color(1f, 0.95f, 0.40f));
        x += 12f;
        woodText = BuildResourceSlot(topBar.transform, ref x, woodSprite, "0", iconSize, labelY,
            new Color(0.95f, 0.82f, 0.50f));
        x += 12f;
        foodText = BuildResourceSlot(topBar.transform, ref x, foodSprite, "0", iconSize, labelY,
            new Color(1f, 0.90f, 0.35f));
        x += 24f;

        MakeVerticalSeparator(topBar.transform, x);
        x += 16f;

        dayText = MakeText(topBar.transform, "Chapter", new Vector2(x, labelY), new Vector2(380, 44),
            ChapterName, 26, FontStyle.Bold,
            new Color(1f, 0.95f, 0.55f), TextAnchor.MiddleLeft);

        // Health bar in the top ribbon (right side)
        BuildRibbonHealthBar(topBar.transform);

        // Reports panel (top right, ABOVE the minimap which is bottom-right)
        var evPanel = UIStyleHelper.MakeOrnatePanel(transform, 360, 320);
        var evRt = evPanel.GetComponent<RectTransform>();
        evRt.anchorMin = new Vector2(1, 1);
        evRt.anchorMax = new Vector2(1, 1);
        evRt.pivot = new Vector2(1, 1);
        evRt.anchoredPosition = new Vector2(-18, -116);
        evRt.sizeDelta = new Vector2(360, 320);

        MakeText(evPanel.transform, "Header", new Vector2(40, -22), new Vector2(320, 50),
            "REPORTS", 32, FontStyle.Bold,
            new Color(0.95f, 0.75f, 0.20f), TextAnchor.MiddleLeft);

        eventText = MakeText(evPanel.transform, "Events", new Vector2(40, -80), new Vector2(280, 220),
            "", 18, FontStyle.Bold,
            new Color(1f, 0.90f, 0.60f), TextAnchor.UpperLeft);
    }

    Image healthBarFill;
    Text healthText;

    void BuildRibbonHealthBar(Transform ribbon)
    {
        var hb = new GameObject("HealthBar");
        hb.transform.SetParent(ribbon, false);
        var hRt = hb.AddComponent<RectTransform>();
        hRt.anchorMin = new Vector2(1, 0.5f);
        hRt.anchorMax = new Vector2(1, 0.5f);
        hRt.pivot = new Vector2(1, 0.5f);
        hRt.anchoredPosition = new Vector2(-80, 0);
        hRt.sizeDelta = new Vector2(280, 44);

        // Simple background
        var bg = new GameObject("BG");
        bg.transform.SetParent(hb.transform, false);
        var bgRt = bg.AddComponent<RectTransform>();
        bgRt.anchorMin = Vector2.zero;
        bgRt.anchorMax = Vector2.one;
        bgRt.offsetMin = Vector2.zero;
        bgRt.offsetMax = Vector2.zero;
        var bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.05f, 0.05f, 1f);
        bgImg.raycastTarget = false;

        // Green fill bar (left-aligned)
        var fill = new GameObject("Fill");
        fill.transform.SetParent(hb.transform, false);
        var fillRt = fill.AddComponent<RectTransform>();
        fillRt.anchorMin = new Vector2(0, 0);
        fillRt.anchorMax = new Vector2(1, 1);
        fillRt.offsetMin = Vector2.zero;
        fillRt.offsetMax = Vector2.zero;
        healthBarFill = fill.AddComponent<Image>();
        healthBarFill.color = new Color(0.3f, 0.85f, 0.3f, 1f);
        healthBarFill.raycastTarget = false;
        healthBarFill.type = Image.Type.Filled;
        healthBarFill.fillMethod = Image.FillMethod.Horizontal;
        healthBarFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        healthBarFill.fillAmount = 1f;

        // HP text centered
        healthText = MakeText(hb.transform, "HPText", new Vector2(0, 0), new Vector2(280, 44),
            "HP 100/100", 18, FontStyle.Bold,
            new Color(1f, 0.95f, 0.55f), TextAnchor.MiddleCenter);
    }

    Text BuildResourceSlot(Transform parent, ref float x, Sprite sprite, string initialValue,
        float iconSize, float labelY, Color valueColor)
    {
        UIStyleHelper.MakeOrnateIcon(parent, sprite, (int)iconSize);
        var iconRt = parent.GetChild(parent.childCount - 1).GetComponent<RectTransform>();
        iconRt.anchorMin = new Vector2(0, 1);
        iconRt.anchorMax = new Vector2(0, 1);
        iconRt.pivot = new Vector2(0, 1);
        iconRt.anchoredPosition = new Vector2(x, 0);
        x += iconSize;

        var text = MakeText(parent, "Value_" + initialValue, new Vector2(x, labelY), new Vector2(120, 50),
            initialValue, 34, FontStyle.Bold, valueColor, TextAnchor.MiddleLeft);
        x += 120;
        return text;
    }

    void MakeVerticalSeparator(Transform parent, float x)
    {
        var sep = new GameObject("Sep");
        sep.transform.SetParent(parent, false);
        var rt = sep.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = new Vector2(x, -22);
        rt.sizeDelta = new Vector2(2, 70);
        var img = sep.AddComponent<Image>();
        img.color = new Color(0.55f, 0.40f, 0.18f, 0.7f);
        img.raycastTarget = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab)) ActionMenu.Toggle();

        if (goldText != null) goldText.text = $"{Gold}";
        if (woodText != null) woodText.text = $"{Wood}";
        if (foodText != null) foodText.text = $"{Food}";
        if (dayText != null) dayText.text = ChapterName;

        // Health bar
        if (healthBarFill != null && healthText != null)
        {
            int hp = PlayerController3D.Health;
            int maxHp = PlayerController3D.maxHealth;
            float pct = (float)hp / Mathf.Max(1, maxHp);
            healthBarFill.fillAmount = pct;
            // Color shifts from green to red as health drops
            if (pct > 0.6f) healthBarFill.color = new Color(0.3f, 0.85f, 0.3f, 1f);
            else if (pct > 0.3f) healthBarFill.color = new Color(0.95f, 0.7f, 0.2f, 1f);
            else healthBarFill.color = new Color(0.9f, 0.25f, 0.2f, 1f);
            healthText.text = $"HP {hp}/{maxHp}";
        }

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
