using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void AddGameToLibrary(string gameName)
    {
        string libraryString = PlayerPrefs.GetString("library");
        PlayerPrefs.SetString("library", libraryString + gameName + ";");
    }

    public string[] GetlibraryItems()
    {
        return PlayerPrefs.GetString("library").Split(";");
    }
}
