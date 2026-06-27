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
        oImg.color = new Color(0, 0, 0, 0.75f);
        oImg.raycastTarget = false;

        goldIcon = LoadIcon("HUDIcons/gold_icon");
        woodIcon = LoadIcon("HUDIcons/wood_icon");
        foodIcon = LoadIcon("HUDIcons/wheat_icon");
        BuildParchmentPanel();
        introSeen = false;
        ShowIntro();
    }

    void BuildParchmentPanel()
    {
        Color woodDark = new Color(0.20f, 0.10f, 0.04f);
        Color woodMid = new Color(0.32f, 0.18f, 0.07f);
        Color parchmentBg = new Color(0.92f, 0.85f, 0.72f);
        Color goldAccent = new Color(0.85f, 0.65f, 0.20f);
        Color goldLight = new Color(0.95f, 0.78f, 0.30f);
        Color bannerRed = new Color(0.55f, 0.10f, 0.08f);
        Color inkDark = new Color(0.18f, 0.10f, 0.03f);

        // Drop shadow
        var shadowPanel = new GameObject("ShadowPanel");
        shadowPanel.transform.SetParent(overlay.transform, false);
        var sRt = shadowPanel.AddComponent<RectTransform>();
        sRt.anchorMin = new Vector2(0.5f, 0.5f);
        sRt.anchorMax = new Vector2(0.5f, 0.5f);
        sRt.pivot = new Vector2(0.5f, 0.5f);
        sRt.anchoredPosition = new Vector2(8, -8);
        sRt.sizeDelta = new Vector2(680, 580);
        var sImg = shadowPanel.AddComponent<Image>();
        sImg.color = new Color(0, 0, 0, 0.55f);
        sImg.raycastTarget = false;

        // Procedural ornate gold border (outer)
        var goldBorder = new GameObject("GoldBorder");
        goldBorder.transform.SetParent(overlay.transform, false);
        var gbRt = goldBorder.AddComponent<RectTransform>();
        gbRt.anchorMin = new Vector2(0.5f, 0.5f);
        gbRt.anchorMax = new Vector2(0.5f, 0.5f);
        gbRt.pivot = new Vector2(0.5f, 0.5f);
        gbRt.anchoredPosition = Vector2.zero;
        gbRt.sizeDelta = new Vector2(680, 580);
        var gbImg = goldBorder.AddComponent<Image>();
        gbImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 18, 24);
        gbImg.type = Image.Type.Sliced;
        gbImg.color = Color.white;
        gbImg.raycastTarget = false;

        // Procedural parchment background
        panel = new GameObject("InfoPanel");
        panel.transform.SetParent(overlay.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(640, 540);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = UIStyleHelper.MakeParchmentSprite(256, 256);
        pImg.type = Image.Type.Sliced;
        pImg.color = Color.white;
        pImg.raycastTarget = false;

        // Top banner / ribbon with red background and gold trim
        var banner = new GameObject("Banner");
        banner.transform.SetParent(panel.transform, false);
        var bRt = banner.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0, 1);
        bRt.anchorMax = new Vector2(1, 1);
        bRt.pivot = new Vector2(0.5f, 1);
        bRt.anchoredPosition = Vector2.zero;
        bRt.sizeDelta = new Vector2(-40, -84);
        var bImg = banner.AddComponent<Image>();
        bImg.color = bannerRed;
        bImg.raycastTarget = false;

        // Banner gold trim top
        var trimTop = new GameObject("TrimTop");
        trimTop.transform.SetParent(banner.transform, false);
        var ttRt = trimTop.AddComponent<RectTransform>();
        ttRt.anchorMin = new Vector2(0, 1);
        ttRt.anchorMax = new Vector2(1, 1);
        ttRt.pivot = new Vector2(0.5f, 1);
        ttRt.anchoredPosition = Vector2.zero;
        ttRt.sizeDelta = new Vector2(0, -4);
        var ttImg = trimTop.AddComponent<Image>();
        ttImg.color = goldLight;
        ttImg.raycastTarget = false;

        // Banner gold trim bottom
        var trimBottom = new GameObject("TrimBottom");
        trimBottom.transform.SetParent(banner.transform, false);
        var tbRt = trimBottom.AddComponent<RectTransform>();
        tbRt.anchorMin = new Vector2(0, 1);
        tbRt.anchorMax = new Vector2(1, 1);
        tbRt.pivot = new Vector2(0.5f, 1);
        tbRt.anchoredPosition = new Vector2(0, -84);
        tbRt.sizeDelta = new Vector2(0, -4);
        var tbImg = trimBottom.AddComponent<Image>();
        tbImg.color = goldLight;
        tbImg.raycastTarget = false;

        // Side pillars
        for (int side = -1; side <= 1; side += 2)
        {
            var pillar = new GameObject("Pillar");
            pillar.transform.SetParent(panel.transform, false);
            var plRt = pillar.AddComponent<RectTransform>();
            plRt.anchorMin = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.anchorMax = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.pivot = new Vector2(side == -1 ? 0 : 1, 1);
            plRt.anchoredPosition = new Vector2(side * 18, -10);
            plRt.sizeDelta = new Vector2(10, -50);
            var plImg = pillar.AddComponent<Image>();
            plImg.color = woodDark;
        }

        // Resource icons at top (inside the banner area)
        float iconY = -26f;
        AddOrnateIcon(panel, goldIcon, -200f, iconY, 56, 0);
        AddOrnateIcon(panel, woodIcon, 0f, iconY, 56, 1);
        AddOrnateIcon(panel, foodIcon, 200f, iconY, 56, 2);

        // Chapter title (above banner) — bold dark color on parchment for contrast
        titleText = MakeLabel(panel, "Chapter I: The Awakening",
            new Vector2(0, -110), new Vector2(640, 50),
            new Color(0.30f, 0.12f, 0.0f), 34, FontStyle.Bold, TextAnchor.MiddleCenter);

        // Subtitle
        var subtitle = MakeLabel(panel, "From Zero to Hero",
            new Vector2(0, -155), new Vector2(640, 30),
            new Color(0.50f, 0.25f, 0.05f), 20, FontStyle.Italic, TextAnchor.MiddleCenter);

        // Ornamental separator with diamond center
        var separator = new GameObject("Separator");
        separator.transform.SetParent(panel.transform, false);
        var sepRt = separator.AddComponent<RectTransform>();
        sepRt.anchorMin = new Vector2(0.5f, 1);
        sepRt.anchorMax = new Vector2(0.5f, 1);
        sepRt.pivot = new Vector2(0.5f, 1);
        sepRt.anchoredPosition = new Vector2(0, -195);
        sepRt.sizeDelta = new Vector2(500, 4);
        var sepImg = separator.AddComponent<Image>();
        sepImg.color = goldAccent;
        sepImg.raycastTarget = false;

        // Diamond on separator
        var diamond = new GameObject("Diamond");
        diamond.transform.SetParent(separator.transform, false);
        var dRt = diamond.AddComponent<RectTransform>();
        dRt.anchorMin = new Vector2(0.5f, 0.5f);
        dRt.anchorMax = new Vector2(0.5f, 0.5f);
        dRt.pivot = new Vector2(0.5f, 0.5f);
        dRt.anchoredPosition = Vector2.zero;
        dRt.sizeDelta = new Vector2(14, 14);
        var dImg = diamond.AddComponent<Image>();
        dImg.color = bannerRed;
        dRt.localRotation = Quaternion.Euler(0, 0, 45);

        // Body text — normal weight for readability, not bold
        bodyText = MakeLabel(panel, "",
            new Vector2(0, -220), new Vector2(560, 270),
            new Color(0.22f, 0.12f, 0.03f), 17, FontStyle.Normal, TextAnchor.UpperCenter);

        // Hint text at bottom
        var hintText = MakeLabel(panel, "Press any key to begin",
            new Vector2(0, -508), new Vector2(560, 28),
            new Color(0.95f, 0.80f, 0.40f), 15, FontStyle.Bold, TextAnchor.MiddleCenter);

        // Corner ornaments (4 corners) - larger
        var cornerSize = new Vector2(40, 40);
        var cornerColor = goldAccent;
        MakeCorner(panel, new Vector2(-300, 250), cornerSize, cornerColor, 0);
        MakeCorner(panel, new Vector2(300, 250), cornerSize, cornerColor, 90);
        MakeCorner(panel, new Vector2(-300, -250), cornerSize, cornerColor, -90);
        MakeCorner(panel, new Vector2(300, -250), cornerSize, cornerColor, 180);
    }

    void AddOrnateIcon(GameObject parent, Sprite sprite, float x, float y, int size, int slotIdx)
    {
        UIStyleHelper.MakeOrnateIcon(parent.transform, sprite, size);
        var iconObj = parent.transform.GetChild(parent.transform.childCount - 1).gameObject;
        var iRt = iconObj.GetComponent<RectTransform>();
        iRt.anchorMin = new Vector2(0.5f, 1);
        iRt.anchorMax = new Vector2(0.5f, 1);
        iRt.pivot = new Vector2(0.5f, 1);
        iRt.anchoredPosition = new Vector2(x, y);

        // Value text next to icon
        var val = MakeLabel(parent, "0", new Vector2(x + 24, y - 6), new Vector2(50, 26),
            new Color(0.95f, 0.78f, 0.30f), 18, FontStyle.Bold, TextAnchor.MiddleLeft);
        if (slotIdx == 0) goldVal = val;
        else if (slotIdx == 1) woodVal = val;
        else foodVal = val;
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
        if (goldVal != null) goldVal.text = $"{HUDController.Gold}";
        if (woodVal != null) woodVal.text = $"{HUDController.Wood}";
        if (foodVal != null) foodVal.text = $"{HUDController.Food}";
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

        // Close any open BuildingPopup with the same keys
        if (BuildingPopup.IsVisible &&
            (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X)))
        {
            BuildingPopup.Hide();
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
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

    void Refresh()
    {
        if (bodyText == null) return;
        bodyText.text = $"Day {HUDController.Day}\n\n" +
                        "[WASD] Move  |  [E] Build/Interact\n" +
                        "[TAB] Close";
        if (goldVal != null) goldVal.text = $"{HUDController.Gold}";
        if (woodVal != null) woodVal.text = $"{HUDController.Wood}";
        if (foodVal != null) foodVal.text = $"{HUDController.Food}";
    }
}
