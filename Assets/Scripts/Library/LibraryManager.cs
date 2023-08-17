using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class LibraryManager : MonoBehaviour
{
    public static LibraryManager instance;

    public Transform gameItemParent;
    public GameObject gameItemPrefab;

    public GameObject gameInformationWindow;
    public Text achievementCount;
    public Text playtime;

    private GameObject previousSelected;

    private List<GameObject> libraryCollection = new List<GameObject>();

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        SpawnItems();
        ResetSelection();
    }

    private void SpawnItems()
    {
        var gameNames = new List<string>(GameManager.instance.GetlibraryItems());
        gameNames.RemoveAt(gameNames.Count - 1);

        foreach (string gameName in gameNames)
        {
            Debug.Log("Checking for name: " + gameName);
            // Check if item exists in store. If yes replicate the item in library
            foreach (GameItem gameItem in StoreManager.instance.gameItems)
            {
                if (gameName == gameItem.gameName && !CheckDuplicate(gameName))
                {
                    GameObject spawnedGameItem = Instantiate(gameItemPrefab, gameItemParent);
                    spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(gameItem.gameLogo, gameName);
                    libraryCollection.Add(spawnedGameItem);
                }
            }            
        }    
    }

    private bool CheckDuplicate(string gameName)
    {
        foreach (GameObject game in libraryCollection)
        {
            if (gameName == game.name)
            {
                return true;
            }
        }
        return false;        
    }

    public void SelectGame(GameObject game)
    {
        // Change color of selected game and activate game information window
        gameInformationWindow.SetActive(true);
        previousSelected.GetComponent<LibraryItem>().background.color = new Color(0.16f, 0.16f, 0.16f);
        game.GetComponent<LibraryItem>().background.color = new Color(0.4f, 0.4f, 0.4f);
        

        // Load achievements and playtime
        if (PlayerPrefs.HasKey(game.name))
        {
            string[] gameInformation = PlayerPrefs.GetString(game.name).Split(";");
            achievementCount.text = gameInformation[0];
            playtime.text = "Playtime: " + gameInformation[0] + "hours.";
        }
        else
        {
            achievementCount.text = 0.ToString();
            playtime.text = "Playtime: 0 hours.";
        }

        previousSelected = game;
    }

    public void OnClickDelete()
    {
        List<string> games = PlayerPrefs.GetString("library").Split(";").ToList();
        if (games.Contains(previousSelected.name))
        {
            games.Remove(previousSelected.name);
            PlayerPrefs.SetString("library", String.Join(";", games));
        }       

        libraryCollection.Remove(previousSelected);
        Destroy(previousSelected);
        ResetSelection();       
    }

    private void ResetSelection()
    {
        if (libraryCollection.Count > 0)
        {
            previousSelected = libraryCollection[0];
            SelectGame(libraryCollection[0]);
        }
        else
        {
            gameInformationWindow.SetActive(false);
        }
    }

    public void SetGameButtons()
    {
        foreach (Game game in DataManager.Games)
        {
            if (game.IsDownloaded)
            {
                if (game.IsUpdated)
                {
                    Debug.Log("Is updated");
                    // Buttontext = Play
                }
                else
                {
                    Debug.Log("Isn't updated");
                    // Buttontext = Update
                }
            }
            else
            {
                // Buttontext = Download

                StartCoroutine(Installer.Download(game));
            }
        }
    }
}
