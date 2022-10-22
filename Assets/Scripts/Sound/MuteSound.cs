using UnityEngine;
using UnityEngine.UI;

namespace Sound
{
    public class MuteSound : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private GameObject soundOnImage;
        [SerializeField] private GameObject soundOffImage;

        private void Start()
        {
            _button.onClick.AddListener(ToggleSound);

            if (PlayerPrefs.GetInt(PlayerPrefsHelper.Sound) == 1)
                return;

            ToggleSound();
        }

        public void ToggleSound()
        {
            SoundManager.Instance.ToggleMute();
        
            if (SoundManager.Instance.IsMuted)
            {
                soundOnImage.SetActive(false);
                soundOffImage.SetActive(true);
                PlayerPrefs.SetInt(PlayerPrefsHelper.Sound, 0);
            }
            else
            {
                soundOnImage.SetActive(true);
                soundOffImage.SetActive(false);
                PlayerPrefs.SetInt(PlayerPrefsHelper.Sound, 1);
            }
            PlayerPrefs.Save();

        }
    }
}
