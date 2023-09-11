using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Game
{
    public string Name;
    public string URL;
    public List<CurrentVersion> CurrentVersions;
    public string LatestVersion;
    
    public bool IsDownloaded;
    public bool IsUpdated;

    public double PlaytimeInMinutes;

    [Serializable]
    public class CurrentVersion
    {
        public string Key;
        public string Value;
    }

    // TODO Add Achievements, Stats and Playtime

    public string GetCurrentVersion()
    {
        string currentVersion = "";

        if (CurrentVersions.Count == 0)
        {
            Debug.LogError("Current Versions are empty");
            return null;
        }

        foreach (var kvp in CurrentVersions)
        {
            if (kvp.Key == PlayerPrefs.GetString("DeviceToken")) 
            {
                currentVersion = kvp.Value;
            }
        }

        if (string.IsNullOrEmpty(currentVersion))
        {
            // Game doesn't exists on this device
            return "-1";
        }

        return currentVersion;
    }

    public void UpdateCurrentVersion(string version)
    {
        string deviceToken = PlayerPrefs.GetString("DeviceToken");
        CurrentVersion currentVersion = new() { Key = deviceToken, Value = version };

        for (int i = 0; i < CurrentVersions.Count; i++)
        {
            var kvp = CurrentVersions[i];
            if (kvp.Key == deviceToken)
            {
                CurrentVersions[i] = currentVersion;
            }
        }
        if (CurrentVersions.Count == 0)
        {
            CurrentVersions.Add(currentVersion);
        }
    }

    public void UpdateGame(string url, string latestVersion, string _currentVersion)
    {
        URL = url;
        LatestVersion = latestVersion;
        string currentVersion;

        if (string.IsNullOrEmpty(_currentVersion) )
        {
            currentVersion = GetCurrentVersion();
        }
        else
        {
            UpdateCurrentVersion(_currentVersion);
            currentVersion = _currentVersion;
        }

        if (currentVersion == LatestVersion)
        {
            IsDownloaded = true;
            IsUpdated = true;
        }
        else if (currentVersion != "-1")
        {
            IsDownloaded = true;
            IsUpdated = false;
        }
        else
        {
            IsDownloaded = false;
            IsUpdated = false;
        }
    }

    public static Game FromJson(string json)
    {
        return JsonUtility.FromJson<Game>(json);
    }
}
