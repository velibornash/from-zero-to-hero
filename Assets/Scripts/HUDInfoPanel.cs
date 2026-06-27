using UnityEngine;
using UnityEngine.UI;

public class HUDInfoPanel : PopupBase
{
    void Start()
    {
        BuildUI("Chapter I: The Awakening",
            "The road ahead is long, but every hero starts at zero.\n\n" +
            "You arrive in a forgotten valley, where ruins of an old\n" +
            "settlement whisper tales of a once-thriving community.\n\n" +
            "The land is fertile, the forest is rich, and opportunity\n" +
            "awaits those bold enough to seize it.\n\n" +
            "Build your village, gather resources, and forge your destiny\n" +
            "from nothing but grit and ambition.\n\n" +
            "[WASD]  Move  |  [E] Build/Interact  |  [TAB] Info Panel");
        panel = GameObject.Find("Panel");
        ShowIntro();
    }

    void ShowIntro()
    {
        Show("Chapter I: The Awakening",
            "The road ahead is long, but every hero starts at zero.\n\n" +
            "You arrive in a forgotten valley, where ruins of an old\n" +
            "settlement whisper tales of a once-thriving community.\n\n" +
            "The land is fertile, the forest is rich, and opportunity\n" +
            "awaits those bold enough to seize it.\n\n" +
            "Build your village, gather resources, and forge your destiny\n" +
            "from nothing but grit and ambition.\n\n" +
            "[WASD]  Move  |  [E] Build/Interact  |  [TAB] Info Panel");
    }

    void Update()
    {
        if (overlay == null || !overlay.activeSelf) return;
        if (Input.anyKeyDown)
        {
            Hide();
        }
    }
}
