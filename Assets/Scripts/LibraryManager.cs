using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LibraryManager : MonoBehaviour
{
    [Header("Windows")]
    public GameObject library;

    [Header("Game Items")]
    public GameItem[] gameItems;
    public GameObject gameItemParent;
    public GameObject gameItemPrefab;



    private void OnEnable()
    {
        foreach (GameItem gameItem in gameItems)
        {
            var spawnedGameItem = Instantiate(gameItemPrefab, gameItemParent.transform);

            spawnedGameItem.GetComponentInChildren<Text>().text = gameItem.gameName;
            spawnedGameItem.GetComponentInChildren<Image>().sprite = gameItem.gameLogo;
        }
    }

    public void Library()
    {
        library.SetActive(true);
    }

}
