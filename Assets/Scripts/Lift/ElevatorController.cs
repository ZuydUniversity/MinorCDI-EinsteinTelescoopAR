using UnityEngine;
using System.Collections;

/// <summary>
/// Controls elevator door animations and state management
/// </summary>
public class ElevatorController : MonoBehaviour
{
    /// <summary>
    /// The Animator component that controls the door animations
    /// </summary>
    [SerializeField] private Animator doorAnimator;
    
    /// <summary>
    /// Possible states of the elevator doors
    /// </summary>
    public enum ElevatorState { Closed, Open, Animating }
    
    /// <summary>
    /// Current state of the elevator doors
    /// </summary>
    public ElevatorState currentState = ElevatorState.Closed;
    
    /// <summary>
    /// Initialize elevator state and ensure doors are closed
    /// </summary>
    void Start()
    {
        if (doorAnimator == null)
        {
            doorAnimator = GetComponent<Animator>();
        }

        currentState = ElevatorState.Closed;
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("DoorsOpen", false);
        }
    }
    
    /// <summary>
    /// Opens the elevator doors if they are currently closed
    /// </summary>
    public void OpenDoors()
    {
        if (doorAnimator != null && currentState == ElevatorState.Closed)
        {
            StartCoroutine(AnimateDoors(true, null));
        }
    }

    /// <summary>
    /// Closes the elevator doors if they are currently open
    /// </summary>
    public void CloseDoors()
    {
        if (doorAnimator != null && currentState == ElevatorState.Open)
        {
            StartCoroutine(AnimateDoors(false, null));
        }
    }
    
    /// <summary>
    /// Starts the elevator sequence by opening doors and executing callback when complete
    /// </summary>
    /// <param name="onSequenceComplete">Callback to execute when doors are fully open</param>
    public void StartElevatorSequence(System.Action onSequenceComplete = null)
    {
        if (!gameObject.activeInHierarchy || doorAnimator == null)
        {
            onSequenceComplete?.Invoke();
            return;
        }
        
        if (currentState == ElevatorState.Open)
        {
            onSequenceComplete?.Invoke();
            return;
        }
        
        if (currentState == ElevatorState.Animating)
        {
            StartCoroutine(WaitForAnimationThenCallback(onSequenceComplete));
            return;
        }
        
        StartCoroutine(AnimateDoors(true, onSequenceComplete));
    }
    
    /// <summary>
    /// Opens doors when arriving at a new scene with delay and button reset
    /// </summary>
    public void OpenDoorsOnArrival()
    {
        if (doorAnimator == null)
        {
            return;
        }
        
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(OpenDoorsOnArrivalCoroutine());
        }
        else
        {
            currentState = ElevatorState.Open;
            doorAnimator.SetBool("DoorsOpen", true);
        }
    }
    
    /// <summary>
    /// Opens doors with delay when arriving at new scene and resets button states
    /// </summary>
    private IEnumerator OpenDoorsOnArrivalCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(AnimateDoors(true, null));
        
        if (ButtonEmissionManager.Instance != null)
        {
            ButtonEmissionManager.Instance.ResetAllButtons();
        }
    }
    
    /// <summary>
    /// Waits for current animation to complete then executes callback or opens doors
    /// </summary>
    /// <param name="onComplete">Callback to execute when animation is complete</param>
    private IEnumerator WaitForAnimationThenCallback(System.Action onComplete)
    {
        while (currentState == ElevatorState.Animating)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        if (currentState == ElevatorState.Closed)
        {
            yield return StartCoroutine(AnimateDoors(true, onComplete));
        }
        else
        {
            onComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// Animates doors opening or closing with optional callback when complete
    /// </summary>
    /// <param name="open">True to open doors, false to close them</param>
    /// <param name="onComplete">Callback to execute when animation is complete</param>
    private IEnumerator AnimateDoors(bool open, System.Action onComplete)
    {
        currentState = ElevatorState.Animating;
        
        if (open)
        {
            doorAnimator.SetBool("DoorsOpen", true);
            yield return new WaitForSeconds(2.1f);
        }
        else
        {
            yield return StartCoroutine(SmoothCloseDoors());
        }
        
        currentState = open ? ElevatorState.Open : ElevatorState.Closed;
        onComplete?.Invoke();
    }
    
    /// <summary>
    /// Smoothly closes doors by playing the opening animation in reverse
    /// </summary>
    private IEnumerator SmoothCloseDoors()
    {
        doorAnimator.SetBool("DoorsOpen", true);
        yield return new WaitForEndOfFrame();
        
        float duration = 2.1f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            float normalizedTime = 1.0f - (elapsedTime / duration);
            
            for (int layer = 0; layer < doorAnimator.layerCount; layer++)
            {
                AnimatorStateInfo stateInfo = doorAnimator.GetCurrentAnimatorStateInfo(layer);
                if (stateInfo.IsName("Door01Open") || stateInfo.IsName("Door02Open"))
                {
                    doorAnimator.Play(stateInfo.fullPathHash, layer, normalizedTime);
                }
            }
            
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        doorAnimator.SetBool("DoorsOpen", false);
    }
    
    /// <summary>
    /// Resets elevator to initial state with doors closed
    /// </summary>
    [ContextMenu("Reset Elevator State")]
    public void ResetElevatorState()
    {
        currentState = ElevatorState.Closed;
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("DoorsOpen", false);
        }
    }
}