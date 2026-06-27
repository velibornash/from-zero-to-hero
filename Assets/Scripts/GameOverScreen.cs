using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : MonoBehaviour
{
    static GameOverScreen instance;
    Canvas canvas;
    GameObject overlay;

    public static void Show()
    {
        if (instance == null)
        {
            var go = new GameObject("GameOverScreen");
            instance = go.AddComponent<GameOverScreen>();
        }
        instance.DoShow();
    }

    void Awake()
    {
        canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null) return;
        BuildUI();
    }

    void BuildUI()
    {
        // Dark backdrop
        overlay = new GameObject("GameOverOverlay");
        overlay.transform.SetParent(canvas.transform, false);
        var oRt = overlay.AddComponent<RectTransform>();
        oRt.anchorMin = Vector2.zero;
        oRt.anchorMax = Vector2.one;
        oRt.sizeDelta = Vector2.zero;
        var oImg = overlay.AddComponent<Image>();
        oImg.color = new Color(0.1f, 0.0f, 0.0f, 0.85f);
        oImg.raycastTarget = true;
        overlay.SetActive(false);

        // Panel with ornate border
        var border = new GameObject("Border");
        border.transform.SetParent(overlay.transform, false);
        var bRt = border.AddComponent<RectTransform>();
        bRt.anchorMin = new Vector2(0.5f, 0.5f);
        bRt.anchorMax = new Vector2(0.5f, 0.5f);
        bRt.pivot = new Vector2(0.5f, 0.5f);
        bRt.anchoredPosition = Vector2.zero;
        bRt.sizeDelta = new Vector2(560, 360);
        var bImg = border.AddComponent<Image>();
        bImg.sprite = UIStyleHelper.Make9SliceBorder(96, 96, 14, 18);
        bImg.type = Image.Type.Sliced;
        bImg.color = Color.white;
        bImg.raycastTarget = false;

        var panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);
        var pRt = panel.AddComponent<RectTransform>();
        pRt.anchorMin = new Vector2(0.5f, 0.5f);
        pRt.anchorMax = new Vector2(0.5f, 0.5f);
        pRt.pivot = new Vector2(0.5f, 0.5f);
        pRt.anchoredPosition = Vector2.zero;
        pRt.sizeDelta = new Vector2(540, 340);
        var pImg = panel.AddComponent<Image>();
        pImg.sprite = UIStyleHelper.MakeParchmentSprite(256, 256);
        pImg.type = Image.Type.Sliced;
        pImg.color = Color.white;
        pImg.raycastTarget = false;

        // Red banner at top
        var banner = new GameObject("Banner");
        banner.transform.SetParent(panel.transform, false);
        var baRt = banner.AddComponent<RectTransform>();
        baRt.anchorMin = new Vector2(0, 1);
        baRt.anchorMax = new Vector2(1, 1);
        baRt.pivot = new Vector2(0.5f, 1);
        baRt.anchoredPosition = Vector2.zero;
        baRt.sizeDelta = new Vector2(0, -80);
        var baImg = banner.AddComponent<Image>();
        baImg.color = new Color(0.55f, 0.10f, 0.08f);
        baImg.raycastTarget = false;

        // Title "DEFEAT"
        var title = MakeText(panel, "Title", new Vector2(0, -10), new Vector2(500, 60),
            "DEFEAT", 56, FontStyle.Bold,
            new Color(1f, 0.85f, 0.30f), TextAnchor.MiddleCenter);

        // Subtitle
        var sub = MakeText(panel, "Sub", new Vector2(0, -65), new Vector2(500, 30),
            "The village has fallen", 20, FontStyle.Italic,
            new Color(0.40f, 0.20f, 0.05f), TextAnchor.MiddleCenter);

        // Body
        var body = MakeText(panel, "Body", new Vector2(0, -110), new Vector2(480, 140),
            "The wolves and barbarians overwhelmed your defenses.\n\n" +
            "You fought bravely, but the village needed more towers, more mages, more time.\n\n" +
            "The Serbian banner has fallen. The people have scattered.\n\n" +
            "Perhaps next time, build faster, defend better.",
            16, FontStyle.Normal,
            new Color(0.25f, 0.12f, 0.03f), TextAnchor.UpperCenter);

        // Restart button
        var btn = new GameObject("RestartBtn");
        btn.transform.SetParent(panel.transform, false);
        var btRt = btn.AddComponent<RectTransform>();
        btRt.anchorMin = new Vector2(0.5f, 0);
        btRt.anchorMax = new Vector2(0.5f, 0);
        btRt.pivot = new Vector2(0.5f, 0);
        btRt.anchoredPosition = new Vector2(0, 20);
        btRt.sizeDelta = new Vector2(220, 50);
        var btImg = btn.AddComponent<Image>();
        btImg.color = new Color(0.55f, 0.10f, 0.08f);
        btImg.raycastTarget = true;
        var bBtn = btn.AddComponent<Button>();
        bBtn.onClick.AddListener(RestartGame);
        MakeText(btn, "Lbl", Vector2.zero, new Vector2(220, 50),
            "TRY AGAIN", 22, FontStyle.Bold,
            new Color(1f, 0.95f, 0.65f), TextAnchor.MiddleCenter);
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
        return text;
    }

    public void DoShow()
    {
        if (overlay != null) overlay.SetActive(true);
        Time.timeScale = 0f; // Pause game
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        PlayerController3D.Health = PlayerController3D.maxHealth;
        PlayerController3D.IsDead = false;
        // Delete all enemies
        foreach (var e in FindObjectsByType<Enemy>())
            Destroy(e.gameObject);
        // Delete the game over screen
        if (instance != null) Destroy(instance.gameObject);
        // Reset hero position
        var hero = FindAnyObjectByType<PlayerController3D>();
        if (hero != null) hero.transform.position = new Vector3(0, 0, -55);
        HUDController.PushEvent("A new day begins. Defend the village!");
    }
}
