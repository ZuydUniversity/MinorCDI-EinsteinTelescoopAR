using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MovementManager : MonoBehaviour
{
    /// <summary>
    /// player, gets moved
    /// </summary>
    public GameObject xrOrigin; 
    /// <summary>
    /// maincamera used for raytracing tap detection
    /// </summary>
    public Camera mainCamera;
    /// <summary>
    /// prefab for spawning arrows indication where you can move
    /// </summary>
    public GameObject arrowButtonPrefab;
    /// <summary>
    /// used to check where player can move to
    /// </summary>
    private Dictionary<string, MovementPoint> movementPoints = new();
    /// <summary>
    /// current movement point the user is at
    /// </summary>
    private MovementPoint currentPoint;
    /// <summary>
    /// fixed position used to move player, movement is disabled 
    /// </summary>
    private Vector3 fixedPosition;
    /// <summary>
    /// Unity start function, sets xrOrigin if null, fills movementPoints Dictionary, moves player to the start
    /// </summary>
    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("XR Origin (AR Rig)");
        }

        foreach (var point in gameObject.GetComponentsInChildren<MovementPoint>())
        {
            movementPoints[point.pointID] = point;
        }

        if (movementPoints.TryGetValue("1", out var start))
        {
            MoveToPoint(start);
        }
    }
    /// <summary>
    /// Unity Lateupdate function, makes sure the xrOrigin rotates with the main camera
    /// </summary>
    private void LateUpdate()
    {
        xrOrigin.transform.localPosition = fixedPosition;

        Vector3 euler = xrOrigin.transform.eulerAngles;
        euler.y = mainCamera.transform.eulerAngles.y;
        xrOrigin.transform.eulerAngles = euler;
    }
    /// <summary>
    /// Moves the player to a movementpoint (called when a movementarrow is tapped)
    /// </summary>
    /// <param name="newPoint"></param>
    public void MoveToPoint(MovementPoint newPoint)
    {
        currentPoint = newPoint;
        StartCoroutine(SmoothMove(xrOrigin.transform, newPoint, 2f));

        // Oude pijlen weg
        foreach (var arrow in GameObject.FindGameObjectsWithTag("MoveArrow"))
        {
            Destroy(arrow);
        }
    }
    /// <summary>
    /// summmon movementarrows based on possible movement directions
    /// </summary>
    /// <param name="target"></param>
    private void CreateArrowTo(MovementPoint target)
    {
        var arrow = Instantiate(arrowButtonPrefab, target.transform.position, Quaternion.identity);
        arrow.tag = "MoveArrow";
        arrow.GetComponent<Canvas>().worldCamera = Camera.main;

        // arrow pointed away from user
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.y = arrow.transform.position.y; 
        arrow.transform.LookAt(cameraPosition);

        // button listener
        Button[] buttons = arrow.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => MoveToPoint(target));
            }
        }
    }
    /// <summary>
    /// makes the player move more smoothly instead of teleporting
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="newPoint"></param>
    /// <param name="duration"></param>
    /// <returns></returns>
    private IEnumerator SmoothMove(Transform obj, MovementPoint newPoint, float duration)
        {
            Vector3 startPos = obj.position;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                fixedPosition = Vector3.Lerp(startPos, newPoint.transform.position, t);
                yield return null;
            }

            fixedPosition = newPoint.transform.position;

            // New arrows
            foreach (var target in newPoint.connectedPoints)
            {
                CreateArrowTo(target);
            }
        }
    }
