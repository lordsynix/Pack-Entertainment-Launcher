using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Login : MonoBehaviour
{
    [SerializeField] private string loginEndpoint = "http://127.0.0.1:13756/account/login";
    [SerializeField] private string createEndpoint = "http://127.0.0.1:13756/account/create";
    private const string PASSWORD_REGEX = "(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,24})";

    [SerializeField] private InputField usernameInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Text alertText;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button createButton;

    public async void OnLoginClick()
    {
        alertText.text = "Signing in...";
        ActivateButtons(false);

        await TryLogin();
    }

    public async void OnCreateClick()
    {
        alertText.text = "Creating account...";
        ActivateButtons(false);

        await TryCreate();

        // Call result
    }

    private async Task TryLogin()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        /*if (username.Length < 3 || username.Length > 20)
        {
            alertText.text = "Invalid username";
            ActivateButtons(true);
            return;
        }

        if (!Regex.IsMatch(password, PASSWORD_REGEX))
        {
            alertText.text = "Invalid password";
            ActivateButtons(true);
            return;
        }*/

        await AuthManager.Instance.SignInWithUsernamePasswordAsync(username, password);
    }

    private async Task TryCreate()
    {
        string username = usernameInputField.text;
        string password = passwordInputField.text;

        /*if (username.Length < 3 || username.Length > 24)
        {
            alertText.text = "Invalid username";
            ActivateButtons(true);
            return;
        }

        if (!Regex.IsMatch(password, PASSWORD_REGEX))
        {
            alertText.text = "Invalid password";
            ActivateButtons(true);
            return;
        }*/

        await AuthManager.Instance.SignUpWithUsernamePasswordAsync(username, password);
    }

    private void ActivateButtons(bool toggle)
    {
        loginButton.interactable = toggle;
        createButton.interactable = toggle;
    }
}
