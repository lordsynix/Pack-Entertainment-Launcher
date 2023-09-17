using System;
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
        // Opens troubleshooting webpage in the user's standard browser
        Application.OpenURL("https://packentertainment.net/troubleshooting");

        detail1.text = "Error Code:";
        detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
        detail2.text = "";
        detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = "";
        detail3.text = "";
        detail3.gameObject.transform.GetChild(0).GetComponent<Text>().text = "";

        switch (errorCode)
        {
            // Connection
            case 1000:
                OnConnectionError(errorCode);
                break;

            // Invalid Version
            case 1001:
                OnInvalidVersionError(errorCode, details);
                break;

            // Maintenance
            case 1002:
                // Texts
                OnMaintenance(errorCode, details);
                break;
            
            case 1003:
                OnAccessDeniedError(errorCode);
                break;

            case 1004:
                OnGameLaunchError(errorCode, details);
                break;

            case 1005:
                OnExtractionError(errorCode, details);
                break;

            case 1006:
                OnLauncherLaunchError(errorCode);
                break;

            default:
                break;
        }
    }

    private void OnConnectionError(int errorCode)
    {
        // Texts
        errorHeader.text = "Connection";
        errorText.text = "Unfortunately, we could not establish an Internet " +
                         "connection. Please check your connection and try again.";

        // Details
        DateTime now = DateTime.Now;

        detail1.text = "Error Code:";
        detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
        detail2.text = "Last Checked:";
        detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{now.Hour:00}:{now.Minute:00}";

        // Button & Panel
        errorButton.onClick.AddListener(Quit);
        errorButton.GetComponentInChildren<Text>().text = "Quit Game";
        errorPanel.SetActive(true);
    }

    private void OnInvalidVersionError(int errorCode, string[] details)
    {
        // Texts
        errorHeader.text = "Invalid Version";
        errorText.text = "Good news! There is a more recent version of this application. Since this " +
                         "is a Game Launcher you cannot continue using it with an outdated version.";

        // Details
        DateTime now = DateTime.Now;

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
    }

    private void OnMaintenance(int errorCode, string[] details)
    {
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
    }

    private void OnAccessDeniedError(int errorCode)
    {
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
    }

    private void OnLauncherLaunchError(int errorCode)
    {
        // Texts
        errorHeader.text = "Launcher launch failed";
        errorText.text = $"The new Launcher could not be started. Please try to launch the new version's executable manually " +
                         $"under the path 'C:/Users/[User]/Documents/Pack Entertainment/Launcher-[Newest Version]'.";

        // Details
        detail1.text = "Error Code:";
        detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
        detail2.text = "Game Name:";
        detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"Launcher";

        // Button & Panel
        errorButton.onClick.AddListener(Back);
        errorButton.GetComponentInChildren<Text>().text = "Back";
        errorPanel.SetActive(true);
    }

    private void OnExtractionError(int errorCode, string[] details)
    {
        // Texts
        errorHeader.text = "Game extraction failed";
        errorText.text = $"{details[0]} could not be extracted. Probably the " +
                         $"file already exists. Please try downloading the game again.";

        // Details
        detail1.text = "Error Code:";
        detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
        detail2.text = "Game Name:";
        detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{details[0]}";

        // Button & Panel
        errorButton.onClick.AddListener(Back);
        errorButton.GetComponentInChildren<Text>().text = "Back";
        errorPanel.SetActive(true);
    }

    private void OnGameLaunchError(int errorCode, string[] details)
    {
        // Texts
        errorHeader.text = "Game launch failed";
        errorText.text = $"{details[0]} could not be started. Please note that you cannot change the " +
                         $"file path of the game. The {details[0]} will be downloaded again for you.";

        // Details
        detail1.text = "Error Code:";
        detail1.gameObject.transform.GetChild(0).GetComponent<Text>().text = errorCode.ToString();
        detail2.text = "Game Name:";
        detail2.gameObject.transform.GetChild(0).GetComponent<Text>().text = $"{details[0]}";

        // Button & Panel
        errorButton.onClick.AddListener(Back);
        errorButton.GetComponentInChildren<Text>().text = "Back";
        errorPanel.SetActive(true);
    }

    private void Back()
    {
        errorPanel.SetActive(false);
    }

    private void Quit()
    {
        // TODO Open Game Launcher.
        try
        {
            GameManager.instance.Quit();
        }
        catch
        {
            Application.Quit();
        }
    }

    private void UpdateLauncher()
    {
        GameManager.instance.updateScreen.SetActive(true);
        Back();
        StartCoroutine(Installer.Download(null, true));
    }
}
