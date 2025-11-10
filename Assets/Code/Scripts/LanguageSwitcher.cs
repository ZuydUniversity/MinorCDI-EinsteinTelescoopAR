using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

/// <summary>
/// script for buttons to call to change language
/// </summary>
public class LanguageSwitcher : MonoBehaviour
{
    /// <summary>
    /// Checks if it is changing language.
    /// </summary>
    private bool isChanging = false;

    /// <summary>
    /// Call this from a button click to change the entire app’s language.
    /// </summary>
    /// <param name="languageCode">The language code</param>
    public void SetLanguage(string languageCode)
    {
        // Only run if no other language change is ongoing
        if (!isChanging)
        {
            StartCoroutine(ChangeLanguageCoroutine(languageCode));
        }
    }
    /// <summary>
    /// Triggers when a dropdown value changes.
    /// Maps Dropdown Index to language code and initiates the language update.
    /// </summary>
    public void OnLanguageDropDownChanged(int index)
    {
        string languageCode;
        // Map dropdown index to specific language codes.
        switch(index)
        {
            case 0:
                languageCode = "en";
                break;

            case 1:
                languageCode = "nl-NL";
                break;

            default:
                languageCode = "N/A";
                break;
        }

        SetLanguage(languageCode);
    }

    /// <summary>
    /// Changes the language on the background
    /// </summary>
    /// <param name="languageCode">The code of the language to load</param>
    private IEnumerator ChangeLanguageCoroutine(string languageCode)
    {
        isChanging = true;

        // Wait for Unity Localization to initialize
        yield return LocalizationSettings.InitializationOperation;

        var locales = LocalizationSettings.AvailableLocales.Locales;       
        foreach (var locale in locales)
        {
            if (locale.Identifier.Code == languageCode)
            {              
                LocalizationSettings.SelectedLocale = locale;
                break;
            }
        }

        isChanging = false; // Unlock for future changes.
    }
}
