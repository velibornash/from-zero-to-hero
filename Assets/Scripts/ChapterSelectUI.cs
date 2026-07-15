using UnityEngine;
using UnityEngine.UI;

public class ChapterSelectUI : MonoBehaviour
{
    public static ChapterSelectUI Instance;
    public static int CurrentChapter = 1;

    Canvas canvas;
    GameObject panel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ShowIfShouldUnlock();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
            Toggle();
    }

    public void Toggle()
    {
        if (panel != null && panel.activeSelf)
            panel.SetActive(false);
        else
            Show();
    }

    public void Show()
    {
        if (panel == null) CreateUI();
        UpdateLockStates();
        panel.SetActive(true);
    }

    public void ShowIfShouldUnlock()
    {
        int next = CurrentChapter + 1;
        if (next <= 3 && PlayerPrefs.GetInt($"Chapter{next}Unlocked", 0) == 0)
        {
            if (IsChapterComplete(CurrentChapter))
            {
                PlayerPrefs.SetInt($"Chapter{next}Unlocked", 1);
                PlayerPrefs.Save();
                Debug.Log($"[Chapter] Chapter {next} unlocked!");
            }
        }
    }

    bool IsChapterComplete(int chapter)
    {
        if (chapter == 1)
            return HUDController.Gold >= 300 && AllBuildingsBuilt();
        return false;
    }

    bool AllBuildingsBuilt()
    {
        var slots = FindObjectsByType<BuildSlot>(FindObjectsInactive.Exclude);
        int[] required = { 0, 1, 2, 3, 4, 5 };
        foreach (int i in required)
        {
            bool found = false;
            foreach (var s in slots)
                if (s.slotIndex == i && s.IsBuilt) { found = true; break; }
            if (!found) return false;
        }
        return true;
    }

    void CreateUI()
    {
        Canvas mainCanvas = null;
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsInactive.Exclude))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { mainCanvas = c; break; }
        if (mainCanvas == null) return;

        panel = new GameObject("ChapterSelect");
        panel.transform.SetParent(mainCanvas.transform, false);
        var panelRt = panel.AddComponent<RectTransform>();
        panelRt.anchorMin = Vector2.zero;
        panelRt.anchorMax = Vector2.one;
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;

        var overlay = panel.AddComponent<Image>();
        overlay.color = new Color(0, 0, 0, 0.7f);
        overlay.raycastTarget = true;

        var title = MakeText(panel.transform, "Title",
            new Vector2(0, 200), new Vector2(600, 60),
            "SELECT CHAPTER", 32, FontStyle.Bold,
            new Color(1f, 0.85f, 0.3f), TextAnchor.MiddleCenter);

        string[] names = { "The Awakening", "The Gathering Storm", "Kingdom Rising" };
        string[] sprites = { "ch1", "ch2", "ch3" };

        float cardW = 280f;
        float cardH = 380f;
        float gap = 30f;
        float totalW = cardW * 3 + gap * 2;
        float startX = -totalW * 0.5f;

        for (int i = 0; i < 3; i++)
        {
            bool unlocked = i == 0 || PlayerPrefs.GetInt($"Chapter{i + 1}Unlocked", 0) == 1;
            bool completed = i < CurrentChapter;

            float x = startX + i * (cardW + gap) + cardW * 0.5f;
            CreateCard(panel.transform, x, -30f, cardW, cardH,
                names[i], sprites[i], i + 1, unlocked, completed);
        }

        var closeBtn = MakeButton(panel.transform, "Close",
            new Vector2(0, -250), new Vector2(160, 45),
            "CLOSE", () => panel.SetActive(false));
    }

    void CreateCard(Transform parent, float x, float y, float w, float h,
        string name, string spriteName, int chapter, bool unlocked, bool completed)
    {
        var card = new GameObject($"Chapter{chapter}");
        card.transform.SetParent(parent, false);
        var rt = card.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);

        var img = card.AddComponent<Image>();
        img.color = unlocked
            ? new Color(0.15f, 0.12f, 0.08f, 0.95f)
            : new Color(0.08f, 0.08f, 0.08f, 0.85f);

        var bgTex = Resources.Load<Texture2D>($"Chapters/{spriteName}");
        if (bgTex != null)
        {
            var bgSprite = Sprite.Create(bgTex, new Rect(0, 0, bgTex.width, bgTex.height), Vector2.one * 0.5f);
            var bg = new GameObject("BG");
            bg.transform.SetParent(card, false);
            var bgRt = bg.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = Vector2.one;
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bg.AddComponent<Image>();
            bgImg.sprite = bgSprite;
            bgImg.type = Image.Type.Simple;
            bgImg.preserveAspect = false;
            bgImg.color = unlocked ? Color.white : new Color(0.3f, 0.3f, 0.3f, 0.6f);
        }

        if (!unlocked)
        {
            var lockGo = new GameObject("Lock");
            lockGo.transform.SetParent(card, false);
            var lockRt = lockGo.AddComponent<RectTransform>();
            lockRt.anchorMin = new Vector2(0.5f, 0.5f);
            lockRt.anchorMax = new Vector2(0.5f, 0.5f);
            lockRt.sizeDelta = new Vector2(60, 60);
            var lockImg = lockGo.AddComponent<Image>();
            lockImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            lockImg.raycastTarget = false;
            var lockText = MakeText(lockGo.transform, "LockIcon",
                Vector2.zero, new Vector2(60, 60),
                "LOCKED", 14, FontStyle.Bold,
                new Color(0.7f, 0.7f, 0.7f), TextAnchor.MiddleCenter);
        }

        MakeText(card.transform, "Name",
            new Vector2(0, -h * 0.5f + 30), new Vector2(w - 20, 40),
            name, 20, FontStyle.Bold,
            unlocked ? new Color(1f, 0.9f, 0.6f) : new Color(0.4f, 0.4f, 0.4f),
            TextAnchor.MiddleCenter);

        if (completed)
        {
            MakeText(card.transform, "Check",
                new Vector2(0, 0), new Vector2(60, 60),
                "DONE", 18, FontStyle.Bold,
                new Color(0.3f, 0.9f, 0.3f), TextAnchor.MiddleCenter);
        }

        if (unlocked && !completed)
        {
            MakeButton(card.transform, "PlayBtn",
                new Vector2(0, -h * 0.5f + 70), new Vector2(160, 40),
                "PLAY", () => SelectChapter(chapter));
        }
    }

    void SelectChapter(int chapter)
    {
        CurrentChapter = chapter;
        panel.SetActive(false);
        Debug.Log($"[Chapter] Selected Chapter {chapter}");
    }

    void UpdateLockStates()
    {
        if (panel == null) return;
        for (int i = 1; i <= 3; i++)
        {
            var card = panel.transform.Find($"Chapter{i}");
            if (card == null) continue;
            var lockObj = card.Find("Lock");
            bool unlocked = i == 1 || PlayerPrefs.GetInt($"Chapter{i}Unlocked", 0) == 1;
            if (lockObj != null) lockObj.gameObject.SetActive(!unlocked);
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

    Button MakeButton(Transform parent, string name, Vector2 pos, Vector2 size,
        string label, UnityEngine.Events.UnityAction onClick)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        var img = go.AddComponent<Image>();
        img.color = new Color(0.75f, 0.55f, 0.1f);
        var btn = go.AddComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(onClick);

        var txtGo = new GameObject("Label");
        txtGo.transform.SetParent(go.transform, false);
        var txtRt = txtGo.AddComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.offsetMin = Vector2.zero;
        txtRt.offsetMax = Vector2.zero;
        var txt = txtGo.AddComponent<Text>();
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 18;
        txt.fontStyle = FontStyle.Bold;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = label;
        txt.raycastTarget = false;
        return btn;
    }
}
