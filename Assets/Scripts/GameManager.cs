using System;
using UnityEngine;
internal class GameManager
{
    internal static void SessionExpired()
    {
        PlayerPrefs.DeleteAll();

        Application.Quit();
    }
}
