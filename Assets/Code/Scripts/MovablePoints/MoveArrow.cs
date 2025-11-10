using UnityEngine;
using Unity.XR.CoreUtils;
using System.Collections;

/// <summary>
/// The arrows used by the movablepoint. These
/// arrows are responsable for moving the user to
/// the next movable point.
/// </summary>
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
    /// How long the user should take to get to the next movable point.
    /// </summary>
    public float moveDuration = 6f;

    /// <summary>
    /// The xrOrigin in the scene to move.
    /// </summary>
    private XROrigin xrOrigin;

    /// <summary>
    /// Gets current XROrigin.
    /// </summary>
    void Start() 
    {
        xrOrigin = FindFirstObjectByType<XROrigin>();
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

        // Calculate the goal position relative to the camera.
        Vector3 offset = endpoint.transform.position - Camera.main.transform.position;
        offset.y = 0;

        Vector3 targetPosition = xrOrigin.transform.position + offset;
        StartCoroutine(SmoothMove(xrOrigin.transform, targetPosition, moveDuration));
    }

    /// <summary>
    /// Smoothly moves an object to a target position.
    /// </summary>
    /// <param name="objectToMove" type="Transform">The transform of the object to move to the target position.</param>
    /// <param name="targetPosition" type="Vector3">The position to move the object to.</param>
    /// <param name="duration" type="float">The time the movement should take to be completed.</param>
    private IEnumerator SmoothMove(Transform objectToMove, Vector3 targetPosition, float duration)
    {
        moving = true;

        Vector3 startPosition = objectToMove.position;
        float normalizedTime = 0f;
        while (normalizedTime < 1f)
        {
            normalizedTime += Time.deltaTime / duration;
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, normalizedTime);
            yield return null;
        }

        objectToMove.position = targetPosition;
        Destroy(gameObject);
    }
}