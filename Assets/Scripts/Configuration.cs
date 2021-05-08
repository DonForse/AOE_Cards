using System.Net.Http;

public static class Configuration
{
    public static readonly HttpClient client = new HttpClient();
    //public const string UrlBase = "https://gameofcardsaoeapi.somee.com/";
    public const string UrlBase = "https://aoegamecards.ar/";
    //public const string UrlBase = "https://localhost:44324";
//    public const string UrlBaseDebug = "http://localhost:44324";
    public const float TurnTimer = 60f;
    public const float LowTimer = 5f;
}