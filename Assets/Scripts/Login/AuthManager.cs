using System;
using System.Security.Authentication;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance;

    public GameObject startingScreen;

    async void Awake()
    {
        Instance = this;

        try
        {
            await UnityServices.InitializeAsync();
            startingScreen.SetActive(false);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    public async Task SignUpWithUsernamePasswordAsync(string username, string password)
    { 
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
            Debug.Log("SignUp is successful.");
        }
        catch (Unity.Services.Authentication.AuthenticationException ex)
        {
            Debug.Log("Authentication Exception " + ex.ErrorCode);
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.Log("Request Exception");
            Debug.LogException(ex);
        }
    }

    public async Task SignInWithUsernamePasswordAsync(string username, string password)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
            Debug.Log("SignIn is successful.");
        }
        catch (Unity.Services.Authentication.AuthenticationException ex)
        {
            Debug.LogException(ex);
        }
        catch (RequestFailedException ex)
        {
            Debug.LogException(ex);
        }
    }

    void SetupEvents()
    {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Debug.Log($"Access Token: {AuthenticationService.Instance.AccessToken}");

        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Debug.LogError(err);
        };

        AuthenticationService.Instance.SignedOut += () => {
            Debug.Log("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>
        {
            Debug.Log("Player session could not be refreshed and expired.");
        };
    }

    public void Quit()
    {
        Application.Quit();
    }
}
