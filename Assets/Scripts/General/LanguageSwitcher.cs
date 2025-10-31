using UnityEngine;
/// <summary>
/// script for buttons to call to change language
/// </summary>
public class LanguageSwitcher : MonoBehaviour
{
    /// <summary>
    /// Sets language. languageCode should be the name of the language file in the StreamingAssets folder
    /// </summary>
    /// <param name="languageCode"></param>
    public void SetLanguage(string languageCode)
    {
        LocalizationManager.Instance.LoadLanguage(languageCode);
    }
}
