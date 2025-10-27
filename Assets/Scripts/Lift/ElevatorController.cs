using UnityEngine;
using System.Collections;

public class ElevatorController : MonoBehaviour
{
    /// <summary>
    /// The Animator component that controls the door animations
    /// </summary>
    [Header("Animation Settings")]
    [SerializeField] private Animator doorAnimator;
    /// <summary>
    /// The animator bool parameter name to control door animations
    /// </summary>
    [Header("Door Animation Parameter")]
    [SerializeField] private string doorsOpenBool = "DoorsOpen";
    
    [Header("Timing Configuration")]
    [SerializeField] private float doorOpenDuration = 2.1f;
    [SerializeField] private float doorCloseDuration = 2.1f;
    
    public enum ElevatorState { Idle, Opening, Open, Closing, Moving }
    public ElevatorState currentState = ElevatorState.Idle;
    
    /// <summary>
    /// Initialize the door animator component if not assigned in the inspector
    /// </summary>
    void Awake()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }
    }
    
    /// <summary>
    /// Initialize elevator state and ensure doors are closed
    /// </summary>
    void Start()
    {
        StartCoroutine(InitializeElevatorState());
    }
    
    /// <summary>
    /// Initialize elevator state after a short delay to avoid conflicts
    /// </summary>
    private IEnumerator InitializeElevatorState()
    {
        yield return new WaitForSeconds(0.1f);
        
        if (currentState == ElevatorState.Idle || currentState == ElevatorState.Open || currentState == ElevatorState.Moving)
        {
            ResetElevatorState();
        }
    }
    
    /// <summary>
    /// Opens doors and transitions to Open state
    /// </summary>
    public void OpenDoors()
    {
        if (IsAnimatorValid() && currentState == ElevatorState.Idle)
        {
            StartCoroutine(OpenDoorsCoroutine());
        }
    }

    /// <summary>
    /// Closes doors and transitions to Idle state
    /// </summary>
    public void CloseDoors()
    {
        if (IsAnimatorValid())
        {
            if (currentState == ElevatorState.Open)
            {
                StartCoroutine(CloseDoorsCoroutine());
            }
            else if (currentState == ElevatorState.Opening)
            {
                StartCoroutine(CloseDoorsAfterOpening());
            }
        }
    }
    
    /// <summary>
    /// Coroutine that transitions from Idle to Opening to Open state
    /// </summary>
    private IEnumerator OpenDoorsCoroutine()
    {
        yield return StartCoroutine(AnimateDoors(true, doorOpenDuration, ElevatorState.Opening, ElevatorState.Open));
    }
    
    /// <summary>
    /// Coroutine that transitions from Open to Closing to Idle state
    /// </summary>
    private IEnumerator CloseDoorsCoroutine()
    {
        currentState = ElevatorState.Closing;
        doorAnimator.SetBool(doorsOpenBool, false);
        yield return new WaitForSeconds(doorCloseDuration);
        currentState = ElevatorState.Idle;
    }
    
    /// <summary>
    /// Waits for Opening state to complete, then closes doors
    /// </summary>
    private IEnumerator CloseDoorsAfterOpening()
    {
        while (currentState == ElevatorState.Opening)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (currentState == ElevatorState.Open)
        {
            StartCoroutine(CloseDoorsCoroutine());
        }
    }
    
    /// <summary>
    /// Starts the elevator sequence: open doors and execute callback
    /// </summary>
    public void StartElevatorSequence(System.Action onSequenceComplete = null)
    {
        if (!gameObject.activeInHierarchy)
        {
            onSequenceComplete?.Invoke();
            return;
        }
        
        if (!IsAnimatorValid())
        {
            onSequenceComplete?.Invoke();
            return;
        }
        
        // If doors are already open, execute callback immediately
        if (currentState == ElevatorState.Open)
        {
            onSequenceComplete?.Invoke();
            return;
        }
        
        // If doors are closing, wait for them to close then start sequence
        if (currentState == ElevatorState.Closing)
        {
            StartCoroutine(WaitForClosingThenStartSequence(onSequenceComplete));
            return;
        }
        
        // If doors are opening, wait for them to open then execute callback
        if (currentState == ElevatorState.Opening)
        {
            StartCoroutine(WaitForOpeningThenExecuteCallback(onSequenceComplete));
            return;
        }
        
        // Reset state if needed
        if (currentState == ElevatorState.Moving)
        {
            currentState = ElevatorState.Idle;
        }
        
        if (currentState == ElevatorState.Idle)
        {
            StartCoroutine(ElevatorSequenceCoroutine(onSequenceComplete));
        }
        else
        {
            onSequenceComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Opens doors and executes callback when reaching Open state
    /// </summary>
    private IEnumerator ElevatorSequenceCoroutine(System.Action onSequenceComplete)
    {
        yield return StartCoroutine(AnimateDoors(true, doorOpenDuration, ElevatorState.Opening, ElevatorState.Open));
        onSequenceComplete?.Invoke();
    }
    
    /// <summary>
    /// Opens doors when arriving at a new scene (called when scene loads)
    /// </summary>
    public void OpenDoorsOnArrival()
    {
        if (IsAnimatorValid())
        {
            if (gameObject.activeInHierarchy)
            {
                StartCoroutine(OpenDoorsOnArrivalCoroutine());
            }
            else
            {
                SetDoorState(true, ElevatorState.Open);
            }
        }
    }
    
    /// <summary>
    /// Opens doors with delay when arriving at new scene
    /// </summary>
    private IEnumerator OpenDoorsOnArrivalCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AnimateDoors(true, doorOpenDuration, ElevatorState.Opening, ElevatorState.Open));
    }
    
    /// <summary>
    /// Waits for doors to finish closing and starts the elevator sequence
    /// </summary>
    private IEnumerator WaitForClosingThenStartSequence(System.Action onSequenceComplete)
    {
        yield return StartCoroutine(WaitForState(ElevatorState.Closing, () => {
            if (currentState == ElevatorState.Idle)
            {
                StartCoroutine(ElevatorSequenceCoroutine(onSequenceComplete));
            }
            else
            {
                onSequenceComplete?.Invoke();
            }
        }));
    }
    
    /// <summary>
    /// Waits for doors to finish opening and executes callback
    /// </summary>
    private IEnumerator WaitForOpeningThenExecuteCallback(System.Action onSequenceComplete)
    {
        yield return StartCoroutine(WaitForState(ElevatorState.Opening, () => onSequenceComplete?.Invoke()));
    }
    
    /// <summary>
    /// Resets elevator to initial state (idle) with doors closed
    /// </summary>
    [ContextMenu("Reset Elevator State")]
    public void ResetElevatorState()
    {
        var previousState = currentState;
        currentState = ElevatorState.Idle;
        
        if (IsAnimatorValid() && previousState != ElevatorState.Opening && previousState != ElevatorState.Closing)
        {
            doorAnimator.SetBool(doorsOpenBool, false);
        }
    }
    
    /// <summary>
    /// Checks if the door animator is valid
    /// </summary>
    private bool IsAnimatorValid()
    {
        return doorAnimator != null && doorAnimator.runtimeAnimatorController != null;
    }
    
    /// <summary>
    /// Sets elevator state and animator parameter
    /// </summary>
    private void SetDoorState(bool open, ElevatorState state)
    {
        currentState = state;
        doorAnimator.SetBool(doorsOpenBool, open);
    }
    
    /// <summary>
    /// Animates doors through state transitions
    /// </summary>
    private IEnumerator AnimateDoors(bool open, float duration, ElevatorState duringState, ElevatorState endState)
    {
        currentState = duringState;
        doorAnimator.SetBool(doorsOpenBool, open);
        yield return new WaitForSeconds(duration);
        currentState = endState;
    }
    
    /// <summary>
    /// Waits for the elevator to reach the target state
    /// </summary>
    private IEnumerator WaitForState(ElevatorState targetState, System.Action onComplete)
    {
        while (currentState == targetState)
        {
            yield return new WaitForSeconds(0.1f);
        }
        onComplete?.Invoke();
    }
}