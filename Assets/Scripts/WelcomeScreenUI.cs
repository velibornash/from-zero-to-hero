using UnityEngine;
using System.Collections;

public class WelcomeScreenUI : MonoBehaviour
{
    void Start()
    {
        if (PlayerPrefs.GetInt("HasSeenTeslaIntro", 0) == 1) return;
        StartCoroutine(ShowTeslaIntro());
    }

    IEnumerator ShowTeslaIntro()
    {
        yield return new WaitForSeconds(0.5f);
        if (ElderPopup.Instance != null)
        {
            ElderPopup.Instance.Show(
                "I am Nikola Tesla. I will guide you through this world and help you with knowledge along the way.",
                5f);
        }
        else
        {
            Debug.LogWarning("[Welcome] ElderPopup.Instance is null — cannot show Tesla introduction.");
        }
        PlayerPrefs.SetInt("HasSeenTeslaIntro", 1);
        PlayerPrefs.Save();
    }
}
