using System;
using Infrastructure.Services;
using Login.Scripts.Infrastructure;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    private ServicesProvider() { }
    public GameObject matchServiceGo;
    public GameObject loginServiceGo;
    public GameObject playServiceGo;
    public GameObject tokenServiceGo;

    private IMatchService _matchService;
    private ILoginService _loginService;
    private IPlayService _playService;
    private ITokenService _tokenService;

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

    internal ITokenService GetTokenService()
    {
        if (_tokenService != null)
            return _tokenService;
        var ls = GameObject.Find("TokenService");
        if (ls == null)
        {
            ls = Instantiate<GameObject>(tokenServiceGo);
            ls.name = "TokenService";
        }

        _tokenService = ls.GetComponent<ITokenService>();
        return _tokenService;
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