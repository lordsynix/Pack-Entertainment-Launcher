using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading.Tasks;
using Unity.Services.CloudSave;
using Unity.Services.RemoteConfig;

public static class DataManager
{
    public static Dictionary<string, Game> LibraryGames = new();
    public static string LauncherURL;
    public static string LauncherVersion;

    public struct userAttributes { }
    public struct appAttributes { }

    public static async Task GetPlayerData()
    {
        // Internet connection?
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.instance.errorHandler.OnError(1000);
            return;
        }

        // Get all games the player has
        List<string> keys = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();

        foreach (string key in keys)
        {
            if (key == "SignUpDate") continue;
            Dictionary<string, string> data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });
            Game game = Game.FromJson(data[key]);
            LibraryGames.Add(game.Name, game);
        }

        // Get all games from remote config
        RemoteConfigService.Instance.FetchCompleted += ApplyRemoteSettings;
        RemoteConfigService.Instance.FetchConfigs(new userAttributes(), new appAttributes());

    }

    static void ApplyRemoteSettings(ConfigResponse configResponse)
    {
        var data = RemoteConfigService.Instance.appConfig.config;
        List<GameItem> gameItems = new();

        foreach (var kvp in data)
        {
            var jsonValue = kvp.Value;

            if (jsonValue != null)
            {
                string json = jsonValue.ToString();

                GameItem item = JsonUtility.FromJson<GameItem>(json);

                if (item != null && item.Name != "PackEntertainmentLauncher")
                {
                    gameItems.Add(item);
                }
                else
                {
                    CheckLauncherVersion(item);
                }
            }
        }

        StoreManager.instance.gameItems = gameItems.ToArray();
        StoreManager.instance.EnableCategory();

        ProcessData();
    }

    public static async Task SaveLibraryGames()
    {
        Dictionary<string, object> convertedDictionary = new();

        foreach(var kvp in LibraryGames)
        {
            convertedDictionary.Add(kvp.Key, kvp.Value);
        }

        await CloudSaveService.Instance.Data.ForceSaveAsync(convertedDictionary);
    }

    private static void ProcessData()
    {
        if (StoreManager.instance.gameItems.Length == 0)
        {
            Debug.LogError("Config data request failed");
            return;
        }

        // Check Launcher
        string keyToUpdate = "";
        foreach (GameItem storeGame in StoreManager.instance.gameItems)
        {
            Debug.Log($"Looping through {storeGame.Name}");

            foreach (var kvp in LibraryGames)
            {
                Game libraryGame = kvp.Value;

                if (libraryGame.Name == storeGame.Name)
                {
                    // Update the latest version and url with the config data
                    keyToUpdate = kvp.Key;
                }
            }
            if (!string.IsNullOrEmpty(keyToUpdate))
            {
                Game game = LibraryGames[keyToUpdate];
                game.UpdateGame(storeGame.DownloadURL, storeGame.LatestVersion);
                LibraryGames[keyToUpdate] = game;
                keyToUpdate = "";
            }
        }

        LibraryManager.instance.SpawnItems();

        GameManager.instance.startingScreen.SetActive(false);
    }

    private static void CheckLauncherVersion(GameItem launcher)
    {
        if (launcher.Name == "PackEntertainmentLauncher")
        {
            LauncherVersion = launcher.LatestVersion;
            LauncherURL = launcher.DownloadURL;

            // Maintenance (Error Code 1002)

            if (Application.version != LauncherVersion)
            {
                // Invalid Version
                string[] details = { Application.version, LauncherVersion };
                GameManager.instance.errorHandler.OnError(1001, details);
            }

            Installer.DeleteOldLauncherVersions();
        }
        else
        {
            Debug.LogError("Couldn't find launcher data");
        }
    }
}
