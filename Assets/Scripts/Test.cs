using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Test : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        string path = Application.dataPath + "/../../";
        Debug.Log(path);
        Debug.Log(Path.GetFileName(path));
    }

}
