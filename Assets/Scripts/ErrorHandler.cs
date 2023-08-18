using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ErrorHandler : MonoBehaviour
{
    [Header("Error")]
    public GameObject errorPanel;
    public Button errorButton;
    public Text errorHeader;
    public Text errorText;

    [Header("Details")]
    public Text detail1;
    public Text detail2;
    public Text detail3;

    public void OnError(int errorCode, string[] details = null)
    {
        switch (errorCode)
        {
            // Connection
            case 1000:
                // Texts
                errorHeader.text = "Connection";
                errorText.text = "Unfortunately, we could not establish an Internet " +
                                 "connection. Please check your connection and try again.";

                // Details
                DateTime now = DateTime.Now;

                detail1.text = "Error Code:";
                detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
                detail2.text = "Time:";
                detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{now.Hour:00}:{now.Minute:00}";

                // Button & Panel
                errorButton.onClick.AddListener(Quit);
                errorButton.GetComponentInChildren<Text>().text = "Quit Game";
                errorPanel.SetActive(true);
                break;

            // Invalid Version
            case 1001:
                // Texts
                errorHeader.text = "Invalid Version";
                errorText.text = "Good news! There is a more recent version of this application. Since this " +
                                 "is a Game Launcher you cannot continue using it with an outdated version.";

                // Details
                detail1.text = "Error Code:";
                detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
                detail2.text = "Current Version:";
                detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = details[0];
                detail3.text = "Latest Version:";
                detail3.gameObject.transform.GetChild(0).GetComponent<Text>().text = details[1];

                // Button & Panel
                errorButton.onClick.AddListener(UpdateLauncher);
                errorButton.GetComponentInChildren<Text>().text = "Update Launcher";
                errorPanel.SetActive(true);
                break;

            // Maintenance
            case 1002:
                // Texts
                errorHeader.text = "Maintenance";
                errorText.text = "Our developers are maintaining the infrastructure " +
                                 "right now. We apologize for the inconvenience.";

                // Details
                DateTime current = DateTime.Now;

                detail1.text = "Error Code:";
                detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
                detail2.text = "Current Time:";
                detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{current.Hour:00}:{current.Minute:00}";
                detail3.text = "Expected Time:";
                detail3.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{details[0]}";

                // Button & Panel
                errorButton.onClick.AddListener(Quit);
                errorButton.GetComponentInChildren<Text>().text = "Quit Game";
                errorPanel.SetActive(true);
                break;
            
            case 1003:
                // Texts
                errorHeader.text = "Access to the path denied";
                errorText.text = "Access to the folder path you selected is denied. Change the path " +
                                 "during a new installation for more efficient memory usage. ";

                // Details
                detail1.text = "Error Code:";
                detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
                detail2.text = "Suggested Path:";
                detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"Documents";

                // Button & Panel
                errorButton.onClick.AddListener(Back);
                errorButton.GetComponentInChildren<Text>().text = "Back";
                errorPanel.SetActive(true);
                break;

            default:
                break;
        }
    }

    private void Back()
    {
        errorPanel.SetActive(false);
    }

    private void Quit()
    {
        // TODO Open Game Launcher.
        Application.Quit();
    }

    private void UpdateLauncher()
    {
        Debug.Log("Updating Launcher...");
    }
}
