using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;

public static class Installer
{
    public static bool IsDownloading { get; private set; }

    private static string gamesPath;

    public static IEnumerator Download(Game game)
    {
        Debug.Log($"Downloading {game.Name}");
        IsDownloading = true;
        CreateFolders();
        yield return new WaitForSeconds(1f);
        
        using UnityWebRequest request = UnityWebRequest.Get(game.URL);

        // Downloading...

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(request.error);
        }
        else
        {
            // Download Zip-File
            string gameFolder = gamesPath + game.Name;
            string filePath = Path.Combine(gameFolder, $"{game.Name}.zip");

            if (!Directory.Exists(gameFolder))
            {
                Directory.CreateDirectory(gameFolder);
            }

            Debug.Log(filePath);
            byte[] zipData = request.downloadHandler.data;
            File.WriteAllBytes(filePath, zipData);

            // Extract Zip-File
            ZipFile.ExtractToDirectory(filePath, gameFolder);

            while (!File.Exists(Path.Combine(gameFolder, $"{game.Name}.exe")))
            {
                // Extracting...
            }

            // Delete Zip-File
            Directory.Delete(Path.GetDirectoryName(filePath), true);

            OnDownloadComplete(game);
        }
    }

    private static void OnDownloadComplete(Game game)
    {
        Debug.Log($"Successfully downloaded {game.Name}");

        game.IsDownloaded = true;
        game.IsUpdated = true;
        game.CurrentVersion = game.NewestVersion;

        // Store new game
        GameManager.instance.StoreLibraryItems();

        IsDownloading = false;
    }

    private static void CreateFolders()
    {
        // Create the Games Folder
        if (Application.isEditor)
        {
            gamesPath = Application.dataPath + "/Games";
            return;
        }
        gamesPath = Application.dataPath + "/../../Games/";

        if (!Directory.Exists(gamesPath))
        {
            Directory.CreateDirectory(gamesPath);
        }
    }
}
