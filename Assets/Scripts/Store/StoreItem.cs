using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreItem : MonoBehaviour
{
    public GameObject addButton;

    public void OnClickAdd()
    {
        addButton.SetActive(false);
        GameManager.instance.AddGameToLibrary(GetComponent<ItemConstructor>().itemName.text);
    }
}
