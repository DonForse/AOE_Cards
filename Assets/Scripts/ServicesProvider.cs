using System;
using Infrastructure.Services;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    private ServicesProvider() { }
    public GameObject matchServiceGo;
    public GameObject loginServiceGo;

    private IMatchService _matchService;
    private ILoginService _loginService;

    public IMatchService GetMatchService()
    {
        if (_matchService != null)
            return _matchService;
        var ms = GameObject.Find("MatchService");
        if (ms == null)
        {
            ms = Instantiate<GameObject>(matchServiceGo);
            ms.name = "MatchService";
        }
            
        _matchService = ms.GetComponent<IMatchService>();
        return _matchService;
    }

    internal ILoginService GetLoginService()
    {
        if (_loginService != null)
            return _loginService;
        var ls = GameObject.Find("LoginService");
        if (ls == null)
        {
            ls = Instantiate<GameObject>(loginServiceGo);
            ls.name = "LoginService";
        }

        _loginService = ls.GetComponent<ILoginService>();
        return _loginService;
    
}
}