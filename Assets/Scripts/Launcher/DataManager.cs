using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public static class DataManager
{
    public static List<Game> Games;

    private static readonly string URL = "https://onedrive.live.com/download?resid=5DC2466FF23494E4%21307183&authkey=!AI2NXp2LB61Ebf0";
    
    public static IEnumerator GetData()
    {
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
        var games = new List<string>(PlayerPrefs.GetString("library").Split(";"));
        var versions = new List<string>(PlayerPrefs.GetString("versions").Split(";"));

        games.RemoveAt(games.Count - 1);
        versions.RemoveAt(versions.Count - 1);

        Games = new();
        int index = 0;
        foreach (string game in games)
        {
            Debug.Log($"Looping through {game}");
            foreach (string d in data)
            {
                string[] gameData = d.Split(',');

                if (gameData[0] == game)
                {
                    string version = versions[index];
                    var newGame = new Game(gameData[0], gameData[2], version, gameData[1]);
                    Games.Add(newGame);
                    break;
                }
            }
            index++;
        }

        if (games.Count != 0)
        {
            LibraryManager.instance.SetGameButtons();
        }
    }
}
