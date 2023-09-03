using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Game
{
    public string Name;
    public string URL;
    public string CurrentVersion;
    public string LatestVersion;
    
    public bool IsDownloaded;
    public bool IsUpdated;

    // TODO Add Achievements, Stats and Playtime

    public void UpdateGame(string url, string latestVersion)
    {
        URL = url;
        LatestVersion = latestVersion;

        if (CurrentVersion == LatestVersion)
        {
            IsDownloaded = true;
            IsUpdated = true;
        }
        else if (CurrentVersion != "-1")
        {
            IsDownloaded = true;
            IsUpdated = false;
        }
        else
        {
            IsDownloaded = false;
            IsUpdated = false;
        }

        Debug.Log($"Updated {Name}! Downloaded: {IsDownloaded} Updated: {IsUpdated}");
    }

    public static Game FromJson(string json)
    {
        return JsonUtility.FromJson<Game>(json);
    }
}
