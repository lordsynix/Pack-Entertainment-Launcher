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
    private Process process;
    private Game activeGame;
    private float startTime;

    private Dictionary<string, GameObject> libraryDict = new();

    private void Awake()
    {
        instance = this;

        gameButton.onClick.AddListener(OnMainButton);
        deleteButton.onClick.AddListener(OnDeleteButton);
    }

    private void Update()
    {
        if (process != null)
        {
            if (process.HasExited)
            {
                process = null;

                // Calculate the total elapsed time
                double playtime = activeGame.PlaytimeInMinutes;
                float elapsedTimeInSeconds = Time.realtimeSinceStartup - startTime;

                int hours = Mathf.FloorToInt(elapsedTimeInSeconds / 3600);
                int minutes = Mathf.FloorToInt((elapsedTimeInSeconds - hours *  3600) / 60);
                SetGameStats(activeGame, hours, minutes);

                SetLibraryButtonTextWithGameState(activeGame.Name);
            }
        }
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
        SetGameStats(DataManager.LibraryGames[game.name]);
        SetLibraryButtonTextWithGameState(game.name);

        previousSelected = game;
    }

    private void SetGameStats(Game game, int _hours = -1, int _minutes = -1)
    {
        if (_hours == -1 && _minutes == -1)
        {
            int hours = Mathf.FloorToInt((float)game.PlaytimeInMinutes / 60);
            int minutes = Mathf.FloorToInt(((float)game.PlaytimeInMinutes - hours * 60) / 60);

            playtime.text = $"Playtime: {hours} hours and {minutes} minutes";
        }
        else
        {
            int hours = Mathf.FloorToInt((float)game.PlaytimeInMinutes / 60) + _hours;
            int minutes = Mathf.FloorToInt(((float)game.PlaytimeInMinutes - hours * 60) / 60) + _minutes;
            if (minutes > 59)
            {
                hours++;
                minutes -= 60;
            }
            playtime.text = $"Playtime: {hours} hours and {minutes} minutes";
            game.PlaytimeInMinutes = hours * 60 + minutes;
            DataManager.LibraryGames[game.Name] = game;
        }

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

    public void SetLibraryButton(string text)
    {
        gameButton.GetComponentInChildren<Text>().text = text;
    }

    public void SetLibraryButtonTextWithGameState(string gameName)
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

    public void OnDeletionCompleted(Game game)
    {
        libraryDict.Remove(game.Name);
        Destroy(previousSelected);
        ResetSelection();
        StoreManager.instance.EnableCategory();
    }

    public void LaunchGame(Game game)
    {
        if (game == null) return;

        string exeFile = Path.Combine(Installer.CreateGamesFolder(), Path.Combine(game.Name, $"{game.Name}.exe"));
        
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exeFile,
                Arguments = $"-username {PlayerPrefs.GetString("username")} -password {PlayerPrefs.GetString("password")}"
            };

            process = Process.Start(startInfo);
            activeGame = game;
            startTime = Time.realtimeSinceStartup;

            // TODO get playtime when user quits application.
        }
        catch
        {
            string[] details;
            if (game != null) details = new string[] { game.Name };
            else details = new string[] { "Launcher" };
            GameManager.instance.errorHandler.OnError(1004, details);
        }
    }

    public void LaunchLauncher(string exeFile)
    {
        try
        {
            ProcessStartInfo startInfo = new()
            {
                FileName = exeFile,
                Arguments = $"-username {PlayerPrefs.GetString("username")} -password {PlayerPrefs.GetString("password")}"
            };

            process = Process.Start(startInfo);
            Application.Quit();
        }
        catch
        {
            var details = new string[] { "Launcher" };
            GameManager.instance.errorHandler.OnError(1004, details);
        }
    }
}
