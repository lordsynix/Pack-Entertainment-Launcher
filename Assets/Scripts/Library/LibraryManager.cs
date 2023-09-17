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
    public Button finalDeleteButton;
    public Button confirmDeleteButton;
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
        finalDeleteButton.onClick.AddListener(OnDeleteButton);
    }

    private void Update()
    {
        if (process != null)
        {
            if (process.HasExited)
            {
                process = null;

                // Calculate the total elapsed time
                float elapsedTimeInSeconds = Time.realtimeSinceStartup - startTime;

                int minutes = Mathf.FloorToInt(elapsedTimeInSeconds / 60);
                SetGameStats(activeGame, minutes);

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
                    Sprite logo = StoreManager.instance.gameSprites[gameName];
                    string name = StoreManager.instance.AddSpacesToCamelCase(gameName);
                    spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(logo, name, gameItem.Name);

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

    private void SetGameStats(Game game, int _minutes = -1)
    {
        double playtimeInMinutes = game.PlaytimeInMinutes;

        if (_minutes != -1)
        {
            playtimeInMinutes += _minutes;

            game.PlaytimeInMinutes = playtimeInMinutes;
            DataManager.LibraryGames[game.Name] = game;

            _ = DataManager.SaveLibraryGames();
        }

        int hours = Mathf.FloorToInt((float)playtimeInMinutes / 60);
        int minutes = (int)playtimeInMinutes - 60 * hours;

        playtime.text = $"{hours} hours and {minutes} minutes";
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
                gameButton.GetComponentInChildren<Text>().text = "PLAY";
                confirmDeleteButton.gameObject.SetActive(true);
            }
            else
            {
                gameButton.GetComponentInChildren<Text>().text = "UPDATE";
                confirmDeleteButton.gameObject.SetActive(true);
            }
        }
        else
        {
            gameButton.GetComponentInChildren<Text>().text = "DOWNLOAD";
            confirmDeleteButton.gameObject.SetActive(false);
        }
    }

    public void OnMainButton()
    {
        if (Installer.IsDownloading) return;

        Game game = DataManager.LibraryGames[previousSelected.name];

        if (game.IsDownloaded)
        {
            if (game.IsUpdated)
            {
                // Launch Game
                gameButton.GetComponentInChildren<Text>().text = "RUNNING...";
                LaunchGame(game);
            }
            else
            {
                // Update Game
                gameButton.GetComponentInChildren<Text>().text = "UPDATING...";
                Installer.Delete(game, true); // Deletes and redownloads the game.
            }
        }
        else
        {
            // Download Game
            gameButton.GetComponentInChildren<Text>().text = "DOWNLOADING...";
            StartCoroutine(Installer.Download(game));
        }
    }

    public void OnDeleteButton()
    {
        Game game = DataManager.LibraryGames[previousSelected.name];

        Installer.Delete(game);
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
            Game newGame = new()
            {
                Name = game.Name,
                URL = game.URL,
                LatestVersion = game.LatestVersion,
                IsDownloaded = false,
                IsUpdated = false,
                CurrentVersions = new()
            };

            foreach (Game.CurrentVersion kvp in game.CurrentVersions)
            {
                if (kvp.Key != PlayerPrefs.GetString("DeviceToken"))
                {
                    newGame.CurrentVersions.Add(kvp);
                }
                else
                {
                    Game.CurrentVersion currentVersion = new() { Key = kvp.Key, Value = "-1"};
                    newGame.CurrentVersions.Add(currentVersion);
                }
            }
            DataManager.LibraryGames[game.Name] = newGame;

            SpawnItems();
            SetLibraryButtonTextWithGameState(game.Name);
            GameManager.instance.errorHandler.OnError(1004, new string[] { game.Name });
            OnMainButton();
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
            GameManager.instance.errorHandler.OnError(1006);
        }
    }
}
