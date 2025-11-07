using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Collections;
using TMPro;

/// <summary>
/// script for buttons to call to change language
/// </summary>
public class LanguageSwitcher : MonoBehaviour
{

    public GameObject Dropdown;
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
    /// <summary>
    /// Changes the language on the background
    /// </summary>
    /// <param name="languageCode"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Function to call function from the UI dropdown
    /// </summary>
    public void ChangeLanguageFromDropdown()
    {
        string code = "";
        if (Dropdown.GetComponent<TMP_Dropdown>().captionText.text == "Nederlands") { code = "nl-NL"; }
        if (Dropdown.GetComponent<TMP_Dropdown>().captionText.text == "English") { code = "en"; }
        SetLanguage(code);
    }
}
