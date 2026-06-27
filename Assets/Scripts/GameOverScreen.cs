using UnityEngine;
using UnityEngine.UI;

public class GameOverScreen : PopupBase
{
    static GameOverScreen instance;

    public static void Show()
    {
        if (instance == null)
        {
            var go = new GameObject("GameOverScreen");
            instance = go.AddComponent<GameOverScreen>();
            instance.BuildUI("DEFEAT",
                "The wolves and barbarians overwhelmed your defenses.\n\n" +
                "You fought bravely, but the village needed more towers, more mages, more time.\n\n" +
                "The Serbian banner has fallen. The people have scattered.\n\n" +
                "Perhaps next time, build faster, defend better.");
            // Override close button text to "TRY AGAIN" and wire to RestartGame
            var closeBtn = instance.panel.transform.Find("CloseButton");
            if (closeBtn != null)
            {
                var btn = closeBtn.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(instance.RestartGame);
                var lbl = closeBtn.transform.Find("CloseLabel");
                if (lbl != null) lbl.GetComponent<Text>().text = "TRY AGAIN";
            }
        }
        instance.ShowPopup("DEFEAT", "The village has fallen.");
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
        Hide();
        if (instance != null) Destroy(instance.gameObject);
        HUDController.PushEvent("A new day begins. Defend the village!");
    }
}

