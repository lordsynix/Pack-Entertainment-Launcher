using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Screens")]
    public GameObject updateScreen;
    public GameObject startingScreen;
    public GameObject quitingScreen;
    public ErrorHandler errorHandler;

    [Header("Windows")]
    public GameObject libraryWindow;
    public GameObject storeWindow;
    public GameObject profileWindow;

    public Text updateStateText;

    public Sprite defaultLogo;

    private async void Awake()
    {
        instance = this;
        
        await DataManager.GetPlayerData();
    }

    public void AddGameToLibrary(string gameName, string version = "-1")
    {
        foreach (GameItem gameItem in StoreManager.instance.gameItems)
        {
            if (gameName == gameItem.Name)
            {
                Game game = new()
                {
                    Name = gameName,
                    CurrentVersions = new()
                };
                game.UpdateGame(gameItem.DownloadURL, gameItem.LatestVersion, version);

                Debug.Log($"Added {gameName} to library");
                DataManager.LibraryGames.Add(gameName, game);
            }
        }
        LibraryManager.instance.SpawnItems();
        ProfileManager.Instance.SetProfileInformation();
        _ = DataManager.SaveLibraryGames();
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("Quiting...");
        quitingScreen.SetActive(true);
        await DataManager.SaveLibraryGames(true);
    }

    public void Quit()
    {
        OnApplicationQuit();
    }

    public void ChangeUpdateState(string text)
    {
        updateStateText.text = text;
    }

    public void ActivateWindow(string s)
    {
        char c = s.ToCharArray()[0];

        if (c == 'S') StoreManager.instance.OnEnable();
        else if (c == 'P') ProfileManager.Instance.SetPlayerProfile(ProfileManager.Instance.GetPlayerProfile());

        libraryWindow.SetActive(c == 'L');
        storeWindow.SetActive(c == 'S');
        profileWindow.SetActive(c == 'P');
    }
}
