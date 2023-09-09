using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    public GameObject ProfileInformationWindow;
    public Text ProfileTabTag;

    [Header("Information Containers")]
    public GameObject ProfileInformationParent;
    public GameObject DevicesInformationParent;

    [Header("Information Prefab")]
    public GameObject ProfileInformationPrefab;
    public GameObject DevicesInformationPrefab;

    private Profile PlayerProfile;
    private GameObject previousSelected;

    private void Awake()
    {
        Instance = this;

        previousSelected = ProfileInformationWindow;
    }

    public Profile GetPlayerProfile()
    {
        return PlayerProfile;
    }

    public void SetPlayerProfile(Profile profile)
    {
        PlayerProfile = profile;

        SetProfileInformation();
        SetDevicesInformation();
    }

    private void SetProfileInformation()
    {
        Dictionary<string, string> data = new()
        {
            { "Username:", PlayerPrefs.GetString("username") },
            { "Password:", new string('*', PlayerPrefs.GetString("password").Length) },
            { "In library:", $"{DataManager.LibraryGames.Count} games added" },
            { "Account devices:", $"{PlayerProfile.Devices.Count} active devices" }
        };

        foreach (var kvp in data)
        {
            var newKeyValuePair = Instantiate(ProfileInformationPrefab, ProfileInformationParent.transform);

            Text[] informations = newKeyValuePair.GetComponentsInChildren<Text>();
            informations[0].text = kvp.Key;
            informations[1].text = kvp.Value;
        }
    }

    private void SetDevicesInformation()
    {
        foreach (var device in PlayerProfile.Devices)
        {
            var newDevice = Instantiate(DevicesInformationPrefab, DevicesInformationParent.transform);

            Text[] informations = newDevice.GetComponentsInChildren<Text>();
            informations[0].text = device.DeviceName;
            informations[1].text = device.DeviceType;
            informations[2].text = device.DeviceToken;
        }
    }

    public void OnClickProfileTab(GameObject tabWindow)
    {
        if (previousSelected != tabWindow)
        {
            tabWindow.SetActive(true);
            ProfileTabTag.text = tabWindow.name;

            if (previousSelected != null)
            {
                previousSelected.SetActive(false);
            }

            previousSelected = tabWindow;
        }
    }
}
