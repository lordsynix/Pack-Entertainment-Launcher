using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject startingScreen;
    public ErrorHandler errorHandler;

    private void Awake()
    {
        Debug.LogError("Open console");
        instance = this;

        StartCoroutine(DataManager.GetData());
    }

    public void AddGameToLibrary(string gameName, string version = "-1")
    {
        Debug.Log($"Added {gameName} to Library");
        string libraryString = PlayerPrefs.GetString("library");
        PlayerPrefs.SetString("library", libraryString + gameName + ";");

        string versionString = PlayerPrefs.GetString("versions");
        PlayerPrefs.SetString("versions", versionString + version + ";");

        StartCoroutine(DataManager.GetData());
    }

    public string[] GetlibraryItems()
    {
        return PlayerPrefs.GetString("library").Split(";");
    }

    private void OnApplicationQuit()
    {
        Debug.Log("Quiting...");
        StoreLibraryItems();
    }

    public void StoreLibraryItems()
    {
        string libraryString = "";
        string versionString = "";

        if (DataManager.Games.Count > 0)
        {
            foreach (KeyValuePair<string, Game> game in DataManager.Games)
            {
                libraryString += game.Value.Name + ";";
                versionString += game.Value.CurrentVersion + ";";
            }

            PlayerPrefs.SetString("library", libraryString);
            PlayerPrefs.SetString("versions", versionString);
        }
    }
}
