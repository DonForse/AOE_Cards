using System;
using UniRx;

namespace Login.UnityDelivery
{
    public interface ILoginView
    {
        IObservable<(string username, string password)> OnLoginButtonPressed();
        IObservable<Unit> OnGuestButtonPressed();
        IObservable<(string username, string password)> OnRegisterButtonPressed();
        void ShowError(string error);
        void DisableButtons();
        void EnableButtons();

        void NavigateToHomeView();
    }
}