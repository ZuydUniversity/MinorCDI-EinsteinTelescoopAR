using System.Collections;
using UnityEngine;

public class AnimateModel : MonoBehaviour
{
    [Header("Animation Settings")]
    [Tooltip("The duration of the animation in seconds.")]
    public float animationDuration = 5.0f;

    [Tooltip("How far down the object should move in meters.")]
    public float targetDownDistance = 300.0f;

    [Tooltip("How many times larger the object should become.")]
    public float targetScaleMultiplier = 1000.0f;

    // To store the object's original state
    private Vector3 originalPosition;
    private Vector3 originalScale;
    
    // To store the target state
    private Vector3 targetPosition;
    private Vector3 targetScale;

    // To prevent starting the animation while it's already running
    private bool isAnimating = false;

    // This is called when the script instance is being loaded
    void Start()
    {
        // Store the initial position and scale so we can animate from them
        originalPosition = transform.position;
        originalScale = transform.localScale;

        // Calculate the target position and scale based on the public variables
        targetPosition = originalPosition - new Vector3(0, targetDownDistance, 0);
        targetScale = originalScale * targetScaleMultiplier;
    }

    // This is the public function you will call "on command"
    public void StartAnimation()
    {
        // If the animation is not already running, start it
        if (!isAnimating)
        {
            StartCoroutine(AnimateObject());
        }
    }

    // The Coroutine that handles the animation over time
    private IEnumerator AnimateObject()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        // Loop until the elapsed time is greater than the set duration
        while (elapsedTime < animationDuration)
        {
            // Calculate the current progress of the animation (a value from 0 to 1)
            float t = elapsedTime / animationDuration;

            // Smoothly interpolate the position from original to target
            transform.position = Vector3.Lerp(originalPosition, targetPosition, t);

            // Smoothly interpolate the scale from original to target
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            // Increment the elapsed time by the time passed since the last frame
            elapsedTime += Time.deltaTime;

            // Wait for the next frame before continuing the loop
            yield return null;
        }

        // --- Animation finished ---
        // To be perfectly precise, ensure the object is exactly at its target state
        transform.position = targetPosition;
        transform.localScale = targetScale;
        
        isAnimating = false;
        Debug.Log("Animation Complete!");
    }
}