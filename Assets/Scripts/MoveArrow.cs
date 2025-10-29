using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;

public class MoveArrow : MonoBehaviour, ITappable
{
    /// <summary>
    /// Point to move to.
    /// </summary>
    public MovablePoint endpoint;

    /// <summary>
    /// Tells movable point not to delete this arrow as it
    /// is still moving the camera into the right position.
    /// </summary>
    public bool moving = false;

    /// <summary>
    /// The xrOrigin in the scene to move.
    /// </summary>
    private XROrigin xrOrigin;

    /// <summary>
    /// How long it takes in seconds to move to the next place over tapping the arrow.
    /// </summary>
    public float moveDuration = 6f;
    /// <summary>
    /// Gets current XROrigin.
    /// </summary>
    void Start() 
    {
        xrOrigin = FindObjectOfType<XROrigin>();
    }

    /// <summary>
    /// Moves to point when tapped.
    /// </summary>
    public void OnTapped()
    {
        if (xrOrigin == null || endpoint == null)
        {
            return;
        }

        // Bereken doelpositie relatief aan camera
        Vector3 offset = endpoint.transform.position - Camera.main.transform.position;
        offset.y = 0;

        Vector3 targetPosition = xrOrigin.transform.position + offset;

        moving = true;
        // Start smooth move
        StartCoroutine(SmoothMove(xrOrigin.transform, targetPosition, moveDuration));
    }

    /// <summary>
    /// Smoothly moves an object to a target position.
    /// </summary>
    private IEnumerator SmoothMove(Transform obj, Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = obj.position;
        float normalizedTime = 0f;

        while (normalizedTime < 1f)
        {
            normalizedTime += Time.deltaTime / duration;
            obj.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            yield return null;
        }

        obj.position = targetPosition;
        Destroy(gameObject);
    }
}
