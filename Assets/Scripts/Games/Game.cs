using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game
{
    public string Name;
    public string URL;
    public string CurrentVersion;
    public string NewestVersion;

    public bool IsDownloaded;
    public bool IsUpdated;

    public Game(string name, string url, string currentVersion = null, string newestVersion = null)
    {
        Name = name;
        URL = url;
        CurrentVersion = currentVersion;
        NewestVersion = newestVersion;

        if (!string.IsNullOrEmpty(NewestVersion) && CurrentVersion == NewestVersion)
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
        Debug.Log($"Created {Name}! CurVersion: {CurrentVersion} NewVersion: {NewestVersion} " +
            $"Downloaded: {IsDownloaded} Updated: {IsUpdated} URL: {URL}");
    }
}
