using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;

public class LocalizationManager : MonoBehaviour
{
    /// <summary>
    /// Used to make itself a Singleton
    /// </summary>
    public static LocalizationManager Instance;
    /// <summary>
    /// Dictionary of all texts which should be changed
    /// </summary>
    private Dictionary<string, string> localizedTexts = new Dictionary<string, string>();
    /// <summary>
    /// Language currently applied
    /// </summary>
    public string CurrentLanguage { get; private set; } = "en";
    /// <summary>
    /// Action to call all text fields. Gets triggerd by a language change
    /// </summary>
    public event Action OnLanguageChanged; 
    /// <summary>
    /// Used to make itself a singleton
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadLanguage(CurrentLanguage);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    /// <summary>
    /// Loads selected language after a button press
    /// </summary>
    /// <param name="languageCode"></param>
    public void LoadLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;

        string path = Path.Combine(Application.streamingAssetsPath, $"{languageCode}.json");

        if (!File.Exists(path))
        {
            Debug.LogWarning($"Localization file not found: {path}");
            return;
        }

        string json = File.ReadAllText(path);
        localizedTexts = JsonUtility.FromJson<LocalizationDictionary>(json).ToDictionary();

        Debug.Log($"Language loaded: {languageCode}");
        OnLanguageChanged?.Invoke(); 
    }

    public string GetText(string key)
    {
        if (localizedTexts.TryGetValue(key, out string value))
            return value;
        else
            return $"#{key}#"; 
    }
}
/// <summary>
/// Entry in json file used for localization
/// </summary>
[System.Serializable]
public class LocalizationEntry
{
    public string key;
    public string value;
}
/// <summary>
/// Dictionary of entries used for localization
/// </summary>
[System.Serializable]
public class LocalizationDictionary
{
    public LocalizationEntry[] entries;

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var entry in entries)
            dict[entry.key] = entry.value;
        return dict;
    }
}
