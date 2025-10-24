using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ClickableObject : MonoBehaviour
{
    /// <summary>
    /// title for text canvas for showing information
    /// </summary>
    public string title;
    /// <summary>
    /// description for text canvas for showing information
    /// </summary>
    public string description;

    /// <summary>
    /// Bool if clickable object is a lever. If it is a lever it does not show information but plays its animation instead.
    /// </summary>
    public bool isLever = false;
    /// <summary>
    /// bool if lever is on. used only if object is lever. used for animation
    /// </summary>
    private bool isOn = false;
    /// <summary>
    /// animator for lever anitmation
    /// </summary>
    public Animator leverAnimator;
    /// <summary>
    /// bool if lever is on. used only if object is lever. used for animation
    /// </summary>
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
            leverAnimator.SetFloat("Speed", 1f);
            leverAnimator.Play("Lever_Animation", 0, 0f);
        }
        else
        {
            // Animation in reverse
            leverAnimator.SetFloat("Speed", -1f);
            leverAnimator.Play("Lever_Animation", 0, 1f);
        }

    }
}
