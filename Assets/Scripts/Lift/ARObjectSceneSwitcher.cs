using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Collections;

public class ARObjectSceneSwitcher : MonoBehaviour
{
    /// <summary>
    /// The name of the scene that is loaded
    /// </summary>
    [SerializeField] private string targetSceneName = "Above Ground";
    /// <summary>
    /// The maximum distance at which a raycast can be performed
    /// </summary>
    [SerializeField] private float maxRaycastDistance = 50f;
    /// <summary>
    /// The collider that is linked to the object for touch interaction
    /// </summary>
    [SerializeField] private Collider targetCollider;
    /// <summary>
    /// Reference to the ElevatorController to play animations before scene transition
    /// </summary>
    [SerializeField] private ElevatorController elevatorController;
    /// <summary>
    /// Reference to the MovementManager to check current player position (found dynamically)
    /// </summary>
    private MovementManager movementManager;

    /// <summary>
    /// AR Camera used for raycasting
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// The name of the main scene that may not be unloaded.
    /// </summary>
    public string mainSceneName = "ElevatorScene";

    /// <summary>
    /// Initializes camera and creates a BoxCollider for the specific prefab
    /// </summary>
    void Awake()
    {
        mainCamera = Camera.main;

        if (elevatorController == null)
        {
            elevatorController = GetComponentInParent<ElevatorController>();
        }
        
        if (movementManager == null)
        {
            movementManager = FindFirstObjectByType<MovementManager>();
        }

        if (targetCollider == null)
        {
            if (!TryGetComponent(out targetCollider))
            {
                var box = gameObject.AddComponent<BoxCollider>();
                var renderers = GetComponents<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                    box.center = transform.InverseTransformPoint(b.center);
                    box.size = transform.InverseTransformVector(b.size);
                }
                targetCollider = box;
            }
        }
    }

    /// <summary>
    /// Check if it runs on phone/tablet or in Unity Editor and use the appropriate touch
    /// </summary>
    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckTouch(Touchscreen.current.primaryTouch.position.ReadValue());
        }

        #if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckTouch(Mouse.current.position.ReadValue());
        }
        #endif
    }

    /// <summary>
    /// Checks whether the touch/raycast hits a Collider that is set
    /// </summary>
    void CheckTouch(Vector2 screenPos)
    {
        if (mainCamera != null && targetCollider != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, maxRaycastDistance) && hit.collider == targetCollider)
            {
                // Only respond to touch if not at MovementPoint2
                if (movementManager != null)
                {
                    var currentPoint = movementManager.GetCurrentPoint();
                    if (currentPoint != null && currentPoint.pointID == "2")
                    {
                        return;
                    }
                }
                
                // Register this switcher as the last clicked one
                MovementManager.RegisterLastClickedSwitcher(this);
                StartElevatorSequence();
            }
        }
    }

    /// <summary>
    /// Starts the elevator animation sequence and then loads the target scene
    /// </summary>
    void StartElevatorSequence()
    {
        if (elevatorController != null)
        {
            // First open doors, then check position and close doors before scene load
            elevatorController.StartElevatorSequence(CheckPositionCloseDoorsAndLoadScene);
        }
        else
        {
            CheckPositionCloseDoorsAndLoadScene();
        }
    }

    /// <summary>
    /// Checks position, closes doors if at MovementPoint2, then loads scene
    /// </summary>
    public void CheckPositionCloseDoorsAndLoadScene()
    {
        if (movementManager != null)
        {
            var currentPoint = movementManager.GetCurrentPoint();
            if (currentPoint == null || currentPoint.pointID != "2")
            {
                return;
            }
        }
        
        // At MovementPoint2, close doors and load scene
        if (elevatorController != null)
        {
            elevatorController.CloseDoors();
            // Wait for doors to close and load scene
            StartCoroutine(WaitForDoorsToCloseAndLoadScene());
        }
        else
        {
            LoadTargetScene();
        }
    }
    
    
    /// <summary>
    /// Load the targetScene that has been set
    /// </summary>
    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name != mainSceneName)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }

            SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
            
            
            // Use fixed 2 second delay for door opening after scene load
            Invoke(nameof(OpenElevatorDoorsDelayed), 2f);
        }
    }
    
    /// <summary>
    /// Waits for elevator doors to close and loads the target scene
    /// </summary>
    private System.Collections.IEnumerator WaitForDoorsToCloseAndLoadScene()
    {
        // Wait for doors to start closing
        yield return new WaitForSeconds(0.1f);
        
        // Wait until doors are closed
        while (elevatorController != null && 
               (elevatorController.currentState == ElevatorController.ElevatorState.Closing ||
                elevatorController.currentState == ElevatorController.ElevatorState.Open))
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        LoadTargetScene();
    }
    
    
    /// <summary>
    /// Simple delayed door opening method called via Invoke
    /// </summary>
    private void OpenElevatorDoorsDelayed()
    {
        if (elevatorController != null)
        {
            elevatorController.OpenDoorsOnArrival();
        }
    }
    
    /// <summary>
    /// Gets the target scene name for this ARObjectSceneSwitcher
    /// </summary>
    public string GetTargetSceneName()
    {
        return targetSceneName;
    }
    
}