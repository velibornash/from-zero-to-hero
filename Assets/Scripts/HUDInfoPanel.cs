using UnityEngine;
using UnityEngine.UI;

public class HUDInfoPanel : MonoBehaviour
{
    GameObject overlay;
    GameObject panel;
    Text titleText;
    Text bodyText;
    bool introSeen;
    bool panelVisible;

    Sprite goldIcon, woodIcon, foodIcon;
    Text goldVal, woodVal, foodVal;

    void Start()
    {
        var canvas = GetComponent<Canvas>();

        overlay = new GameObject("InfoOverlay");
        overlay.transform.SetParent(transform, false);
        var oRt = overlay.AddComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.sizeDelta = Vector2.zero;
        var oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0, 0, 0, 0.6f);

        goldIcon = LoadIcon("HUDIcons/gold_icon");
        woodIcon = LoadIcon("HUDIcons/wood_icon");
        foodIcon = LoadIcon("HUDIcons/wheat_icon");
        BuildParchmentPanel();
        BuildTabIcons();
        introSeen = false;
        ShowIntro();
    }

    void BuildParchmentPanel()
    {
        Color woodDark = new Color(0.25f, 0.14f, 0.06f);
        Color woodMid = new Color(0.35f, 0.20f, 0.08f);
        Color parchmentBg = new Color(0.92f, 0.85f, 0.72f);
        Color parchmentDark = new Color(0.80f, 0.68f, 0.48f);
        Color inkDark = new Color(0.18f, 0.10f, 0.03f);
        Color inkBrown = new Color(0.40f, 0.25f, 0.12f);
        Color bannerRed = new Color(0.55f, 0.12f, 0.08f);
        Color goldAccent = new Color(0.75f, 0.55f, 0.12f);

        // Drop shadow
        var shadowPanel = new GameObject("ShadowPanel");
        shadowPanel.transform.SetParent(overlay.transform, false);
        var sRt = shadowPanel.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0.5f, 0.5f);
        sRt.anchorMax = new Vector2(0.5f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = new Vector2(5, -5);
        sRt.sizeDelta = new Vector2(590, 510);
        var sImg = shadowPanel.AddComponent<Image>();
        sImg.color = new Color(0, 0, 0, 0.45f);

        // Main panel
        panel = new GameObject("InfoPanel");
        panel.transform.SetParent(overlay.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(590, 510);
        var pImg = panel.AddComponent<Image>();
        pImg.color = parchmentBg;

        // Wood frame - outer thick border
        var woodFrame = new GameObject("WoodFrame");
        woodFrame.transform.SetParent(panel.transform, false);
        var wfRt = woodFrame.AddComponent<RectTransform>();
        wfRt.anchorMin = Vector2.zero;
        wfRt.anchorMax = Vector2.one;
        wfRt.sizeDelta = new Vector2(-10, -10);
        var wfImg = woodFrame.AddComponent<Image>();
        wfImg.color = woodMid;

        // Wood frame inner
        var woodInner = new GameObject("WoodInner");
        woodInner.transform.SetParent(woodFrame.transform, false);
        var wiRt = woodInner.AddComponent<RectTransform>();
        wiRt.anchorMin = Vector2.zero;
        wiRt.anchorMax = Vector2.one;
        wiRt.sizeDelta = new Vector2(-6, -6);
        var wiImg = woodInner.AddComponent<Image>();
        wiImg.color = woodDark;

        // Inner parchment border
        var innerBorder = new GameObject("InnerBorder");
        innerBorder.transform.SetParent(woodInner.transform, false);
        var ibRt = innerBorder.AddComponent<RectTransform>();
        ibRt.anchorMin = Vector2.zero;
        ibRt.anchorMax = Vector2.one;
        ibRt.sizeDelta = new Vector2(-8, -8);
        var ibImg = innerBorder.AddComponent<Image>();
        ibImg.color = new Color(0.90f, 0.82f, 0.68f);

        // Top banner / ribbon
        var banner = new GameObject("Banner");
        banner.transform.SetParent(panel.transform, false);
        var bRt = banner.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 1);
        bRt.anchorMax = new Vector2(1, 1);
        bRt.pivot = new Vector2(0.5f, 1);
        bRt.anchoredPosition = Vector2.zero;
        bRt.sizeDelta = new Vector2(-28, -68);
        var bImg = banner.AddComponent<Image>();
        bImg.color = bannerRed;

        // Gold trim on banner (top)
        var trimTop = new GameObject("TrimTop");
        trimTop.transform.SetParent(banner.transform, false);
        var ttRt = trimTop.AddComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0, 1);
        ttRt.anchorMax = new Vector2(1, 1);
        ttRt.pivot = new Vector2(0.5f, 1);
        ttRt.anchoredPosition = Vector2.zero;
        ttRt.sizeDelta = new Vector2(0, -3);
        var ttImg = trimTop.AddComponent<Image>();
        ttImg.color = goldAccent;

        // Gold trim on banner (bottom)
        var trimBottom = new GameObject("TrimBottom");
        trimBottom.transform.SetParent(banner.transform, false);
        var tbRt = trimBottom.AddComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = new Vector2(0, -68);
        tbRt.sizeDelta = new Vector2(0, -3);
        var tbImg = trimBottom.AddComponent<Image>();
        tbImg.color = goldAccent;

        // Side pillars (left + right wood strips flanking banner)
        for (int side = -1; side <= 1; side += 2)
        {
            var pillar = new GameObject("Pillar");
            pillar.transform.SetParent(panel.transform, false);
            var plRt = pillar.AddComponent<RectTransform>();
            plRt.anchorMin = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.anchorMax = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.pivot = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.anchoredPosition = new Vector2(side * 14, -8);
            plRt.sizeDelta = new Vector2(8, -40);
            var plImg = pillar.AddComponent<Image>();
            plImg.color = woodDark;
        }

        // Chapter title
        titleText = MakeLabel(panel, "Chapter I: The Awakening",
            new Vector2(0, -90), new Vector2(550, 40),
            new Color(1, 1, 0.85f), 32, FontStyle.Bold, TextAnchor.MiddleCenter);

        // Subtitle
        var subtitle = MakeLabel(panel, "From Zero to Hero",
            new Vector2(0, -126), new Vector2(550, 26),
            goldAccent, 20, FontStyle.Normal, TextAnchor.MiddleCenter);

        // Corner ornaments (4 corners) - larger, richer
        var cornerSize = new Vector2(36, 36);
        var cornerColor = new Color(0.60f, 0.40f, 0.15f);
        MakeCorner(panel, new Vector2(-260, 220), cornerSize, cornerColor, 0);
        MakeCorner(panel, new Vector2(260, 220), cornerSize, cornerColor, 90);
        MakeCorner(panel, new Vector2(-260, -220), cornerSize, cornerColor, -90);
        MakeCorner(panel, new Vector2(260, -220), cornerSize, cornerColor, 180);

        // Ornamental separator line
        var separator = new GameObject("Separator");
        separator.transform.SetParent(panel.transform, false);
        var sepRt = separator.AddComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0.5f, 1);
        sepRt.anchorMax = new Vector2(0.5f, 1);
        sepRt.pivot = new Vector2(0.5f, 1);
        sepRt.anchoredPosition = new Vector2(0, -165);
        sepRt.sizeDelta = new Vector2(430, 3);
        var sepImg = separator.AddComponent<Image>();
        sepImg.color = new Color(0.55f, 0.35f, 0.15f);

        // Small diamond ornament on separator
        var diamond = new GameObject("Diamond");
        diamond.transform.SetParent(separator.transform, false);
        var dRt = diamond.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.5f, 0.5f);
        dRt.anchorMax = new Vector2(0.5f, 0.5f);
        dRt.pivot = new Vector2(0.5f, 0.5f);
        dRt.anchoredPosition = Vector2.zero;
        dRt.sizeDelta = new Vector2(12, 12);
        var dImg = diamond.AddComponent<Image>();
        dImg.color = new Color(0.55f, 0.12f, 0.08f);
        dRt.localRotation = Quaternion.Euler(0, 0, 45);

        // Decorative rope loops on each side of diamond
        for (int side = -1; side <= 1; side += 2)
        {
            var loop = new GameObject("Loop");
            loop.transform.SetParent(separator.transform, false);
            var lRt = loop.AddComponent<RectTransform>();
            lRt.anchorMin = new Vector2(0.5f, 0.5f);
            lRt.anchorMax = new Vector2(0.5f, 0.5f);
            lRt.pivot = new Vector2(0.5f, 0.5f);
            lRt.anchoredPosition = new Vector2(side * 20, 0);
            lRt.sizeDelta = new Vector2(6, 6);
            var lImg = loop.AddComponent<Image>();
            lImg.color = goldAccent;
            lRt.localRotation = Quaternion.Euler(0, 0, 45);
        }

        // Body text
        bodyText = MakeLabel(panel, "",
            new Vector2(0, -179), new Vector2(520, 260),
            inkDark, 16, FontStyle.Normal, TextAnchor.UpperLeft);

        // Hint text at bottom
        var hintText = MakeLabel(panel, "Press any key to begin",
            new Vector2(0, -478), new Vector2(530, 24),
            inkBrown, 14, FontStyle.Normal, TextAnchor.MiddleCenter);
    }

    void MakeCorner(GameObject parent, Vector2 pos, Vector2 size, Color color, float rotation)
    {
        var corner = new GameObject("Corner");
        corner.transform.SetParent(parent.transform, false);
        var cRt = corner.AddComponent<RectTransform>();
        cRt.anchorMin = new Vector2(0.5f, 0.5f);
        cRt.anchorMax = new Vector2(0.5f, 0.5f);
        cRt.pivot = new Vector2(0.5f, 0.5f);
        cRt.anchoredPosition = pos;
        cRt.sizeDelta = size;
        cRt.localRotation = Quaternion.Euler(0, 0, rotation);
        var cImg = corner.AddComponent<Image>();
        cImg.color = color;
    }

    void ShowIntro()
    {
        panelVisible = false;
        overlay.SetActive(true);
        bodyText.text = "The road ahead is long, but every hero starts at zero.\n\n" +
                        "You arrive in a forgotten valley, where ruins of an old\n" +
                        "settlement whisper tales of a once-thriving community.\n\n" +
                        "The land is fertile, the forest is rich, and opportunity\n" +
                        "awaits those bold enough to seize it.\n\n" +
                        "Build your village, gather resources, and forge your destiny\n" +
                        "from nothing but grit and ambition.\n\n" +
                        "[WASD]  Move  |  [E] Build/Interact  |  [TAB] Info Panel";
    }

    Text MakeLabel(GameObject parent, string content, Vector2 pos, Vector2 size,
        Color color, int fontSize, FontStyle fontStyle, TextAnchor anchor)
    {
        var go = new GameObject("Label");
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
        text.fontStyle = fontStyle;
        text.color = color;
        text.alignment = anchor;
        text.text = content;
        return text;
    }

    void Update()
    {
        if (overlay.activeSelf && !introSeen)
        {
            if (Input.anyKeyDown)
            {
                introSeen = true;
                overlay.SetActive(false);
            }
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            panelVisible = !panelVisible;
            overlay.SetActive(panelVisible);
            if (panelVisible) Refresh();
        }
    }

    Sprite LoadIcon(string path)
    {
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        return null;
    }

    void BuildTabIcons()
    {
        float ix = -220f;
        float iy = -60f;

        AddTabIcon(panel, ref ix, goldIcon, "Gold", ref goldVal, new Color(1f, 0.84f, 0f));
        ix += 40f;
        AddTabIcon(panel, ref ix, woodIcon, "Wood", ref woodVal, new Color(0.65f, 0.45f, 0.15f));
        ix += 40f;
        AddTabIcon(panel, ref ix, foodIcon, "Food", ref foodVal, new Color(1f, 0.85f, 0.2f));
    }

    void AddTabIcon(GameObject parent, ref float x, Sprite sprite, string label, ref Text valText, Color tint)
    {
        var icon = new GameObject("TabIcon");
        icon.transform.SetParent(parent.transform, false);
        var iRt = icon.AddComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0.5f, 1);
        iRt.anchorMax = new Vector2(0.5f, 1);
        iRt.pivot = new Vector2(0, 1);
        iRt.anchoredPosition = new Vector2(x, -55f);
        iRt.sizeDelta = new Vector2(22, 22);
        var iImg = icon.AddComponent<Image>();
        iImg.sprite = sprite;
        iImg.color = tint;

        var val = new GameObject("TabVal");
        val.transform.SetParent(parent.transform, false);
        var vRt = val.AddComponent<RectTransform>();
        vRt.anchorMin = new Vector2(0.5f, 1);
        vRt.anchorMax = new Vector2(0.5f, 1);
        vRt.pivot = new Vector2(0, 1);
        vRt.anchoredPosition = new Vector2(x + 28, -55f);
        vRt.sizeDelta = new Vector2(50, 22);
        var vText = val.AddComponent<Text>();
        vText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        vText.fontSize = 16;
        vText.fontStyle = FontStyle.Bold;
        vText.color = new Color(0.18f, 0.10f, 0.03f);
        vText.alignment = TextAnchor.MiddleLeft;
        vText.text = "0";
        valText = vText;

        x += 110f;
    }

    void Refresh()
    {
        if (bodyText == null) return;
        bodyText.text = $"\n\n\n\n" +  // offset past the icon row
                        $"Day {HUDController.Day}\n\n" +
                        "[WASD] Move  |  [E] Build/Interact\n" +
                        "[TAB] Close";
        if (goldVal != null) goldVal.text = $"{HUDController.Gold}";
        if (woodVal != null) woodVal.text = $"{HUDController.Wood}";
        if (foodVal != null) foodVal.text = $"{HUDController.Food}";
    }
}
