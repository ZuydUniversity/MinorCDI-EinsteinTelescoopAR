using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// Class is used for elevator buttons to load and unload scenes with elevator integration.
/// </summary>
public class LiftButton : MonoBehaviour, ITappable
{
    /// <summary>
    /// The name of the scene that is loaded.
    /// </summary>
    public string targetSceneName = "Above Ground";
    /// <summary>
    /// The name of the main scene that may not be unloaded.
    /// </summary>
    public string mainSceneName = "ElevatorScene";
    /// <summary>
    /// Reference to the ElevatorController for door operations
    /// </summary>
    [SerializeField] private ElevatorController elevatorController;
    void Start()
    {
        if (elevatorController == null)
            elevatorController = FindObjectOfType<ElevatorController>();
    }

    /// <summary>
    /// Gets executed when tapped on object.
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
            
            LoadSceneWithElevatorSequence();
            return;
        }
    }

    /// <summary>
    /// Loads the target scene and handles elevator door sequence
    /// </summary>
    private void LoadSceneWithElevatorSequence()
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
            
            Invoke(nameof(OpenElevatorDoorsDelayed), 2f);
        }
    }
    
    /// <summary>
    /// Opens elevator doors after scene load delay
    /// </summary>
    private void OpenElevatorDoorsDelayed()
    {
        if (elevatorController != null)
        {
            elevatorController.OpenDoorsOnArrival();
        }
    }
}
