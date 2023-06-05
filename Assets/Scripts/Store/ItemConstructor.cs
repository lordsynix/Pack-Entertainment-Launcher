using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemConstructor : MonoBehaviour
{
    public Image itemImage;
    public Text itemName;

    public void ConstructWithData(Sprite sprite, string name)
    {
        itemImage.sprite = sprite;
        itemName.text = name;
    }
}
