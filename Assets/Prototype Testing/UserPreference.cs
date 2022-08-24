using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using SimpleJSON;
using Survivor.Core;

[CreateAssetMenu(fileName = "User Preference", menuName = "Scriptableobjects/User Preference")]
public class UserPreference : ScriptableObject, IGlobalStorableData
{
    [Header("Audio")]
    [Range(0, 1)]
    public float masterVolume = 1;
    [Range(0, 1)]
    public float musicVolume = 1;
    [Range(0, 1)]
    public float sfxVolume = 1;
    [Range(0, 1)]
    public float ambientVolume = 1;


    public void SaveData() 
    {
        JSONObject json = new JSONObject();
        json.Add(nameof(masterVolume), masterVolume);
        json.Add(nameof(musicVolume), musicVolume);
        json.Add(nameof(sfxVolume), sfxVolume);;
        json.Add(nameof(ambientVolume), ambientVolume);
        File.WriteAllText(GetDataPath(), json.ToString());
    }

    public void LoadData() 
    {
        string path = GetDataPath();
        if (File.Exists(path) == false) return;

        string data = File.ReadAllText(path);
        JSONObject json = JSON.Parse(data) as JSONObject;
        masterVolume = json[nameof(masterVolume)];
        musicVolume = json[nameof(musicVolume)];
        sfxVolume = json[nameof(sfxVolume)];
        ambientVolume = json[nameof(ambientVolume)];
    }

    public string GetDataPath() 
    {
        return GameManager.GetGlobalDataFilePath("uPrefs");
    }
}
