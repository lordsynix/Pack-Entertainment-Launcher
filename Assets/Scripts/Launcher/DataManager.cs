using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class DataManager
{
    public static Dictionary<string, Game> Games = new();
    public static string LauncherURL;
    public static string LauncherVersion;

    private static readonly string URL = "https://onedrive.live.com/download?resid=5DC2466FF23494E4%21307183&authkey=!AI2NXp2LB61Ebf0";

    public static IEnumerator GetData()
    {
        // Internet connection?
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameManager.instance.errorHandler.OnError(1000);
            yield break;
        }

        // Web Request
        UnityWebRequest request = new(URL);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Get Data
            string[] data = request.downloadHandler.text.Split(' ');
            ProcessData(data);
        }
    }

    private static void ProcessData(string[] data)
    {
        CheckLauncherVersion(data[0]);

        var games = new List<string>(PlayerPrefs.GetString("library").Split(";"));
        var versions = new List<string>(PlayerPrefs.GetString("versions").Split(";"));

        games.RemoveAt(games.Count - 1);
        versions.RemoveAt(versions.Count - 1);

        Games = new();
        int index = 0;
        foreach (string gameName in games)
        {
            Debug.Log($"Looping through {gameName}");

            foreach (string d in data)
            {
                string[] gameData = d.Split(',');

                if (gameData[0] == gameName)
                {
                    string version = versions[index];
                    var newGame = new Game(gameData[0], gameData[2], version, gameData[1]);
                    Games.Add(gameName, newGame);
                    break;
                }
            }
            index++;
        }

        LibraryManager.instance.SpawnItems();
        LibraryManager.instance.ResetSelection();
    }

    private static void CheckLauncherVersion(string data)
    {
        string[] launcherData = data.Split(",");

        if (launcherData[0] == "PackEntertainmentLauncher")
        {
            LauncherVersion = launcherData[2];
            LauncherURL = launcherData[3];

            if (launcherData[4] == "true")
            {
                // Maintenance
                string[] details = { launcherData[5] };
                GameManager.instance.errorHandler.OnError(1002, details);
            }

            if (Application.version != launcherData[2])
            {
                // Invalid Version
                string[] details = { Application.version, launcherData[2] };
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
