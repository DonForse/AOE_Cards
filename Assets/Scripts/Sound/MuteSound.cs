using UnityEngine;
using UnityEngine.UI;

public class MuteSound : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private GameObject soundOnImage;
    [SerializeField] private GameObject soundOffImage;

    private void Start()
    {
        _button.onClick.AddListener(ToggleSound);
    }

    public void ToggleSound() {
        SoundManager.Instance.ToggleMute();
        if (SoundManager.Instance.IsMuted) {
            soundOnImage.SetActive(false);
            soundOffImage.SetActive(true);
        }
        else {
            soundOnImage.SetActive(true);
            soundOffImage.SetActive(false);
        }
        
    }
}
