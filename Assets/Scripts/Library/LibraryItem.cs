using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LibraryItem : MonoBehaviour
{
    public Image background;

    public void OnClickItem()
    {
        if (Installer.IsDownloading) return;

        LibraryManager.instance.SelectGame(gameObject);
    }
}
