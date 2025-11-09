using System.Collections;
using UnityEngine;
using UnityEngine.Localization.Settings;
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
        if (!isChanging)
        {
            StartCoroutine(ChangeLanguageCoroutine(languageCode));
        }
    }

    public void OnLanguageDropDownChanged(int index)
    {
        string languageCode;
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

        isChanging = false;
    }
}
