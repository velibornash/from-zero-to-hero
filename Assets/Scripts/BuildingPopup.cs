using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPopup : PopupBase
{
    static BuildingPopup instance;

    public static bool IsVisible => instance != null && instance.overlay != null && instance.overlay.activeSelf;

    public static void Show(string title, string body, string iconType = "default", Vector3 worldPos = default)
    {
        if (instance == null)
        {
            var go = new GameObject("BuildingPopup");
            instance = go.AddComponent<BuildingPopup>();
            instance.BuildUI(title, body);
        }
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
            HidePopup();
    }
}
