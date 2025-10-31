using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Implements a movable point which shows and hides arrows to
/// other movable points.
/// </summary>
public class MovablePoint : MonoBehaviour
{
    [Header("Movement Configuration")]
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

    [Header("Point Identification")]
    /// <summary>
    /// Unique identifier for this movement point
    /// </summary>
    public string pointID = "";

    [Header("Elevator Integration")]
    /// <summary>
    /// Whether this point triggers elevator door closing
    /// </summary>
    public bool triggersElevatorClose = false;
    /// <summary>
    /// Delay before closing elevator doors
    /// </summary>
    public float elevatorCloseDelay = 1f;

    [Header("Scene Transition")]
    /// <summary>
    /// Whether this point triggers automatic scene transition
    /// </summary>
    public bool triggersSceneTransition = false;
    /// <summary>
    /// Delay before starting scene transition
    /// </summary>
    public float sceneTransitionDelay = 1f;

    [Header("Auto Rotation")]
    /// <summary>
    /// Whether this point triggers automatic player rotation
    /// </summary>
    public bool hasAutoRotation = false;
    /// <summary>
    /// Degrees to rotate the player when reaching this point
    /// </summary>
    public float rotationDegrees = 180f;
    /// <summary>
    /// Duration of the rotation animation
    /// </summary>
    public float rotationDuration = 1.5f;

    /// <summary>
    /// List of created arrows.
    /// </summary>
    private List<MoveArrow> arrows = new List<MoveArrow>();

    /// <summary>
    /// Is the camera on the plane.
    /// </summary>
    private bool onPlane = false;

    /// <summary>
    /// Whether this point has been reached via arrow movement (not just by being in the plane)
    /// </summary>
    private bool reachedViaMovement = false;

    /// <summary>
    /// Static reference to track current movement point
    /// </summary>
    private static MovablePoint currentPoint;

    /// <summary>
    /// Static reference to track last clicked ARObjectSceneSwitcher
    /// </summary>
    private static ARObjectSceneSwitcher lastClickedSwitcher;

    /// <summary>
    /// Event triggered when player moves to a new MovablePoint
    /// </summary>
    public static System.Action<MovablePoint> OnPlayerMoved;

    /// <summary>
    /// Gets the current movement point the player is at
    /// </summary>
    public static MovablePoint GetCurrentPoint()
    {
        return currentPoint;
    }

    /// <summary>
    /// Registers which ARObjectSceneSwitcher was last clicked for automatic scene transitions
    /// </summary>
    public static void RegisterLastClickedSwitcher(ARObjectSceneSwitcher switcher)
    {
        lastClickedSwitcher = switcher;
    }

    /// <summary>
    /// Marks this point as reached via arrow movement
    /// </summary>
    public void MarkAsReachedViaMovement()
    {
        reachedViaMovement = true;
    }

    /// <summary>
    /// Triggers movement effects directly (for use after smooth movement completion)
    /// </summary>
    public void TriggerMovementEffects()
    {
        currentPoint = this;
        OnPlayerMoved?.Invoke(this);
        reachedViaMovement = false;
        StartCoroutine(HandleMovementEffects());
    }

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
        // Always update currentPoint for position tracking
        currentPoint = this;

        // Only trigger movement effects if reached via arrow movement
        if (reachedViaMovement)
        {
            OnPlayerMoved?.Invoke(this);
            StartCoroutine(HandleMovementEffects());
            reachedViaMovement = false; // Reset flag
        }

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

    /// <summary>
    /// Handles movement effects like auto-rotation, elevator doors, and scene transitions
    /// </summary>
    private IEnumerator HandleMovementEffects()
    {
        if (hasAutoRotation)
        {
            yield return StartCoroutine(AutoRotatePlayer());
        }

        if (triggersElevatorClose)
        {
            StartCoroutine(CloseElevatorDoorsDelayed());
        }

        if (triggersSceneTransition)
        {
            StartCoroutine(StartAutomaticSceneTransition());
        }
    }

    /// <summary>
    /// Automatically rotates the player by specified degrees
    /// </summary>
    private IEnumerator AutoRotatePlayer()
    {
        var xrOrigin = FindObjectOfType<Unity.XR.CoreUtils.XROrigin>();
        if (xrOrigin == null || Camera.main == null) yield break;

        Vector3 cameraWorldPosition = Camera.main.transform.position;
        Vector3 startRotation = xrOrigin.transform.eulerAngles;
        Vector3 targetRotation = startRotation + new Vector3(0, rotationDegrees, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / rotationDuration;

            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, t);
            xrOrigin.transform.eulerAngles = currentRotation;

            Vector3 newCameraPosition = Camera.main.transform.position;
            Vector3 positionDrift = cameraWorldPosition - newCameraPosition;
            xrOrigin.transform.position += positionDrift;

            yield return null;
        }

        xrOrigin.transform.eulerAngles = targetRotation;

        Vector3 finalCameraPosition = Camera.main.transform.position;
        Vector3 finalDrift = cameraWorldPosition - finalCameraPosition;
        xrOrigin.transform.position += finalDrift;
    }

    /// <summary>
    /// Closes elevator doors after a delay
    /// </summary>
    private IEnumerator CloseElevatorDoorsDelayed()
    {
        yield return new WaitForSeconds(elevatorCloseDelay);

        var elevatorController = FindActiveElevatorController();
        if (elevatorController != null && elevatorController.gameObject.activeInHierarchy)
        {
            elevatorController.CloseDoors();
        }
    }

    /// <summary>
    /// Starts automatic scene transition after a delay
    /// </summary>
    private IEnumerator StartAutomaticSceneTransition()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);

        if (lastClickedSwitcher != null)
        {
            lastClickedSwitcher.CheckPositionCloseDoorsAndLoadScene();
        }
    }

    /// <summary>
    /// Finds the active elevator controller in the scene
    /// </summary>
    private ElevatorController FindActiveElevatorController()
    {
        var allElevatorControllers = FindObjectsByType<ElevatorController>(FindObjectsSortMode.None);

        foreach (var controller in allElevatorControllers)
        {
            if (controller.gameObject.activeInHierarchy)
            {
                return controller;
            }
        }

        return null;
    }
}