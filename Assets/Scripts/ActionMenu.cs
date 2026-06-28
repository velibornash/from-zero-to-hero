using UnityEngine;
using UnityEngine.UI;

public class ActionMenu : PopupBase
{
    static ActionMenu instance;
    static float lastCloseTime;

    public static bool IsVisible => instance != null && instance.overlay != null && instance.overlay.activeSelf;

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
        }

        // Build the dynamic body
        string body = "";
        var slots = Object.FindObjectsByType<BuildSlot>(FindObjectsInactive.Include);
        foreach (var s in slots)
        {
            if (s.data == null) continue;
            string status = s.state.ToString();
            if (s.state == BuildSlot.State.Available)
                status = $"Available — {s.data.cost} gold";
            else if (s.state == BuildSlot.State.Built)
                status = "Built";
            else if (s.state == BuildSlot.State.Locked)
                status = "Locked";
            else if (s.state == BuildSlot.State.Building)
                status = "Building...";

            body += $"{s.data.slotName}  [{status}]\n";
        }

        body += "\nControls:\n";
        body += "[WASD]  Move\n";
        body += "[E]  Interact / Build\n";
        body += "[TAB]  Toggle Menu\n";
        body += "[ESC] [X]  Close\n";

        body += $"\nGold: {HUDController.Gold}  Wood: {HUDController.Wood}  Food: {HUDController.Food}";
        body += $"\n{buildProgress}";

        instance.ShowPopup("GAME ACTIONS", body);

        // Make font bigger and bolder for readability
        if (instance.bodyText != null)
        {
            instance.bodyText.fontSize = 18;
            instance.bodyText.fontStyle = FontStyle.Bold;
            instance.bodyText.resizeTextForBestFit = false;
            instance.bodyText.resizeTextMinSize = 12;
            instance.bodyText.resizeTextMaxSize = 22;
            instance.bodyText.color = new Color(0.30f, 0.15f, 0.05f);
            instance.bodyText.lineSpacing = 1.3f;
        }

        // Make panel taller to fit all content without shrinking font
        if (instance.panel != null)
        {
            var pRt = instance.panel.GetComponent<RectTransform>();
            pRt.sizeDelta = new Vector2(PANEL_WIDTH, PANEL_HEIGHT + 60);
        }
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
