using UnityEngine;

/// <summary>
/// Manages emission colors for elevator buttons
/// </summary>
public class ButtonEmissionManager : MonoBehaviour
{
    /// <summary>
    /// List of buttons which can be lit up.
    /// </summary>
    public GameObject[] buttons;

    /// <summary>
    /// The color to make the button.
    /// </summary>
    public Color activeColor = Color.yellow;
    /// <summary>
    /// The color to make the button if they are not active.
    /// </summary>
    public Color inactiveColor = Color.white;

    /// <summary>
    /// The intensity of the glow.
    /// </summary>
    public float emmisionStrength = 2.0f;

    /// <summary>
    /// Sets button color.
    /// </summary>
    /// <param name="button" type="GameObject">The button to set the color of.</param>
    /// <param name="setActive" type="bool">Tells the function to set the button active(true)/inactive(false).</param>
    public void SetButtonEmission(GameObject button, bool setActive)
    {
        foreach(Renderer renderer in button.GetComponentsInChildren<Renderer>())
        {
            foreach (Material material in renderer.materials)
            {
                if (setActive)
                {
                    material.SetColor("_EmissionColor", activeColor * emmisionStrength);
                }
                else
                {
                    material.SetColor("_EmissionColor", inactiveColor);
                }
            }
        }
    }

    /// <summary>
    /// Resets button colors to inactive.
    /// </summary>
    public void ResetButtons() 
    {
        foreach (GameObject button in buttons) 
        {
            SetButtonEmission(button, false);
        }
    }
}
