using System;
using Home;
using UnityEngine;
using UnityEngine.UI;

public class TutorialView : MonoBehaviour, IView
{
    [SerializeField] private ServicesProvider _servicesProvider;
    [SerializeField] private Navigator _navigator;
    [SerializeField] private Button _backButton;

    public void OnOpening()
    {
        _backButton.onClick.AddListener(CloseView);
        this.gameObject.SetActive(true);
    }

    public void OnClosing()
    {
        _backButton.onClick.RemoveAllListeners();
        this.gameObject.SetActive(false);
    }

    private void CloseView()
    {
        _navigator.OpenHomeView();
    }
}
