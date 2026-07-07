using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPopup : PopupBase
{
    static BuildingPopup instance;
    static System.Action closeCallback;

    public static bool IsVisible => instance != null && instance.overlay != null && instance.overlay.activeSelf;

    public static void Show(string title, string body, string iconType = "default", Vector3 worldPos = default, System.Action onClose = null)
    {
        var canvas = FindMainCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("BuildingPopup: no main Canvas found, cannot show popup.");
            return;
        }

        if (instance == null || instance.overlay == null)
        {
            if (instance != null) Object.DestroyImmediate(instance.gameObject);
            var go = new GameObject("BuildingPopup");
            instance = go.AddComponent<BuildingPopup>();
            instance.BuildUI(title, body, targetCanvas: canvas);
        }

        closeCallback = onClose;
        WireCloseButton();
        instance.ShowPopup(title, body);
    }

    static void WireCloseButton()
    {
        if (instance == null || instance.panel == null) return;
        var closeBtn = instance.panel.transform.Find("CloseButton");
        if (closeBtn == null) return;
        var btn = closeBtn.GetComponent<UnityEngine.UI.Button>();
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() => {
            if (closeCallback != null) closeCallback();
            else instance.HidePopup();
        });
    }

    public static void Hide()
    {
        if (instance != null) instance.HidePopup();
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            if (closeCallback != null) closeCallback();
            else HidePopup();
        }
    }

    static Canvas FindMainCanvas()
    {
        var all = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in all)
        {
            Debug.Log($"BuildingPopup scanning Canvas: name='{c.name}', renderMode={c.renderMode}");
            if (c.name == "Canvas" || c.renderMode == UnityEngine.RenderMode.ScreenSpaceOverlay)
                return c;
        }
        return all.Length > 0 ? all[0] : null;
    }
}
