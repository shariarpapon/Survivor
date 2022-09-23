using UnityEngine;
using SimpleJSON;

[CreateAssetMenu(fileName = "User Preference", menuName = "Scriptableobjects/User Preference")]
public class UserPreference : ScriptableObject, IUniversalStorableData
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


    public void SaveUniversalData() 
    {
        JSONObject json = new JSONObject();
        json.Add(nameof(masterVolume), masterVolume);
        json.Add(nameof(musicVolume), musicVolume);
        json.Add(nameof(sfxVolume), sfxVolume);;
        json.Add(nameof(ambientVolume), ambientVolume);

        GameDataIO.WriteUniversalFile("user_preference", json.ToString());
    }

    public void LoadUniversalData() 
    {
        string data = GameDataIO.ReadUniversalFile("user_preference");
        if (string.IsNullOrEmpty(data)) return;

        JSONObject json = JSON.Parse(data) as JSONObject;
        masterVolume = json[nameof(masterVolume)];
        musicVolume = json[nameof(musicVolume)];
        sfxVolume = json[nameof(sfxVolume)];
        ambientVolume = json[nameof(ambientVolume)];
    }
}
