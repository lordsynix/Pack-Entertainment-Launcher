using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemConstructor : MonoBehaviour
{
    public Image itemImage;
    public Text itemName;

    private bool isDownloaded;
    private bool inLibrary;

    public void ConstructWithData(Sprite sprite, string name, string gameName)
    {
        itemImage.sprite = sprite;
        itemName.text = name;
        gameObject.name = gameName;
        Debug.Log($"{itemName.text} aka. {gameObject.name} constructed");
    }    
}
