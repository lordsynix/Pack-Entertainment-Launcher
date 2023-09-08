using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject updateScreen;
    public GameObject startingScreen;
    public ErrorHandler errorHandler;

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
                    CurrentVersion = version
                };
                game.UpdateGame(gameItem.DownloadURL, gameItem.LatestVersion);

                DataManager.LibraryGames.Add(gameName, game);
            }
        }
        LibraryManager.instance.SpawnItems();
    }

    private async void OnApplicationQuit()
    {
        Debug.Log("Quiting...");
        await DataManager.SaveLibraryGames();
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void ChangeUpdateState(string text)
    {
        updateStateText.text = text;
    }
}
