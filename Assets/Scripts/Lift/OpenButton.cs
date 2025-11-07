using UnityEngine;

/// <summary>
/// Button component that opens elevator doors when tapped.
/// Only functions when elevator is in closed state.
/// </summary>
public class OpenButton : MonoBehaviour, ITappable
{
    /// <summary>
    /// The controller for the elevator used to open it.
    /// </summary>
    
    //sound clip for the button tap
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private ElevatorController elevatorController;
    
    /// <summary>
    /// Initializes the elevator controller.
    /// </summary>
    void Start()
    {
        if (elevatorController == null)
        {
            elevatorController = FindFirstObjectByType<ElevatorController>();
        }
    }
    
    /// <summary>
    /// Opens elevator doors tapped and elevator is in closed state.
    /// </summary>
    public void OnTapped()
    {
        if (elevatorController != null && elevatorController.currentState == ElevatorController.ElevatorState.Closed)
        {
            elevatorController.OpenDoors();
        }
        // Play button sound effect
        if (buttonClip != null)
        {
            AudioSource.PlayClipAtPoint(buttonClip, transform.position);
        }
        else
        {
            Debug.LogWarning("Button clip is not assigned in OpenButton.");
        }

    }
}