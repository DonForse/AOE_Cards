using System;
using Infrastructure.Services;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    private ServicesProvider() { }
    public GameObject matchServiceGo;
    public GameObject loginServiceGo;
    public GameObject playServiceGo;

    private IMatchService _matchService;
    private ILoginService _loginService;
    private IPlayService _playService;

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

    internal IPlayService GetPlayService()
    {
        if (_playService != null)
            return _playService;
        var ls = GameObject.Find("PlayService");
        if (ls == null)
        {
            ls = Instantiate<GameObject>(playServiceGo);
            ls.name = "PlayService";
        }

        _playService = ls.GetComponent<IPlayService>();
        return _playService;
    }
}