using UnityEngine;

/// <summary>
/// Manages emission colors for elevator buttons
/// </summary>
public class ButtonEmissionManager : MonoBehaviour
{
    [SerializeField] private GameObject upButton;
    [SerializeField] private GameObject downButton;
    [SerializeField] private Color activeColor = Color.yellow;
    
    public static ButtonEmissionManager Instance { get; private set; }
    
    /// <summary>
    /// Initialize instance
    /// </summary>
    void Awake()
    {
        Instance = Instance ?? this;
    }
    
    /// <summary>
    /// Set button active and reset other button
    /// </summary>
    public void SetButtonActive(GameObject button, bool isActive)
    {
        SetEmission(button, isActive ? activeColor : Color.white);
        SetEmission(button == upButton ? downButton : upButton, Color.white);
    }
    
    /// <summary>
    /// Reset both buttons
    /// </summary>
    public void ResetAllButtons()
    {
        SetEmission(upButton, Color.white, downButton, Color.white);
    }
    
    /// <summary>
    /// Set emission color for buttons
    /// </summary>
    private void SetEmission(GameObject button, Color color, GameObject button2 = null, Color color2 = default)
    {
        ApplyEmission(button, color);
        if (button2 != null)
        {
            ApplyEmission(button2, color2);
        }
    }
    
    /// <summary>
    /// Apply emission color to all materials in button
    /// </summary>
    private void ApplyEmission(GameObject button, Color color)
    {
        if (button == null)
        {
            return;
        }
        foreach (var renderer in button.GetComponentsInChildren<Renderer>())
        {
            foreach (var material in renderer.materials)
            {
                material.SetColor("_EmissionColor", color);
                material.EnableKeyword("_EMISSION");
            }
        }
    }
}
