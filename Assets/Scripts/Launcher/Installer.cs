using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using Unity.VisualScripting;

public static class Installer
{
    public static bool IsDownloading { get; private set; }
    private static bool reDownlaodingGame = false;

    public static string gamesPath;

    public static IEnumerator Download(Game game, bool launcher = false)
    {
        if (IsDownloading) yield break;

        if (!launcher) Debug.Log($"Downloading {game.Name}");
        else GameManager.instance.ChangeUpdateState("Initialize Update...");
        IsDownloading = true;
        _ = CreateGamesFolder();
        
        var url = launcher ? DataManager.LauncherURL : game.URL;
        using UnityWebRequest request = UnityWebRequest.Get(url);

        // Downloading...

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
            if (launcher) GameManager.instance.ChangeUpdateState("Update failed");
            IsDownloading = false;
        }
        else
        {
            // Download Zip-File
            byte[] zipData = request.downloadHandler.data;
            if (launcher) GameManager.instance.ChangeUpdateState("Processing Download...");

            ProcessDownload(game, launcher, zipData);
        }
    }

    private static void ProcessDownload(Game game, bool launcher, byte[] zipData)
    {
        var version = DataManager.LauncherVersion;
        string folderPath = launcher ? Path.Combine(gamesPath + "../", $"Launcher-{version}") :  Path.Combine(gamesPath, game.Name);
        string filePath = launcher ? Path.Combine(folderPath, $"Launcher-{version}.zip") : Path.Combine(folderPath, $"{game.Name}.zip");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Created Folder at {folderPath}");
        }

        File.WriteAllBytes(filePath, zipData);
        Debug.Log($"Stored ZipFile at: {filePath}");

        try
        {
            // Extract Zip-File
            if (!launcher) LibraryManager.instance.SetLibraryButton("EXTRACTING...");
            else GameManager.instance.ChangeUpdateState("Extracting data...");
            ZipFile.ExtractToDirectory(filePath, folderPath);
        } 
        catch
        {
            // Extraction failed
            // Error 1005 - Auto solve with redownload
            Directory.Delete(folderPath, true);
            IsDownloading = false;
            LibraryManager.instance.confirmDeleteButton.gameObject.SetActive(false);
            LibraryManager.instance.SetLibraryButton("DOWNLOAD");

            if (!reDownlaodingGame)
            {
                if (!launcher) LibraryManager.instance.OnMainButton();
                else GameManager.instance.ChangeUpdateState("Couldn't extract file...");
                reDownlaodingGame = true;
            }
            else
            {
                var details = new string[] { game.Name };
                GameManager.instance.errorHandler.OnError(1005, details);
                reDownlaodingGame = false;
            }

            return;
        }
        
        string exeFileLauncher = "";
        string exeFileGame = "";

        if (launcher)
        {
            exeFileLauncher = Path.Combine(folderPath, "Pack Launcher.exe");
        }
        else
        {
            exeFileGame = Path.Combine(folderPath, $"{game.Name}.exe");
        }

        while (!File.Exists(launcher ? exeFileLauncher : exeFileGame))
        {
            if (launcher) GameManager.instance.ChangeUpdateState("Extracting...");
            // Extracting...
        }

        // Delete Zip-File // Access might be denied
        try
        {
            File.Delete(filePath);
        } 
        catch (Exception ex)
        {
            Debug.LogException(ex);
            Debug.LogError(ex.ToString());
            if (!launcher) GameManager.instance.errorHandler.OnError(1003);
            else GameManager.instance.ChangeUpdateState("Couldn't delete file...");
        }

        if (launcher)
        {
            OnLauncherDownloaded(exeFileLauncher);
        }
        else
        {
            OnGameDownloaded(game);
        }
    }

    private static void OnGameDownloaded(Game game)
    {
        Debug.Log($"Successfully downloaded {game.Name}");

        game.IsDownloaded = true;
        game.IsUpdated = true;
        game.UpdateCurrentVersion(game.LatestVersion);

        // Store new game
        DataManager.LibraryGames[game.Name] = game;
        _ = DataManager.SaveLibraryGames();

        IsDownloading = false;
        LibraryManager.instance.SetLibraryButtonTextWithGameState(game.Name);
    }

    private static void OnLauncherDownloaded(string exeFile)
    {
        GameManager.instance.ChangeUpdateState("Starting launcher...");
        LibraryManager.instance.LaunchLauncher(exeFile);
    }

    public static string CreateGamesFolder()
    {
        // Create the Games Folder
        if (Application.isEditor)
        {
            gamesPath = Application.dataPath + "/Games";
            Debug.Log($"Games Folder {gamesPath}");
        }
        else
        {
            gamesPath = Application.dataPath + "/../../Games/";
            Debug.Log($"Games Folder {gamesPath}");
        }

        if (!Directory.Exists(gamesPath))
        {
            Directory.CreateDirectory(gamesPath);
        }

        return gamesPath;
    }

    public static void Delete(Game game, bool downloadGame = false)
    {
        gamesPath = Application.dataPath + "/../../Games/";
        if (!Directory.Exists(gamesPath)) return;

        string folderPath = Path.Combine(gamesPath, game.Name);
        
        if (Directory.Exists(folderPath))
        {
            try
            {
                Directory.Delete(folderPath, true);

                DataManager.LibraryGames.Remove(game.Name);
                _ = DataManager.SaveLibraryGames();

                if (!downloadGame) OnDeleteCompleted(game);
            }
            catch (Exception ex)
            {
                // Access denied
                Debug.LogError(ex.ToString());
                GameManager.instance.errorHandler.OnError(1003);
            }
        }
        else
        {
            Debug.LogError($"Couldn't find game at: {folderPath}");
            return;
        }

        if (downloadGame)
        {
            while (Directory.Exists(folderPath))
            {
                // Deleting...
            }

            GameManager.instance.StartCoroutine(Download(game));
        }
    }

    private static void OnDeleteCompleted(Game game)
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
                Game.CurrentVersion currentVersion = new() { Key = kvp.Key, Value = "-1" };
                newGame.CurrentVersions.Add(currentVersion);
            }
        }
        DataManager.LibraryGames[game.Name] = newGame; 
        LibraryManager.instance.SetLibraryButtonTextWithGameState(game.Name);
        _ = DataManager.SaveLibraryGames();
    }

    public static void DeleteOldLauncherVersions()
    {
        // Only delete older versions if application is the newest version
        if (Application.version != DataManager.LauncherVersion) return;

        string launchersFolder = Application.dataPath + "/../../";
        string[] subfolders = Directory.GetDirectories(launchersFolder);
        
        foreach (string subfolder in subfolders)
        {
            if (Path.GetFileName(subfolder).Contains("Launcher-"))
            {
                if (Path.GetFileName(subfolder) != "Launcher-" + DataManager.LauncherVersion)
                {
                    try
                    {
                        // Delete old launcher version
                        Directory.Delete(subfolder, true);
                        Debug.Log($"Deleting old launcher version at: {subfolder}");
                    }
                    catch (Exception ex)
                    {
                        // Access denied
                        Debug.LogError(ex.ToString());
                    }
                }
            }
        }
    }
}
