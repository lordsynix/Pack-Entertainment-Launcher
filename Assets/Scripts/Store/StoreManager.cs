using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public static StoreManager instance;

    public GameObject store;
    public GameObject[] categories;
    public GameItem[] gameItems;
    public GameObject gameItemPrefab;

    private GameObject previousCategory;
    public Text categoryTag;

    public Dictionary<string, Sprite> gameSprites;
    public List<Sprite> gameLogos;

    private List<GameObject> spawnedGameItems = new();

    private void Awake()
    {
        instance = this;

        if (gameLogos.Count != 5)
        {
            Debug.LogError("Please assign all game logo sprites");
            return;
        }

        gameSprites = new()
        {
            {"Smut", gameLogos[0] },
            {"ChessEngine", gameLogos[1] },
            {"SpaceBattlefield", gameLogos[2] },
            {"CavernEscape", gameLogos[3] },
            {"TheAce", gameLogos[4] }
        };
    }

    public void OnEnable()
    {
        OnClickCategory(categories[0]);
    }

    public void EnableCategory()
    {
        foreach (GameObject category in categories)
        {
            if (category.activeInHierarchy == true)
            {
                previousCategory = category.gameObject;
                ReloadCategory(category.gameObject);
            }
        }
    }

    public void OnClickCategory(GameObject activeCategory)
    {
        categoryTag.text = activeCategory.name;
        if (previousCategory != null)
        {
            previousCategory.SetActive(false);
        }
        activeCategory.SetActive(true);
        previousCategory = activeCategory;

        ReloadCategory(activeCategory);
    }

    private void ReloadCategory(GameObject activeCategory)
    {
        // Empty current category
        foreach (GameObject spawnedGameItem in spawnedGameItems)
        {
            Destroy(spawnedGameItem);
        }
        spawnedGameItems.Clear();

        SetGamesInCategory(activeCategory);
    }

    public void SetGamesInCategory(GameObject activeCategory)
    {
        // Search all game items in specified category and spawn them
        foreach (GameItem gameItem in gameItems)
        {
            string[] categories = gameItem.Genres;
            foreach (string category in categories)
            {
                if (activeCategory.name == category)
                {
                    SpawnItem(gameItem, activeCategory);
                }
            }
        }
    }

    private void SpawnItem(GameItem gameItem, GameObject category)
    {
        GameObject spawnedGameItem = Instantiate(gameItemPrefab, category.transform.GetChild(0).GetChild(0).GetChild(0));
        string gameName = AddSpacesToCamelCase(gameItem.Name);
        spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(gameSprites[gameItem.Name], gameName, gameItem.Name);

        // Check if item has already been added to library. If yes then disable add button.
        if (DataManager.LibraryGames.ContainsKey(gameItem.Name))
        {
            spawnedGameItem.GetComponent<StoreItem>().addButton.SetActive(false);
        }
        spawnedGameItems.Add(spawnedGameItem);
    }

    public string AddSpacesToCamelCase(string input)
    {
        // Verwende eine regulaere Ausdrucksübereinstimmung, um Großbuchstaben zu finden und Leerzeichen einzufügen.
        string pattern = "(?<=[a-z])(?=[A-Z])"; // Suche nach Übergängen von Klein- zu Großbuchstaben
        string replacement = " "; // Ersetze den Übergang durch ein Leerzeichen

        string formattedString = Regex.Replace(input, pattern, replacement);
        return formattedString;
    }
}
