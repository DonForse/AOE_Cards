using UnityEngine;

namespace Features
{
    public class PlayerPrefsWrapper : IPlayerPrefs
    {
        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);

        public string GetString(string key) => PlayerPrefs.GetString(key);

        public void Save() => PlayerPrefs.Save();
    }
}