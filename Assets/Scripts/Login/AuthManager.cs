using System;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public GameObject startingScreen;
    public ErrorHandler errorHandler;

    private string username;
    private string password;
    private bool usernameFieldActive = false;

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
            PlayerPrefs.DeleteAll();
            string uniqueToken = GenerateUniqueToken();

            PlayerPrefs.SetString("DeviceToken", uniqueToken);

            PlayerPrefs.Save();

            Debug.Log($"Device Token safed: {uniqueToken}");

        }

        try
        {
            await UnityServices.InitializeAsync();

            // Check if launcher with credentials in args
            if (ArgsContainCredentials())
            {
                await SignInWithUsernamePasswordAsync(username, password, true);
                return;
            }

            if (PlayerPrefs.HasKey("username"))
            {
                username = PlayerPrefs.GetString("username");
                if (PlayerPrefs.HasKey("password"))
                {
                    password = PlayerPrefs.GetString("password");
                    await SignInWithUsernamePasswordAsync(username, password);
                    return;
                }
                else Debug.Log("no password found");
            }
            else Debug.Log("no username found");

            startingScreen.SetActive(false);
            SetButtonState(true);
            SelectInputField(true);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private void Start()
    {
        if (!Application.isEditor)
        {
            string launcherExecutablePath = Installer.CreateGamesFolder() + "../Launcher-" + Application.version + "/Pack Launcher.exe";

            // Create new desktop shortcut icon
            DesktopShortcut.Main("Pack Launcher", launcherExecutablePath);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SelectInputField(!usernameFieldActive);
        }

        if (Input.GetKeyUp(KeyCode.KeypadEnter))
        {
            SignIn();
        }
    }

    private void SelectInputField(bool firstField)
    {
        if (firstField)
        {
            usernameInputField.ActivateInputField();
            usernameFieldActive = true;
        }
        else
        {
            passwordInputField.ActivateInputField();
            usernameFieldActive = false;
        }
    }

    private bool ArgsContainCredentials()
    {
        string[] cmdArgs = Environment.GetCommandLineArgs();

        for (int i = 0; i < cmdArgs.Length; i++)
        {
            if (cmdArgs[i] == "-username")
            {
                username = cmdArgs[i];
                PlayerPrefs.SetString("username", username);
            }
            if (cmdArgs[i] == "-password")
            {
                password = cmdArgs[i];
                PlayerPrefs.SetString("password", password);
            }
        }
        if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
        {
            return true;
        }
        else return false;
    }

    private static bool IsStrongPassword(string password)
    {
        // Define the regular expression pattern for the password requirements
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[!@#$%^&*()_+{}|:;<>,.?~\\[\]\""'-]).{8,30}$";

        // Use Regex.IsMatch to check if the password matches the pattern
        return Regex.IsMatch(password, pattern);
    }

    private string HashPassword(string password)
    {
        // Erstelle eine Instanz des SHA-256-Hashalgorithmus
        using (SHA256 sha256 = SHA256.Create())
        {
            // Konvertiere das Passwort in Bytes
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // Berechne den Hashwert des Passworts
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);

            // Konvertiere den Hashwert in eine hexadezimale Zeichenfolge
            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();

            return hashedPassword[..26] + "aA1!";
        }
    }

    public async void SignIn()
    {
        alertText.color = Color.white;
        alertText.text = "Signing in...";

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (username.Length < 3)
        {
            alertText.color = Color.red;
            alertText.text = "Username is too short";
        }
        else if (username.Length > 20)
        {
            alertText.color = Color.red;
            alertText.text = "Username is too long";
        }
        else if (!IsStrongPassword(password))
        {
            alertText.color = Color.red;
            alertText.text = "Password is invalid";
        }
        else
        {
            SetButtonState(false);
            await SignInWithUsernamePasswordAsync(username, HashPassword(password));
        }
    }

    public async void SignUp()
    {
        alertText.color = Color.white;
        alertText.text = "Signing up...";

        string username = usernameInputField.text;
        string password = passwordInputField.text;

        if (username.Length < 3)
        {
            alertText.color = Color.red;
            alertText.text = "Username is too short";
        }
        else if (username.Length > 20)
        {
            alertText.color = Color.red;
            alertText.text = "Username is too long";
        }
        else if (!IsStrongPassword(password))
        {
            alertText.color = Color.red;
            alertText.text = "Password is invalid";
        }
        else
        {
            SetButtonState(false);
            await SignUpWithUsernamePasswordAsync(username, HashPassword(password));
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);

            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("password", password);

            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.ErrorCode);

            alertText.text = GetAuthenticationErrorMessage(ex);
            SetButtonState(true);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.ErrorCode);

            alertText.text = GetRequestErrorMessage(ex);
            SetButtonState(true);
        }
    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password, bool authWithArgs = false)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);

            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("password", password);

            SceneManager.LoadScene(1);
        }
        catch (AuthenticationException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.ErrorCode);

            if (!authWithArgs) alertText.text = GetAuthenticationErrorMessage(ex);
            SetButtonState(true);
            startingScreen.SetActive(false);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
            Debug.Log(ex.Message);

            if (!authWithArgs) alertText.text = GetRequestErrorMessage(ex);
            SetButtonState(true);
            startingScreen.SetActive(false);
        }
    }

    private string GetAuthenticationErrorMessage(AuthenticationException ex)
    {
        alertText.color = Color.red;

        if (ex.ErrorCode == 10002)
        {
            // Invalid credentials
            return "Invalid credentials";
        }
        else if (ex.ErrorCode == 10003)
        {
            // Entity exists
            return "Username already exists";
        }
        else
        {
            return "Authentication failed. Please try again";
        }
    }

    private string GetRequestErrorMessage(RequestFailedException ex)
    {
        alertText.color = Color.red;

        return "Invalid credentials";
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
