using UnityEngine;

/// <summary>
/// Button component that opens elevator doors when tapped.
/// Only functions when elevator is in closed state.
/// </summary>
public class OpenButton : MonoBehaviour, ITappable
{
    [SerializeField] private AudioClip buttonClip;
    [SerializeField] private ElevatorController elevatorController;

    // Add a dedicated AudioSource on this GameObject
    private AudioSource audioSource;

    // (Optional) small debounce so rapid taps don’t spam sound
    [SerializeField] private float minInterval = 0.08f;
    private float lastPlayTime = -999f;

    void Awake()
    {
        // Ensure we have an AudioSource to reuse
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;   // 3D sound (set to 0 for 2D UI sound)
        audioSource.rolloffMode = AudioRolloffMode.Linear;
    }

    void Start()
    {
        if (elevatorController == null)
            elevatorController = FindFirstObjectByType<ElevatorController>();
    }

    /// <summary>
    /// Opens elevator doors when tapped and elevator is in closed state.
    /// </summary>
    public void OnTapped()
    {
        if (elevatorController != null &&
            elevatorController.currentState == ElevatorController.ElevatorState.Closed)
        {
            elevatorController.OpenDoors();
        }

        // Play button sound without overlap
        if (buttonClip != null)
        {
            if (Time.time - lastPlayTime < minInterval) return; // debounce

            // Option A: restart sound cleanly every tap (no overlap)
            audioSource.Stop();
            audioSource.clip = buttonClip;
            audioSource.Play();
            lastPlayTime = Time.time;

            // Option B (alternative): only play if not already playing
            // if (!audioSource.isPlaying) audioSource.PlayOneShot(buttonClip);
        }
        else
        {
            Debug.LogWarning("Button clip is not assigned in OpenButton.");
        }
    }
}
