using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script handles the visibility of the lift buttons
/// based on the current scene and optional movement point position.
/// </summary>
public class LiftButtonManager : MonoBehaviour
{    
    /// <summary>
    /// A rule for a part of a model. The sceneName declares the scene in which to make the part of
    /// the model invisible and the partsToHide array defines which parts to hide of the model.
    /// Optional movementPointID adds condition that player must be at specific point.
    /// </summary>
    [System.Serializable]
    public class ScenePartRule
    {
        public string sceneName;
        public GameObject[] partsToHide;
        public string requiredMovementPointID = "";
    }

    /// <summary>
    /// A list of rules for parts of the model.
    /// </summary>
    [SerializeField] private ScenePartRule[] rules;
    

    /// <summary>
    /// When the object this script is attached to is enabled
    /// it will add eventhandlers to the scene loaded and scene
    /// unloaded events in the scene manager.
    /// </summary>
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        MovablePoint.OnPlayerMoved += OnPlayerMoved;
    }

    /// <summary>
    /// When the object this script is attached to is disabled
    /// it will remove eventhandlers from the scene loaded and scene
    /// unloaded events in the scene manager.
    /// </summary>
    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        MovablePoint.OnPlayerMoved -= OnPlayerMoved;
    }

    /// <summary>
    /// Executes when scene is being loaded.
    /// </summary>
    /// <param name="scene">The scene that is being loaded</param>
    /// <param name="mode">The mode that the scene is being loaded with</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        UpdateVisibility(scene.name, true);
    }

    /// <summary>
    /// Executes when scene is being unloaded.
    /// </summary>
    /// <param name="scene">The scene that is being unloaded</param>
    private void OnSceneUnloaded(Scene scene) 
    {
        UpdateVisibility(scene.name, false);
    }
    
    /// <summary>
    /// Executes when player moves to a new MovementPoint
    /// </summary>
    private void OnPlayerMoved(MovablePoint newPoint)
    {
        ReevaluateAllActiveScenes();
    }
    
    /// <summary>
    /// Re-evaluate visibility rules for all currently loaded scenes
    /// </summary>
    private void ReevaluateAllActiveScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
            {
                UpdateVisibility(scene.name, true);
            }
        }
    }

    /// <summary>
    /// Updates visibility of model parts based on the rules.
    /// </summary>
    /// <param name="sceneName">The name of the scene</param>
    /// <param name="hide">Tells the function if it needs to hide the part</param>
    private void UpdateVisibility(string sceneName, bool hide) 
    {
        foreach (var rule in rules) 
        {
            if (rule.sceneName == sceneName) 
            {
                bool shouldHide = hide;
                
                if (hide && !string.IsNullOrEmpty(rule.requiredMovementPointID))
                {
                    shouldHide = CheckMovementPointCondition(rule.requiredMovementPointID);
                }
                
                foreach (var part in rule.partsToHide) 
                {
                    if (part != null)
                        part.SetActive(!shouldHide);
                }
            }
        }
    }
    
    /// <summary>
    /// Check if player is at the required MovementPoint
    /// </summary>
    /// <param name="requiredPointID">The MovementPoint ID to check</param>
    /// <returns>True if player is at the required point</returns>
    private bool CheckMovementPointCondition(string requiredPointID)
    {
        var currentPoint = MovablePoint.GetCurrentPoint();
        return currentPoint != null && currentPoint.pointID == requiredPointID;
    }
}