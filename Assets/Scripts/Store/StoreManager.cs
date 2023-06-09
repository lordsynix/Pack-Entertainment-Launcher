using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public GameItem[] gameItems;
    public GameObject gameItemParent;
    public GameObject gameItemPrefab;

    private GameObject previousCategory;
    public Text categoryTag;

    private List<GameObject> spawnedGameItems = new List<GameObject>();

    private void OnEnable()
    {
        HorizontalLayoutGroup[] categories = GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup category in categories)
        {
            if (category.enabled == true)
            {
                previousCategory = category.gameObject;
                ReloadCategory(category.gameObject);
            }
        }
    }

    public void OnClickCategory(GameObject activeCategory)
    {
        categoryTag.text = activeCategory.name;
        previousCategory.SetActive(false);
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

        // Search all game items in specified category and spawn them
        foreach (GameItem gameItem in gameItems)
        {
            string[] categories = gameItem.genres;
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
        GameObject spawnedGameItem = Instantiate(gameItemPrefab, category.transform);
        spawnedGameItem.GetComponent<ItemConstructor>().ConstructWithData(gameItem.gameLogo, gameItem.gameName);
        
        // Check if item has already been added to library. If yes then disable add button.
        if (PlayerPrefs.GetString("library").Contains(gameItem.gameName)) 
        {
            spawnedGameItem.GetComponent<StoreItem>().addButton.SetActive(false);
        }
        spawnedGameItems.Add(spawnedGameItem);
    }
}
