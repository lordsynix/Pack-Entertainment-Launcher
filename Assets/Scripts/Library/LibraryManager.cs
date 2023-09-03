using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Diagnostics;
using System.IO;

public class LibraryManager : MonoBehaviour
{
    public static LibraryManager instance;

    public Transform gameItemParent;
    public GameObject gameItemPrefab;

    public GameObject gameInformationWindow;
    public Button gameButton;
    public Button deleteButton;
    public Text achievementCount;
    public Text playtime;

    private GameObject previousSelected;

    private Dictionary<string, GameObject> libraryDict = new();

    private void Awake()
    {
        instance = this;

        gameButton.onClick.AddListener(OnMainButton);
        deleteButton.onClick.AddListener(OnDeleteButton);
    }

    public void SpawnItems()
    {
        libraryDict.Clear();
        int childCount = gameItemParent.childCount;

        for (int i = childCount; i > 0; i--)
        {
            Destroy(gameItemParent.GetChild(i - 1).gameObject);
        }

        var libraryGames = DataManager.LibraryGames;

        foreach (var kvp in libraryGames)
        {
            string gameName = kvp.Key;

            // Check if item exists in store. If yes replicate the item in library
            foreach (GameItem gameItem in StoreManager.instance.gameItems)
            {
                if (gameName == gameItem.Name && !CheckDuplicate(gameName))
                {
                    GameObject spawnedGameItem = Instantiate(gameItemPrefab, gameItemParent);
                    spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(GameManager.instance.defaultLogo, gameName);

                    libraryDict.Add(gameName, spawnedGameItem);
                }
            }
        }
        ResetSelection();
    }

    private bool CheckDuplicate(string gameName)
    {
        foreach (var kvp in libraryDict)
        {
            if (gameName == kvp.Key)
            {
                return true;
            }
        }
        return false;        
    }

    public void SelectGame(GameObject game)
    {
        // Change color of selected game and activate game information window
        gameInformationWindow.SetActive(true);
        previousSelected.GetComponent<LibraryItem>().background.color = new Color(0.16f, 0.16f, 0.16f);
        game.GetComponent<LibraryItem>().background.color = new Color(0.4f, 0.4f, 0.4f);
        

        // Load achievements and playtime
        if (PlayerPrefs.HasKey(game.name))
        {
            string[] gameInformation = PlayerPrefs.GetString(game.name).Split(";");
            achievementCount.text = gameInformation[0];
            playtime.text = "Playtime: " + gameInformation[0] + "hours.";
        }
        else
        {
            achievementCount.text = 0.ToString();
            playtime.text = "Playtime: 0 hours.";
        }
        SetGameButtonText(game.name);

        previousSelected = game;
    }

    public void OnClickDelete()
    {
        DataManager.LibraryGames.Remove(previousSelected.name);
        libraryDict.Remove(previousSelected.name);
        Destroy(previousSelected);
        ResetSelection();       
    }

    public void ResetSelection()
    {
        if (libraryDict.Count > 0)
        {
            previousSelected = libraryDict.First().Value;
            SelectGame(libraryDict.First().Value);
        }
        else
        {
            gameInformationWindow.SetActive(false);
        }
    }

    public void SetGameButtonText(string gameName)
    {
        if (!DataManager.LibraryGames.ContainsKey(gameName)) return;

        var game = DataManager.LibraryGames[gameName];

        if (game.IsDownloaded)
        {
            if (game.IsUpdated)
            {
                gameButton.GetComponentInChildren<Text>().text = "Play";
                deleteButton.gameObject.SetActive(true);
            }
            else
            {
                gameButton.GetComponentInChildren<Text>().text = "Update";
                deleteButton.gameObject.SetActive(true);
            }
        }
        else
        {
            gameButton.GetComponentInChildren<Text>().text = "Download";
            deleteButton.gameObject.SetActive(false);
        }
    }

    public void OnMainButton()
    {
        Game game = DataManager.LibraryGames[previousSelected.name];

        if (game.IsDownloaded)
        {
            if (game.IsUpdated)
            {
                // Launch Game
                gameButton.GetComponentInChildren<Text>().text = "Running...";
                LaunchGame(game);
            }
            else
            {
                // Update Game
                gameButton.GetComponentInChildren<Text>().text = "Updating...";
                Installer.Delete(game, true); // Deletes and redownloads the game.
            }
        }
        else
        {
            // Download Game
            gameButton.GetComponentInChildren<Text>().text = "Downloading...";
            StartCoroutine(Installer.Download(game));
        }
    }

    public void OnDeleteButton()
    {
        Game game = DataManager.LibraryGames[previousSelected.name];

        Installer.Delete(game);
    }

    public void LaunchGame(Game game, string _exeFile = "")
    {
        string exeFile;
        if (string.IsNullOrEmpty(_exeFile))
        {
            exeFile = Path.Combine(Installer.CreateGamesFolder(), Path.Combine(game.Name, $"{game.Name}.exe"));
        }
        else
        {
            exeFile = _exeFile;
        }
        
        try
        {
            Process process = Process.Start(exeFile);
            // TODO get playtime when user quits application.
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogException(ex);
        }
    }
}
