using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ARObjectSceneSwitcher : MonoBehaviour, ITappable
{
    /// <summary>
    /// The name of the scene that is loaded
    /// </summary>
    [SerializeField] private string targetSceneName = "Above Ground";
    /// <summary>
    /// Reference to the ElevatorController to play animations before scene transition
    /// </summary>
    [SerializeField] private ElevatorController elevatorController;
    /// <summary>
    /// The name of the main scene that may not be unloaded.
    /// </summary>
    public string mainSceneName = "ElevatorScene";

    /// <summary>
    /// Initializes components and adds a BoxCollider if none exists
    /// </summary>
    void Awake()
    {
        if (elevatorController == null)
        {
            elevatorController = GetComponentInParent<ElevatorController>();
        }

        if (!TryGetComponent<Collider>(out _))
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
        }
    }

    /// <summary>
    /// Called when the object is tapped. Ensures doors are closed before scene switching
    /// </summary>
    public void OnTapped()
    {
        var currentPoint = MovablePoint.GetCurrentPoint();
        if (currentPoint != null && currentPoint.pointID == "2")
        {
            if (ButtonEmissionManager.Instance != null)
            {
                ButtonEmissionManager.Instance.SetButtonActive(gameObject, true);
            }
            
            MovablePoint.RegisterLastClickedSwitcher(this);
            EnsureDoorsClosedAndLoadScene();
            return;
        }
        
        MovablePoint.RegisterLastClickedSwitcher(this);
        StartElevatorSequence();
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
    /// Ensures doors are closed before loading the target scene, or keeps doors open if already in target scene
    /// </summary>
    public void EnsureDoorsClosedAndLoadScene()
    {
        if (IsTargetSceneAlreadyLoaded())
        {
            if (elevatorController != null)
            {
                elevatorController.OpenDoorsOnArrival();
            }
            return;
        }

        if (elevatorController != null)
        {
            if (elevatorController.currentState == ElevatorController.ElevatorState.Closed)
            {
                LoadTargetScene();
            }
            else
            {
                elevatorController.CloseDoors();
                StartCoroutine(WaitForDoorsToCloseAndLoadScene());
            }
        }
        else
        {
            LoadTargetScene();
        }
    }

    /// <summary>
    /// Checks if at MovementPoint2, manages door state, and loads the target scene
    /// </summary>
    public void CheckPositionCloseDoorsAndLoadScene()
    {
        var currentPoint = MovablePoint.GetCurrentPoint();
        if (currentPoint == null || currentPoint.pointID != "2")
        {
            return;
        }
        
        EnsureDoorsClosedAndLoadScene();
    }
    
    
    /// <summary>
    /// Loads the target scene additively, unloads other scenes except main scene, and schedules door opening
    /// </summary>
    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            bool targetSceneAlreadyLoaded = false;
            
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == targetSceneName)
                {
                    targetSceneAlreadyLoaded = true;
                }
                else if (scene.name != mainSceneName)
                {
                    SceneManager.UnloadSceneAsync(scene);
                }
            }

            if (!targetSceneAlreadyLoaded)
            {
                SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Additive);
                Invoke(nameof(OpenElevatorDoorsDelayed), 2f);
            }
        }
    }
    
    /// <summary>
    /// Coroutine that waits for elevator doors to finish closing animation before loading the target scene
    /// </summary>
    private System.Collections.IEnumerator WaitForDoorsToCloseAndLoadScene()
    {
        yield return new WaitForSeconds(0.1f);
        
        while (elevatorController != null && 
               (elevatorController.currentState == ElevatorController.ElevatorState.Animating ||
                elevatorController.currentState == ElevatorController.ElevatorState.Open))
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        LoadTargetScene();
    }
    
    
    /// <summary>
    /// Opens elevator doors after a delay when arriving at the target scene
    /// </summary>
    private void OpenElevatorDoorsDelayed()
    {
        if (elevatorController != null)
        {
            elevatorController.OpenDoorsOnArrival();
        }
    }
    
    /// <summary>
    /// Checks if the target scene is already loaded
    /// </summary>
    private bool IsTargetSceneAlreadyLoaded()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            return false;
        }

        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.name == targetSceneName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the target scene name for this ARObjectSceneSwitcher
    /// </summary>
    public string GetTargetSceneName()
    {
        return targetSceneName;
    }
    
}