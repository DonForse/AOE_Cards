using Game;
using Home;
using Sound;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Result
{
    public class ResultView : MonoBehaviour, IView
    {
        [SerializeField] private ServicesProvider servicesProvider;
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private Button _continueButton;
        [SerializeField] private Navigator _navigator;

        [SerializeField] private AudioClip victoryMusicClip;
        [SerializeField] private AudioClip defeatMusicClip;

        [SerializeField] private AudioClip victorySoundClip;
        [SerializeField] private AudioClip defeatSoundClip;
        public void OnClosing()
        {
            this.gameObject.SetActive(false);
            SoundManager.Instance.StopBackground();
        }

        public void OnOpening()
        {
            this.gameObject.SetActive(true);
        }

        public void SetResult(MatchResult result)
        {
            if (result == MatchResult.Win)
            {
                SoundManager.Instance.PlayBackground(victoryMusicClip, new AudioClipOptions { loop = true }, true);
                SoundManager.Instance.PlayAudioClip(victorySoundClip, new AudioClipOptions { loop = false });

                _resultText.text = "You are Victorious";
                _resultText.color = new Color(255,204,0);
            }
            else if (result == MatchResult.Lose)
            {
                SoundManager.Instance.PlayBackground(defeatMusicClip, new AudioClipOptions { loop = true }, true);
                SoundManager.Instance.PlayAudioClip(defeatSoundClip, new AudioClipOptions { loop = false });
                _resultText.text = "Defeat";
                _resultText.color = Color.red;
            }
            else {
                SoundManager.Instance.PlayBackground(victoryMusicClip, new AudioClipOptions { loop = true }, true);
                _resultText.text = "Tie";
                _resultText.color = Color.yellow;
            }

            _continueButton.onClick.AddListener(GoBackHome); 

        }

        private void GoBackHome()
        {
            servicesProvider.GetMatchService().RemoveMatch();
            _navigator.OpenHomeView();
        }
    }
}
