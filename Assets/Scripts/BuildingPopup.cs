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
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("BuildingPopup: no Canvas, cannot show popup.");
            return;
        }

        if (instance == null || instance.overlay == null)
        {
            if (instance != null) Object.DestroyImmediate(instance.gameObject);
            var go = new GameObject("BuildingPopup");
            instance = go.AddComponent<BuildingPopup>();
            instance.BuildUI(title, body);
            // Wire close button to custom callback if provided
            var closeBtn = instance.panel?.transform.Find("CloseButton");
            if (closeBtn != null)
            {
                var btn = closeBtn.GetComponent<UnityEngine.UI.Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => {
                    if (closeCallback != null) closeCallback();
                    else instance.HidePopup();
                });
            }
        }

        closeCallback = onClose;
        instance.ShowPopup(title, body);
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
}
