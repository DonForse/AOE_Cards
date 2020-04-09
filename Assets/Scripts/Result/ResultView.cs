using Home;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultView : MonoBehaviour, IView
{

    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private Button _continueButton;
    [SerializeField] private Navigator _navigator;

    public void OnClosing()
    {
        this.gameObject.SetActive(false);
    }

    public void OnOpening()
    {
        this.gameObject.SetActive(true);
    }

    public void SetResult(MatchResult result)
    {
        if (result == MatchResult.Win)
        {
            _resultText.text = "Victory";
            _resultText.color = Color.green;
        }
        else if (result == MatchResult.Lose)
        {
            _resultText.text = "Defeat";
            _resultText.color = Color.red;
        }
        else {
            _resultText.text = "Tie";
            _resultText.color = Color.yellow;
        }

        _continueButton.onClick.AddListener(GoBackHome); 

    }

    private void GoBackHome() {
        _navigator.OpenHomeView();
    }
}
