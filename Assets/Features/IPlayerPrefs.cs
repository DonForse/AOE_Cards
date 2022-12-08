namespace Features
{
    public interface IPlayerPrefs
    {
        void SetString(string key, string value);
        string GetString(string key);
        void Save();
    }
}