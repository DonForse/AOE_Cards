using System;
using Infrastructure.Services;
using UnityEngine;

public class ServicesProvider : MonoBehaviour
{
    private ServicesProvider() { }
    public GameObject matchServiceGo;

    private IMatchService _matchService;

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
            
        _matchService = ms.GetComponent<MatchService>();
        return _matchService;
    }
}