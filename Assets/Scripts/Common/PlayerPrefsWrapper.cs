using UnityEngine;

namespace Common
{
    public class PlayerPrefsWrapper : IPlayerPrefs
    {
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public string GetString(string key) => PlayerPrefs.GetString(key);

        public void Save() => PlayerPrefs.Save();
    }

    public interface IPlayerPrefs
    {
        void SetString(string key, string value);
        string GetString(string key);
        void Save();
    }
}