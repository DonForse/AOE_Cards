namespace Login.Scripts.Domain
{
    internal interface ILoginView
    {
        void OnLoginComplete();
        void OnLoginFail(string message);
        void ShowError(string error);
    }
}