using System;
using Infrastructure.Services;

public class ServicesProvider
{
    private ServicesProvider() { }
    public static readonly ServicesProvider Instance = new ServicesProvider();
    private readonly Lazy<IMatchService> _matchService = new Lazy<IMatchService>(() => new MatchService());

    public IMatchService GetMatchService()
    {
        return _matchService.Value;
    }
}