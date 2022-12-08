using UnityEngine;

namespace Features
{
    internal class GameManager
    {
        internal static void SessionExpired()
        {
            PlayerPrefs.DeleteAll();

            Application.Quit();
        }
    }
}
