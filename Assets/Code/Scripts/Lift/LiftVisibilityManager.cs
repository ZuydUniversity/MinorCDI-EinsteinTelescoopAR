using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// This script handles the visibility of parts of thelift
/// based on the current scene.
/// </summary>
public class LiftVisibilityManager : MonoBehaviour
{    
    /// <summary>
    /// A rule for a part of a model. The sceneName 
    /// delcares the scene in which to make the part of
    /// the model invisible and the partsToHide array
    /// defines which parts to hide of the model.
    /// </summary>
    [System.Serializable]
    public class ScenePartRule
    {
        public string sceneName;
        public GameObject[] partsToHide;
    }

    /// <summary>
    /// A list of rules for parts of the model.
    /// <summary>
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
                foreach (var part in rule.partsToHide) 
                {
                    part.SetActive(!hide);
                }
            }
        }
    }
}