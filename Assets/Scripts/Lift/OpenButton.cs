using UnityEngine;

/// <summary>
/// Button component that opens elevator doors when tapped.
/// Only functions when elevator is in closed state.
/// </summary>
public class OpenButton : MonoBehaviour, ITappable
{
    [SerializeField] private ElevatorController elevatorController;
    
    /// <summary>
    /// Initializes the elevator controller.
    /// </summary>
    void Start()
    {
        if (elevatorController == null)
        {
            elevatorController = FindObjectOfType<ElevatorController>();
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
    }
}