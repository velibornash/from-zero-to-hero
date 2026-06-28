using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : PopupBase
{
    static GameOverScreen instance;

    public static void Show()
    {
        var canvas = FindMainCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("GameOverScreen: no main Canvas found.");
            return;
        }

        if (instance == null || instance.overlay == null)
        {
            if (instance != null) DestroyImmediate(instance.gameObject);
            var go = new GameObject("GameOverScreen");
            instance = go.AddComponent<GameOverScreen>();
            instance.BuildUI("THE VALLEY FALLS",
                "The wolves and barbarians overwhelm your defenses.\n\n" +
                "The bell tower crumbles. The tricolor is torn down.\n" +
                "Your people flee into the dark forest.\n\n" +
                "The valley is lost — but the story is not over.\n" +
                "A hero's spirit cannot be slain.\n\n" +
                "Rise again. Build faster. Defend better.");
            var closeBtn = instance.panel?.transform.Find("CloseButton");
            if (closeBtn != null)
            {
                var btn = closeBtn.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(instance.RestartGame);
                    var lbl = closeBtn.transform.Find("CloseLabel");
                    if (lbl != null) lbl.GetComponent<Text>().text = "TRY AGAIN";
                }
            }
        }
        instance.ShowPopup("THE VALLEY FALLS", "The village has fallen.");
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        PlayerController3D.Health = PlayerController3D.maxHealth;
        PlayerController3D.IsDead = false;
        var hero = FindAnyObjectByType<PlayerController3D>();
        if (hero != null)
        {
            hero.transform.position = new Vector3(0, 0, -55);
            hero.ResetRegen();
        }
        foreach (var e in FindObjectsByType<Enemy>())
            Destroy(e.gameObject);
        HidePopup();
        instance = null;
        if (this != null) Destroy(gameObject);
        HUDController.ResetState();
        HUDController.PushEvent("A new day begins. Defend the village!");
    }

    void OnDestroy()
    {
        if (instance == this) instance = null;
    }

    static Canvas FindMainCanvas()
    {
        var all = FindObjectsByType<Canvas>(FindObjectsInactive.Include);
        foreach (var c in all)
        {
            Debug.Log($"GameOverScreen scanning Canvas: name='{c.name}', renderMode={c.renderMode}");
            if (c.name == "Canvas" || c.renderMode == RenderMode.ScreenSpaceOverlay)
                return c;
        }
        return all.Length > 0 ? all[0] : null;
    }
}

