namespace Login
{
    internal interface ILoginView
    {
        void OnLoginComplete();
        void OnLoginFail(string message);
    }
}