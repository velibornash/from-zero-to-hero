using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HUDInfoPanel : PopupBase
{
    void Start()
    {
        Debug.Log("HUDInfoPanel.Start() called");
        StartCoroutine(ShowWelcomeDelayed());
    }

    IEnumerator ShowWelcomeDelayed()
    {
        Debug.Log("HUDInfoPanel.ShowWelcomeDelayed started");
        yield return null;
        Debug.Log("HUDInfoPanel.ShowWelcomeDelayed after 1-frame yield");

        var all = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in all)
        {
            if (c.name == "Canvas" || c.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                canvas = c;
                break;
            }
        }
        if (canvas == null) canvas = FindAnyObjectByType<Canvas>();
        Debug.Log($"HUDInfoPanel canvas found: {canvas?.name} (renderMode={canvas?.renderMode})");
        if (canvas == null)
        {
            Debug.LogError("HUDInfoPanel: no Canvas found after one-frame delay!");
            yield break;
        }

        string body =
            "You arrive with nothing but 10 gold coins and the\n" +
            "will to rebuild. A forgotten valley lies before you,\n" +
            "rich with resources but prowled by wolves and barbarians.\n\n" +
            "Raise a church to unite the people. Plant the Serbian\n" +
            "flag to rally the land. Build towers to defend what is\n" +
            "yours, and summon mages to strike down the darkness.\n\n" +
            "Secure 300 gold, and the valley will be yours.\n\n" +
            "[WASD] Move  |  [E] Build  |  [TAB] Close";

        BuildUI("Chapter I: The Awakening", body, targetCanvas: canvas);
        Debug.Log($"HUDInfoPanel after BuildUI, overlay null: {overlay == null}, overlay active: {overlay?.activeSelf}");
        ShowPopup("Chapter I: The Awakening", body);
        Debug.Log($"HUDInfoPanel after ShowPopup, overlay active: {overlay?.activeSelf}");

        // Wait 3 frames before allowing dismiss — Unity can register
        // stale input events (anyKeyDown) on the first frame of play mode,
        // which would instantly hide the welcome popup.
        for (int i = 0; i < 3; i++)
            yield return null;

        allowDismiss = true;
    }

    bool allowDismiss;

    void Update()
    {
        if (overlay == null || !overlay.activeSelf || !allowDismiss) return;
        if (Input.anyKeyDown)
        {
            HidePopup();
        }
    }
}
