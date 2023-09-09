using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Profile
{
    public List<Device> Devices = new();

    [System.Serializable]
    public class Device
    {
        public string DeviceName;
        public string DeviceToken;
        public string DeviceType;

        public Device(string deviceName, string deviceToken, string deviceType)
        {
            DeviceName = deviceName;
            DeviceToken = deviceToken;
            DeviceType = deviceType;
        }
    }

    public static Profile FromJson(string json)
    {
        Profile profile;

        if (string.IsNullOrEmpty(json))
        {
            profile = new Profile();
        }
        else
        {
            profile = JsonUtility.FromJson<Profile>(json);
        }

        UpdateDevice(profile);

        ProfileManager.Instance.SetPlayerProfile(profile);

        return profile;
    }

    public static void UpdateDevice(Profile profile)
    {
        string deviceToken = PlayerPrefs.GetString("DeviceToken");
        
        // Find the index of the device with the matching DeviceToken in the list
        int deviceIndex = profile.Devices.FindIndex(device => device.DeviceToken == deviceToken);

        if (deviceIndex != -1)
        {
            // Remove the old device from the list
            if (deviceToken != profile.Devices[deviceIndex].DeviceToken) { Debug.LogError("Device Tokens don't match!"); };
            profile.Devices.RemoveAt(deviceIndex);

            // Create an updated device and add it back to the list
            Device updatedDevice = new(SystemInfo.deviceName, deviceToken, SystemInfo.deviceType.ToString());
            profile.Devices.Add(updatedDevice);

            Debug.Log($"Device with the name {SystemInfo.deviceName} updated.");
        }

        else
        {
            // Device not found, so add it to the list
            Device newDevice = new(SystemInfo.deviceName, deviceToken, SystemInfo.deviceType.ToString());
            profile.Devices.Add(newDevice);

            Debug.Log($"Device with the name {SystemInfo.deviceName} created.");
        }
    }
}
