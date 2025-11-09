using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Implements a movable point which shows and hides arrows to
/// other movable points.
/// </summary>
public class MovablePoint : MonoBehaviour
{
    /// <summary>
    /// The points that can be moved to from this point.
    /// </summary>
    public MovablePoint[] movablePoints;
    /// <summary>
    /// The prefab for the arrows that allow the user to move.
    /// </summary>
    public MoveArrow arrowPrefab;

    /// <summary>
    /// Offset for the arrow on the x and z axis.
    /// </summary>
    public float arrowOffset = 0.5f;
    /// <summary>
    /// Size of the planes x and z axis.
    /// </summary>
    public Vector3 planeSize = new Vector3(1f, 0f, 1f);

    /// <summary>
    /// List of created arrows.
    /// </summary>
    private List<MoveArrow> arrows = new List<MoveArrow>();

    /// <summary>
    /// Is the camera on the plane.
    /// </summary>
    private bool onPlane = false;

    /// <summary>
    /// Checks if camera is on plane.
    /// </summary>
    void Update() 
    {
        if (Camera.main != null)
        {
            Vector3 cameraPosition = Camera.main.transform.position;
            Vector3 planePosition = gameObject.transform.position;
        
            bool currentlyInside = 
                cameraPosition.x >= planePosition.x - planeSize.x / 2 &&
                cameraPosition.x <= planePosition.x + planeSize.x / 2 &&
                cameraPosition.z >= planePosition.z - planeSize.z / 2 &&
                cameraPosition.z <= planePosition.z + planeSize.z / 2;

            if (currentlyInside && !onPlane) 
            {
                OnCameraEnter();
            } 
            else if (!currentlyInside && onPlane) 
            {
                OnCameraLeave();
            }
        }
    }

    /// <summary>
    /// Draws plane in editor.
    /// </summary>
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, planeSize);
    }

    /// <summary>
    /// Creates all arrows on entering the movable point.
    /// </summary>
    private void OnCameraEnter()
    {    
        foreach (MovablePoint movablePoint in movablePoints)
        {
            MoveArrow newArrow = Instantiate(arrowPrefab, gameObject.transform.position, Quaternion.identity);
            newArrow.transform.SetParent(gameObject.transform);
            newArrow.transform.LookAt(movablePoint.transform);

            Vector3 currentRotation = newArrow.transform.eulerAngles;
            currentRotation.x = -30f;
            newArrow.transform.eulerAngles = currentRotation;

            Vector3 offsetDirection = newArrow.transform.forward;
            offsetDirection.x *= arrowOffset + (planeSize.x / 2);
            offsetDirection.y = 0.4f;
            offsetDirection.z *= arrowOffset + (planeSize.z / 2);
            newArrow.transform.position += offsetDirection;

            newArrow.endpoint = movablePoint;
            arrows.Add(newArrow);
        }

        onPlane = true;
    }

    /// <summary>
    /// Destroy's all arrows on leaving the movable point.
    /// </summary>
    private void OnCameraLeave()
    {
        foreach (MoveArrow arrow in arrows) 
        {
            if (!arrow.moving) 
            {
                Destroy(arrow.gameObject);
            }
        }

        arrows.Clear();
        onPlane = false;
    }
}