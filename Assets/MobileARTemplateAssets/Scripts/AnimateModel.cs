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

    private Vector3 originalPosition;
    private Vector3 originalScale;
    
    private Vector3 targetPosition;
    private Vector3 targetScale;

    private bool isAnimating = false;

    public void StartAnimation()
    {
        if (!isAnimating)
        {
            StartCoroutine(AnimateObject());
        }
    }

    private IEnumerator AnimateObject()
    {
        isAnimating = true;
        float elapsedTime = 0f;

        originalPosition = transform.position;
        originalScale = transform.localScale;

        targetPosition = originalPosition - new Vector3(0, targetDownDistance, 0);
        targetScale = originalScale * targetScaleMultiplier;

        while (elapsedTime < animationDuration)
        {
            float t = elapsedTime / animationDuration;

            transform.position = Vector3.Lerp(originalPosition, targetPosition, t);
            transform.localScale = Vector3.Lerp(originalScale, targetScale, t);

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;
        
        isAnimating = false;
        Debug.Log("Animation Complete!");
    }
}