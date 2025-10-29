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
            return;

        // Bereken doelpositie relatief aan camera
        Vector3 offset = endpoint.transform.position - Camera.main.transform.position;
        offset.y = 0;

        Vector3 targetPos = xrOrigin.transform.position + offset;

        // Start smooth move
        StartCoroutine(SmoothMove(xrOrigin.transform, targetPos, moveDuration));
    }

    /// <summary>
    /// Smoothly moves an object to a target position.
    /// </summary>
    private IEnumerator SmoothMove(Transform obj, Vector3 targetPos, float duration)
    {
        Vector3 startPos = obj.position;
        Quaternion startRot = obj.rotation;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        obj.position = targetPos;
    }
}
