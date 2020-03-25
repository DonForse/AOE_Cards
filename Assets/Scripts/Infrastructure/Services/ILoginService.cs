using System;

public interface ILoginService
{
    void Register(string playerName, string password, Action<string> onRegisterComplete);
    void Login(string playerName, string password, Action<string> onLoginComplete);
}