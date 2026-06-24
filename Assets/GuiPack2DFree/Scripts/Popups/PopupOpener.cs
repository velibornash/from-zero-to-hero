using UnityEngine;

namespace GUIPack2DFree
{
    public class PopupOpener : MonoBehaviour
    {
        public GameObject Popup;

        public void OpenPopup()
        {
            var screen = Object.FindAnyObjectByType<PopupSystem>();

            AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
            screen.OpenPopup(Popup);
        }
    }
}