using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public GameObject startingScreen;
    public ErrorHandler errorHandler;

    [SerializeField] private InputField usernameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Text alertText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createButton;

    async void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Internet connection?
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            errorHandler.OnError(1000);
            return;
        }

        if (!PlayerPrefs.HasKey("DeviceToken"))
        {
            string uniqueToken = GenerateUniqueToken();

            PlayerPrefs.SetString("DeviceToken", uniqueToken);

            PlayerPrefs.Save();

            Debug.Log($"Device Token safed: {uniqueToken}");
        }
        
        try
        {
            await UnityServices.InitializeAsync();

            if (PlayerPrefs.HasKey("username"))
            {
                string username = PlayerPrefs.GetString("username");
                if (PlayerPrefs.HasKey("password"))
                {
                    string password = PlayerPrefs.GetString("password");
                    await SignInWithUsernamePasswordAsync(username, password);
                    return;
                }
                else Debug.Log("no password found");
            }
            else Debug.Log("no username found");

            startingScreen.SetActive(false);
            SetButtonState(true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async void SignIn()
    {
        alertText.text = "Signing in...";

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (username.Length < 3)
            alertText.text = "Username is too short";
        else if (username.Length > 20)
            alertText.text = "Username is too long";
        else if (password.Length < 8)
            alertText.text = "Password is too short";
        else if (password.Length > 30)
            alertText.text = "Password is too long";
        else
        {
            SetButtonState(false);
            await SignInWithUsernamePasswordAsync(username, password);
        }
    }

    public async void SignUp()
    {
        alertText.text = "Signing up...";

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (username.Length < 3)
            alertText.text = "Username is too short";
        else if (username.Length > 20)
            alertText.text = "Username is too long";
        else if (password.Length < 8)
            alertText.text = "Password is too short";
        else if (password.Length > 30)
            alertText.text = "Password is too long";
        else
        {
            SetButtonState(false);
            await SignUpWithUsernamePasswordAsync(username, password);
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    { 
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            alertText.text = "forwarding...";

            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("password", password);

            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.Log($"Authentication Error Code: {ex.ErrorCode}");

            if (ex.ErrorCode == 10002)
            {
                // Invalid credentials
                alertText.text = "Invalid credentials";
            }
            else if (ex.ErrorCode == 10003)
            {
                // Entity exists
                alertText.text = "Username already exists";
            }
            else
            {
                alertText.text = "Failed. Please try again";
            }

            SetButtonState(true);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            Debug.Log($"RequestFailed");

            alertText.text = "Invalid password";
            SetButtonState(true);
        }
    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            alertText.text = "forwarding...";

            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("password", password);

            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.Log($"Authentication Error Code: {ex.ErrorCode}");

            alertText.text = "Failed. Please try again";
            SetButtonState(true);
            startingScreen.SetActive(false);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            Debug.Log($"RequestFailed");

            alertText.text = "Invalid credentials";
            SetButtonState(true);
            startingScreen.SetActive(false);
        }
    }

    private void SetButtonState(bool toggle)
    {
        loginButton.interactable = toggle;
        createButton.interactable = toggle;
    }

    public void Quit()
    {
        Application.Quit();
    }

    private string GenerateUniqueToken()
    {
        return DateTime.Now.ToString("yyyyMMddHHmmss");
    }
}
