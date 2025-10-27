using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Class is used for lift buttons to load and unload scenes.
/// </summary>
public class LiftButton : MonoBehaviour, ITappable
{
    /// <summary>
    /// The name of the scene that is loaded.
    /// </summary>
    public string targetSceneName = "BovengrondScene";
    /// <summary>
    /// The name of the main scene that may not be unloaded.
    /// </summary>
    public string mainSceneName = "ElevatorScene";

    /// <summary>
    /// Gets executed when tapped on object.
    /// </summary>
    public void OnTapped() 
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
        }
    }
}
