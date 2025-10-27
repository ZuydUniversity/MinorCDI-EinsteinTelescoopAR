using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    [Header("Auto Rotation Settings")]
    /// <summary>
    /// Duration in seconds for automatic player rotation animations
    /// </summary>
    public float autoRotationDuration = 1.5f;
    /// <summary>
    /// List of movement points that trigger automatic rotation with their respective rotation angles
    /// </summary>
    public List<AutoRotationConfig> autoRotationConfigs = new List<AutoRotationConfig> 
    { 
        new AutoRotationConfig { pointID = "2", rotationDegrees = 180f }
    };
    
    /// <summary>
    /// Configuration class for automatic rotation settings at specific movement points
    /// </summary>
    [System.Serializable]
    public class AutoRotationConfig
    {
        /// <summary>
        /// ID of the movement point that triggers automatic rotation
        /// </summary>
        public string pointID;
        /// <summary>
        /// Degrees to rotate the player when reaching this point
        /// </summary>
        public float rotationDegrees = 180f;
    }
    
    [Header("Elevator Integration")]
    /// <summary>
    /// Reference to the elevator controller for door operations
    /// </summary>
    public ElevatorController elevatorController;
    /// <summary>
    /// List of movement point IDs that trigger elevator door closing
    /// </summary>
    public List<string> elevatorClosePointIDs = new List<string> { "3" };
    /// <summary>
    /// Movement point ID that triggers automatic scene transition
    /// </summary>
    public string sceneTransitionPointID = "2";
    /// <summary>
    /// Delay in seconds before closing elevator doors after reaching trigger points
    /// </summary>
    public float elevatorCloseDelay = 1f;
    /// <summary>
    /// Delay before starting automatic scene transition
    /// </summary>
    public float sceneTransitionDelay = 1f;

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
    /// Flag indicating whether the player is currently performing an automatic rotation
    /// </summary>
    private bool isAutoRotating = false;
    /// <summary>
    /// List to track active arrow GameObjects for efficient cleanup
    /// </summary>
    private List<GameObject> activeArrows = new List<GameObject>();
    
    /// <summary>
    /// Tracks the last clicked ARObjectSceneSwitcher for automatic scene transitions
    /// </summary>
    private static ARObjectSceneSwitcher lastClickedSwitcher;
    
    /// <summary>
    /// Gets the current movement point the player is at
    /// </summary>
    public MovementPoint GetCurrentPoint()
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
    /// Unity LateUpdate function, maintains xrOrigin at fixed position
    /// </summary>
    private void LateUpdate()
    {
        if (xrOrigin == null)
        {
            return;
        }
        xrOrigin.transform.position = fixedPosition;
    }

    /// <summary>
    /// Moves the player to a movementpoint (called when a movementarrow is tapped)
    /// </summary>
    public void MoveToPoint(MovementPoint newPoint)
    {
        if (isAutoRotating)
        {
            return;
        }
        
        currentPoint = newPoint;
        StartCoroutine(SmoothMove(xrOrigin.transform, newPoint, 2f));

        foreach (var arrow in activeArrows)
        {
            if (arrow != null)
            {
                Destroy(arrow);
            }
        }
        activeArrows.Clear();
    }

    /// <summary>
    /// Creates movement arrows for possible movement directions
    /// </summary>
    private void CreateArrowTo(MovementPoint target)
    {
        Vector3 arrowPosition = target.GetArrowDisplayPosition();
        Quaternion arrowRotation = target.GetArrowDisplayRotation();
        
        var arrow = Instantiate(arrowButtonPrefab, arrowPosition, arrowRotation);
        arrow.tag = "MoveArrow";
        arrow.GetComponent<Canvas>().worldCamera = Camera.main;
        activeArrows.Add(arrow);

        if (target.arrowDisplayTransform == null)
        {
            Vector3 cameraPosition = mainCamera.transform.position;
            cameraPosition.y = arrow.transform.position.y; 
            arrow.transform.LookAt(cameraPosition);
        }

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
    /// Smoothly moves player to new point with animation
    /// </summary>
    private IEnumerator SmoothMove(Transform obj, MovementPoint newPoint, float duration)
    {
        Vector3 startPos = obj.position;
        Vector3 targetPos = newPoint.GetPlayerDestination();
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            fixedPosition = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        fixedPosition = targetPos;

        var rotationConfig = autoRotationConfigs.Find(config => config.pointID == newPoint.pointID);
        if (rotationConfig != null)
        {
            yield return StartCoroutine(AutoRotatePlayer(rotationConfig.rotationDegrees));
        }

        if (elevatorClosePointIDs.Contains(newPoint.pointID))
        {
            CloseElevatorDoorsDelayed();
        }
        
        if (newPoint.pointID == sceneTransitionPointID)
        {
            StartAutomaticSceneTransition();
        }
        
        foreach (var target in newPoint.connectedPoints)
        {
            CreateArrowTo(target);
        }
    }
        
    /// <summary>
    /// Automatically rotates the player by specified degrees while maintaining position
    /// </summary>
    private IEnumerator AutoRotatePlayer(float degrees = 180f)
    {
        isAutoRotating = true;
        
        Vector3 currentPosition = fixedPosition;
        Vector3 startRotation = xrOrigin.transform.eulerAngles;
        Vector3 targetRotation = startRotation + new Vector3(0, degrees, 0);
        
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / autoRotationDuration;
            Vector3 currentRotation = Vector3.Lerp(startRotation, targetRotation, t);
            xrOrigin.transform.eulerAngles = currentRotation;
            
            fixedPosition = currentPosition;
            yield return null;
        }
        
        xrOrigin.transform.eulerAngles = targetRotation;
        fixedPosition = currentPosition;
        isAutoRotating = false;
    }
    
    /// <summary>
    /// Starts automatic scene transition sequence at MovementPoint2
    /// </summary>
    private void StartAutomaticSceneTransition()
    {
        StartCoroutine(AutomaticSceneTransitionCoroutine());
    }
    
    /// <summary>
    /// Closes elevator doors after a configurable delay
    /// </summary>
    private void CloseElevatorDoorsDelayed()
    {
        StartCoroutine(CloseElevatorDoorsCoroutine());
    }
    
    /// <summary>
    /// Coroutine for automatic scene transition at MovementPoint2
    /// </summary>
    private IEnumerator AutomaticSceneTransitionCoroutine()
    {
        yield return new WaitForSeconds(sceneTransitionDelay);
        
        if (lastClickedSwitcher != null)
        {
            lastClickedSwitcher.CheckPositionCloseDoorsAndLoadScene();
        }
    }
    
    /// <summary>
    /// Coroutine to close elevator doors with delay
    /// </summary>
    private IEnumerator CloseElevatorDoorsCoroutine()
    {
        yield return new WaitForSeconds(elevatorCloseDelay);
        
        FindActiveElevatorController();
        
        if (elevatorController != null && elevatorController.gameObject.activeInHierarchy)
        {
            elevatorController.CloseDoors();
        }
    }
    
    private void FindActiveElevatorController()
    {
        var allElevatorControllers = FindObjectsByType<ElevatorController>(FindObjectsSortMode.None);
        elevatorController = null;
        
        foreach (var controller in allElevatorControllers)
        {
            if (controller.gameObject.activeInHierarchy)
            {
                elevatorController = controller;
                break;
            }
        }
    }
}