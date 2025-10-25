using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickableObject : MonoBehaviour
{
    public string title;
    public string description;

    public bool isLever = false;
    private bool isOn = false;

    public Animator leverAnimator; 
    public string toggleParameter = "IsOn"; 

    /// <summary>
    /// Toggles lever, use this to add functionality
    /// </summary>
    public void ToggleLever()
    {
        isOn = !isOn;
        Debug.Log("Lever toggled: " + (isOn ? "ON" : "OFF"));

        if (leverAnimator != null)
            leverAnimator.SetBool(toggleParameter, isOn);

        if (isOn)
        {
            // Speel de animatie vooruit
            leverAnimator.SetFloat("Speed", 1f);
            leverAnimator.Play("Lever_Animation", 0, 0f);
        }
        else
        {
            // Speel de animatie achteruit
            leverAnimator.SetFloat("Speed", -1f);
            // Start aan het einde van de animatie
            leverAnimator.Play("Lever_Animation", 0, 1f);
        }

    }
}
