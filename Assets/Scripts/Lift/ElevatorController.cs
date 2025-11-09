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

    // Sound clips for the elevator doors and movement
    [Header("Audio Clips")]
    public AudioClip doorClip;
    public AudioClip moveClip;
    public AudioClip arrivalClip;

    [Header("Audio Settings")]
    [Range(0f, 1f)] public float doorVolume = 1.0f;
    [Range(0f, 1f)] public float moveVolume = 1.0f;
    [Range(0f, 1f)] public float arrivalVolume = 1.0f;

    [Tooltip("3D sound: 0 = 2D, 1 = full 3D")]
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
    public float minDistance = 1f;
    public float maxDistance = 20f;

    // Dedicated audio sources to avoid overlap
    private AudioSource doorSource;
    private AudioSource moveSource;
    private AudioSource arrivalSource;

    void Awake()
    {
        // Prepare reusable audio sources (no prefabs)
        doorSource    = EnsureSource("DoorSource");
        moveSource    = EnsureSource("MoveSource");
        arrivalSource = EnsureSource("ArrivalSource");

        ApplyCommonAudioSettings(doorSource);
        ApplyCommonAudioSettings(moveSource);
        ApplyCommonAudioSettings(arrivalSource);
    }

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
        // Play arrival sound (non-overlapping)
        if (arrivalClip != null)
        {
            PlayNonOverlapping(arrivalSource, arrivalClip, false, arrivalVolume, restartIfPlaying:true);
        }
        else
        {
            Debug.LogWarning("Arrival clip is not assigned in ElevatorController.");
        }
            
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
        // Play move sound (non-overlapping)
        if (moveClip != null)
        {
            PlayNonOverlapping(moveSource, moveClip, false, moveVolume, restartIfPlaying:true);
        }
        else
        {
            Debug.LogWarning("Move clip is not assigned in ElevatorController.");
        }

        yield return new WaitForSeconds(4.0f);
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
        
        // Play door sound effect (restart to sync with each animation trigger)
        if (doorClip != null)
        {
            PlayNonOverlapping(doorSource, doorClip, false, doorVolume, restartIfPlaying:true);
        }
        else
        {
            Debug.LogWarning("Door clip is not assigned in ElevatorController.");
        }

        if (open)
        {
            doorAnimator.SetBool("DoorsOpen", true);
            yield return new WaitForSeconds(2.02f);
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
        
        float duration = 2.02f;
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

    // ---------------------------
    // Audio helpers
    // ---------------------------

    private AudioSource EnsureSource(string nameSuffix)
    {
        // Try to reuse if already present
        var src = gameObject.AddComponent<AudioSource>();
        src.playOnAwake   = false;
        src.loop          = false;
        src.spatialBlend  = spatialBlend;
        src.rolloffMode   = rolloffMode;
        src.minDistance   = Mathf.Max(0.01f, minDistance);
        src.maxDistance   = Mathf.Max(src.minDistance, maxDistance);
        src.dopplerLevel  = 0f;
        src.name          = name + "_" + nameSuffix;
        return src;
    }

    private void ApplyCommonAudioSettings(AudioSource src)
    {
        if (src == null) return;
        src.spatialBlend = spatialBlend;
        src.rolloffMode  = rolloffMode;
        src.minDistance  = Mathf.Max(0.01f, minDistance);
        src.maxDistance  = Mathf.Max(src.minDistance, maxDistance);
    }

    /// <summary>
    /// Plays a clip on a dedicated source without creating new sources.
    /// Optionally restarts if already playing to keep it in sync.
    /// </summary>
    private void PlayNonOverlapping(AudioSource src, AudioClip clip, bool loop, float vol, bool restartIfPlaying)
    {
        if (src == null || clip == null) return;

        // Apply common settings in case inspector values changed at runtime
        ApplyCommonAudioSettings(src);

        src.loop   = loop;
        src.clip   = clip;
        src.volume = Mathf.Clamp01(vol);

        if (src.isPlaying)
        {
            if (restartIfPlaying)
            {
                src.Stop();
                src.Play();
            }
            // else: keep current playback, do not stack
        }
        else
        {
            src.Play();
        }
    }
}
