using System.Collections.Generic;
using UnityEngine;

public class MovementPoint : MonoBehaviour
{
    [Header("Movement Configuration")]
    /// <summary>
    /// Id for point recognisation
    /// </summary>
    public string pointID;
    /// <summary>
    /// Point the player can move to from this point
    /// </summary>
    public List<MovementPoint> connectedPoints = new List<MovementPoint>();
    
    [Header("Arrow Display Configuration")]
    /// <summary>
    /// Transform used for positioning arrow displays
    /// </summary>
    public Transform arrowDisplayTransform;
    
    /// <summary>
    /// Gets the destination position where the player should move to
    /// </summary>
    public Vector3 GetPlayerDestination()
    {
        return transform.position;
    }
    
    /// <summary>
    /// Gets the position where arrows should be displayed for this point
    /// </summary>
    public Vector3 GetArrowDisplayPosition()
    {
        return arrowDisplayTransform != null ? arrowDisplayTransform.position : transform.position;
    }
    
    /// <summary>
    /// Gets the rotation for arrows displayed at this point
    /// </summary>
    public Quaternion GetArrowDisplayRotation()
    {
        return arrowDisplayTransform != null ? arrowDisplayTransform.rotation : transform.rotation;
    }
}