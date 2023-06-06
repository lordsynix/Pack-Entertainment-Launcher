using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LibraryManager : MonoBehaviour
{
    public Transform gameItemParent;
    public GameObject gameItemPrefab;

    public StoreManager store;

    private List<GameObject> libraryCollection = new List<GameObject>();

    private void OnEnable()
    {
        SpawnItems();
        SelectGame(libraryCollection[0]);
    }

    private void SpawnItems()
    {
        string[] gameNames = GameManager.instance.GetlibraryItems();  
        foreach (string gameName in gameNames)
        {
            // Check if item exists in store. If yes replicate the item in library
            foreach (GameItem gameItem in store.gameItems)
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
            if (gameName == game.GetComponent<ItemConstructor>().itemName.text)
            {
                return true;
            }
        }
        return false;        
    }

    public void SelectGame(GameObject game)
    {

    }
}
