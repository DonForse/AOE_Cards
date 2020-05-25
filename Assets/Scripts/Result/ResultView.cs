using Game;
using Home;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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

            _resultText.text = "Victory";
            _resultText.color = Color.green;
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
        servicesProvider.GetMatchService().RemoveMatch(() => { }, (code, error) => {
            //TODO: Be careful on token error and this shit is not enabled, idk what the fuck it will happen. also even if enabled, this shit is not doing anyhitng about tokens.
            //think better token management architechture, make the services implement the token refresh shit.
        });
        _navigator.OpenHomeView();
    }
}
