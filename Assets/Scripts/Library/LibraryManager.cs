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
    private List<GameObject> libraryCollection = new();

    private void Awake()
    {
        instance = this;

        gameButton.onClick.AddListener(OnMainButton);
        deleteButton.onClick.AddListener(OnDeleteButton);
    }

    public void SpawnItems()
    {
        var gameNames = new List<string>(GameManager.instance.GetlibraryItems());
        gameNames.RemoveAt(gameNames.Count - 1);

        foreach (string gameName in gameNames)
        {
            // Check if item exists in store. If yes replicate the item in library
            foreach (GameItem gameItem in StoreManager.instance.gameItems)
            {
                if (gameName == gameItem.gameName && !CheckDuplicate(gameName))
                {
                    GameObject spawnedGameItem = Instantiate(gameItemPrefab, gameItemParent);
                    spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(gameItem.gameLogo, gameName);

                    libraryDict.Add(gameName, spawnedGameItem);
                    libraryCollection.Add(spawnedGameItem);
                }
            }            
        }
        GameManager.instance.startingScreen.SetActive(false);
    }

    private bool CheckDuplicate(string gameName)
    {
        foreach (GameObject game in libraryCollection)
        {
            if (gameName == game.name)
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
        List<string> games = PlayerPrefs.GetString("library").Split(";").ToList();
        if (games.Contains(previousSelected.name))
        {
            games.Remove(previousSelected.name);
            PlayerPrefs.SetString("library", String.Join(";", games));
        }       

        libraryCollection.Remove(previousSelected);
        Destroy(previousSelected);
        ResetSelection();       
    }

    public void ResetSelection()
    {
        if (libraryCollection.Count > 0)
        {
            previousSelected = libraryCollection[0];
            SelectGame(libraryCollection[0]);
        }
        else
        {
            gameInformationWindow.SetActive(false);
        }
    }

    public void SetGameButtonText(string gameName)
    {
        if (!DataManager.Games.ContainsKey(gameName)) return;

        var game = DataManager.Games[gameName];

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
        Game game = DataManager.Games[previousSelected.name];

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
        Game game = DataManager.Games[previousSelected.name];

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
