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

    public static string gamesPath;

    public static IEnumerator Download(Game game, bool launcher = false)
    {
        if (!launcher) Debug.Log($"Downloading {game.Name}");
        IsDownloading = true;
        CreateGamesFolder();
        yield return new WaitForSeconds(1f);

        var url = launcher ? DataManager.LauncherURL : game.URL;
        using UnityWebRequest request = UnityWebRequest.Get(url);

        // Downloading...

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Download Zip-File
            byte[] zipData = request.downloadHandler.data;

            ProcessDownload(game, launcher, zipData);
        }
    }

    private static void ProcessDownload(Game game, bool launcher, byte[] zipData)
    {
        var version = DataManager.LauncherVersion;
        string folderPath = launcher ? Path.Combine(gamesPath + "/../", $"Launcher-{version}") :  Path.Combine(gamesPath, game.Name);
        string filePath = launcher ? Path.Combine(folderPath, $"Launcher-{version}.zip") : Path.Combine(folderPath, $"{game.Name}.zip");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
            Debug.Log($"Created Folder at {folderPath}");
        }

        File.WriteAllBytes(filePath, zipData);
        Debug.Log($"Stored ZipFile at: {filePath}");

        // Extract Zip-File
        LibraryManager.instance.SetLibraryButton("Extracting...");
        ZipFile.ExtractToDirectory(filePath, folderPath);

        string exeFileLauncher = Path.Combine(folderPath, "Launcher.exe");
        string exeFileGame = Path.Combine(folderPath, $"{game.Name}.exe");
        while (!File.Exists(launcher ? exeFileLauncher : exeFileGame))
        {
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
            GameManager.instance.errorHandler.OnError(1003);
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
        game.CurrentVersion = game.LatestVersion;

        // Store new game
        DataManager.LibraryGames[game.Name] = game;
        _ = DataManager.SaveLibraryGames();

        IsDownloading = false;
        LibraryManager.instance.SetLibraryButtonTextWithGameState(game.Name);
    }

    private static void OnLauncherDownloaded(string exeFile)
    {
        LibraryManager.instance.LaunchGame(null, exeFile);
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

                LibraryManager.instance.OnDeletionCompleted(game);
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

            Download(game);
        }
    }

    public static void DeleteOldLauncherVersions()
    {
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
                        GameManager.instance.errorHandler.OnError(1003);
                    }
                }
            }
        }
    }
}
