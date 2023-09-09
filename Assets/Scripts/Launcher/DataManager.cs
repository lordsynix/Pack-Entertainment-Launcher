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
            string deviceToken = PlayerPrefs.GetString("DeviceToken");
            Dictionary<string, string> data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { key });

            Game game = Game.FromJson(data[key]);

            if (PlayerPrefs.GetString("DeviceToken") == game.DeviceToken)
            {
                Debug.Log("Loaded game");
                LibraryGames.Add(game.Name, game);
            } // TODO: Sync data from other devices
        }

        // Create player profile
        if (keys.Contains("Profile"))
        {
            var data = await CloudSaveService.Instance.Data.LoadAsync(new HashSet<string> { "Profile" });
            LoadProfile(data["Profile"]);
        }
        else
        {
            LoadProfile("");
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

    static void LoadProfile(string json)
    {
        Profile.FromJson(json);
    }

    public static async Task SaveLibraryGames()
    {
        List<string> keys = await CloudSaveService.Instance.Data.RetrieveAllKeysAsync();
        string deviceToken = PlayerPrefs.GetString("DeviceToken");

        // Delete cloud game informations of this device
        foreach (var key in keys)
        {
            if (key.Contains(deviceToken))
            {
                await CloudSaveService.Instance.Data.ForceDeleteAsync(key);
            }
        }

        // Save cloud game informations of this device
        Dictionary<string, object> convertedDictionary = new();

        Profile profile = ProfileManager.Instance.GetPlayerProfile();
        if (profile != null)
        {
            convertedDictionary.Add("Profile", profile);
        }
        else Debug.LogError("Player Profile is null");

        foreach(var kvp in LibraryGames)
        {
            convertedDictionary.Add(kvp.Key + '_' + deviceToken, kvp.Value);
        }
        await CloudSaveService.Instance.Data.ForceSaveAsync(convertedDictionary);
        Debug.Log("Profile and Games information saved!");
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
