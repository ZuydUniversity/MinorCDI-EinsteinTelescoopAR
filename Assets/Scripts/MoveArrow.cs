using UnityEngine;
using Unity.XR.CoreUtils;

public class MoveArrow : MonoBehaviour, ITappable
{
    /// <summary>
    /// Point to move to.
    /// </summary>
    public MovablePoint endpoint;

    public AudioSource audioSource;

    /// <summary>
    /// The xrOrigin in the scene to move.
    /// </summary>
    private XROrigin xrOrigin;

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
        audioSource.Play();
        Vector3 offset = endpoint.transform.position;
        offset.x -= Camera.main.transform.position.x;
        offset.y = 0;
        offset.z -= Camera.main.transform.position.z;

        xrOrigin.transform.position += offset;


    }
}
