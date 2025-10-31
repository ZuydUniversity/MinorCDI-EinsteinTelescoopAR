using TMPro;
using UnityEngine;
/// <summary>
/// script for TMP Text objects to translate the text
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class LocalizedText : MonoBehaviour
{
    /// <summary>
    /// identification for the TMP
    /// </summary>
    public string localizationKey;
    /// <summary>
    /// TMP component
    /// </summary>
    private TMP_Text textComponent;
    /// <summary>
    /// Awake function gets TMP component
    /// </summary>
    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
    }
    /// <summary>
    /// Adds TMP to list of text that should be updated when the language gets updated
    /// </summary>
    private void OnEnable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged += UpdateText;
            UpdateText();
        }
    }
    /// <summary>
    /// Adds TMP to list of text that should be updated when the language gets updated
    /// </summary>
    private void OnDisable()
    {
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.OnLanguageChanged -= UpdateText;
        }
    }
    /// <summary>
    /// changes text based on localization key
    /// </summary>
    private void UpdateText()
    {
        textComponent.text = LocalizationManager.Instance.GetText(localizationKey);
    }
}
