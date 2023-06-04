using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    public GameItem[] gameItems;
    public GameObject gameItemParent;
    public GameObject gameItemPrefab;

    public void OnClickGenre(GameObject category)
    {
        foreach (GameItem gameItem in gameItems)
        {
            string[] genres = gameItem.genres;
            foreach (string genre in genres)
            {
                if (category.name == genre)
                {
                    Debug.Log(category.name);
                    SpawnItem(gameItem);
                }
            }
        }
    }

    private void SpawnItem(GameItem gameItem)
    {

    }
}
