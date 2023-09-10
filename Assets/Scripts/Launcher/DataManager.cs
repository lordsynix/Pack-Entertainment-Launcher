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

        // Get player's data keys
        List<string> keys = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();

        // Load player profile
        if (keys.Contains("Profile"))
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Profile" });
            LoadProfile(data["Profile"]);
        }
        else
        {
            LoadProfile("");
        }

        // Load player's games
        foreach (string key in keys)
        {
            if (key.Contains("Profile")) continue;

            // Load players game data
            Dictionary<string, string> data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });

            Game game = Game.FromJson(data[key]);
            
            LibraryGames.Add(game.Name, game);
            
            // TODO: Sync data from other devices
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

        Debug.Log("Store items loaded");
        StoreManager.instance.gameItems = gameItems.ToArray();
        StoreManager.instance.EnableCategory();

        ProcessData();
    }

    static void LoadProfile(string json)
    {
        Profile.FromJson(json);
    }

    public static async Task SaveLibraryGames()
    {
        // Store data in JSON format
        Dictionary<string, object> convertedDictionary = new();

        // Save profile
        Profile profile = ProfileManager.Instance.GetPlayerProfile();
        if (profile != null)
        {
            convertedDictionary.Add("Profile", profile);
        }
        else Debug.LogError("Player Profile is null");

        // Save games
        foreach(var kvp in LibraryGames)
        {
            convertedDictionary.Add(kvp.Key, kvp.Value);
        }
        await CloudSaveService.Instance.Data.ForceSaveAsync(convertedDictionary);
        Debug.Log("Profile and Games saved!");
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
                game.UpdateGame(storeGame.DownloadURL, storeGame.LatestVersion, null);
                LibraryGames[keyToUpdate] = game;
                keyToUpdate = "";
            }
        }

        Debug.Log("Library items loaded");
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
                return;
            }

            Installer.DeleteOldLauncherVersions();
        }
        else
        {
            Debug.LogError("Couldn't find launcher data");
        }
    }
}
