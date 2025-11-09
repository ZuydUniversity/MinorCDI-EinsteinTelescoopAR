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

    //button sound clip
    public AudioClip buttonClip;

    /// <summary>
    /// The name of the main scene that may not be unloaded.
    /// </summary>
    public string mainSceneName = "MainScene";

    /// <summary>
    /// Checks if scene is already being loaded.
    /// </summary>
    private bool loadingScene = false;

    public ButtonEmissionManager buttonEmissionManager;

    /// <summary>
    /// Initializes components and adds a BoxCollider if none exists
    /// </summary>
    void Start()
    {
        elevatorController = GetComponentInParent<ElevatorController>();
    }

    /// <summary>
    /// Starts the elevator animation sequence and then loads the target scene
    /// </summary>
    public void OnTapped()
    {       
        if (!loadingScene)
        {
            buttonEmissionManager.ResetButtons();
            buttonEmissionManager.SetButtonEmission(gameObject, true);
            if (buttonClip != null)
            {
                AudioSource.PlayClipAtPoint(buttonClip, transform.position);
            }
            else
            {
                Debug.LogWarning("Button clip is not assigned in ARObjectSceneSwitcher.");
            }
        }

            loadingScene = true;
            buttonEmissionManager.ResetButtons();
            buttonEmissionManager.SetButtonEmission(gameObject, true);

            EnsureDoorsClosedAndLoadScene();
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
        }
        else
        {
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

        loadingScene = false;
    }
    
    /// <summary>
    /// Loads the target scene additively, unloads other scenes except main scene, and schedules door opening
    /// </summary>
    private void LoadTargetScene()
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
    private IEnumerator WaitForDoorsToCloseAndLoadScene()
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
}