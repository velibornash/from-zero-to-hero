using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BuildingPopup : PopupBase
{
    static BuildingPopup instance;

    public static bool IsVisible => instance != null && overlay != null && overlay.activeSelf;

    public static void Show(string title, string body, string iconType = "default", Vector3 worldPos = default)
    {
        if (instance == null)
        {
            var go = new GameObject("BuildingPopup");
            instance = go.AddComponent<BuildingPopup>();
            instance.BuildUI(title, body);
        }
        else
        {
            instance.Show(title, body);
        }
    }

    public new static void Hide()
    {
        if (instance != null) instance.Hide();
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
            DoHide();
    }

    public new void DoHide()
    {
        Hide();
    }
}
