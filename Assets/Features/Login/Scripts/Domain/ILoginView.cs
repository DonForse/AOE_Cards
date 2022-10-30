using System;
using UniRx;

namespace Login.Scripts.Domain
{
    public interface ILoginView
    {
        IObservable<(string username, string password)> OnLoginButtonPressed();
        IObservable<Unit> OnGuestButtonPressed();
        IObservable<(string username, string password)> OnRegisterButtonPressed();
        void ShowWarning(string message);
        void ShowError(string message);
        void DisableButtons();
        void EnableButtons();

        void NavigateToHomeView();
        
    }
}