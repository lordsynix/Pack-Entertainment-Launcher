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

    private void Start()
    {
        HorizontalLayoutGroup[] categories = GetComponentsInChildren<HorizontalLayoutGroup>();
        foreach (HorizontalLayoutGroup category in categories)
        {
            if (category.enabled == true)
            {
                previousCategory = category.gameObject;
            }
        }
    }

    public void OnClickCategory(GameObject inputCategory)
    {
        inputCategory.SetActive(true);
        previousCategory.SetActive(false);
        previousCategory = inputCategory;

        // Search all game items in specified category
        foreach (GameItem gameItem in gameItems)
        {
            string[] categories = gameItem.genres;
            foreach (string category in categories)
            {
                if (inputCategory.name == category)
                {
                    SpawnItem(gameItem);
                }
                else
                {
                    Debug.Log("Name of category not found!");
                }
            }
        }
    }

    private void SpawnItem(GameItem gameItem)
    {

    }
}
