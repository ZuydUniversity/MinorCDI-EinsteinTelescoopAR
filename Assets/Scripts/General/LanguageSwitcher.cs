using UnityEngine;

public class LanguageSwitcher : MonoBehaviour
{
    public void SetLanguage(string languageCode)
    {
        LocalizationManager.Instance.LoadLanguage(languageCode);
    }
}
