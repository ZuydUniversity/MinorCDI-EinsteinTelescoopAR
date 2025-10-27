using UnityEngine;
using UnityEngine.SceneManagement;

public class LiftSceneLoader : MonoBehaviour
{
    /// <summary>
    /// The name of the default start scene that should be loaded.
    /// </summary>
    [SerializeField] private string defaultStartScene = "TutorialScene";

    /// <summary>
    /// When the object this script is attached to is enabled
    /// it will add eventhandlers to the scene loaded event 
    /// in the scene manager.
    /// </summary>
    private void OnEnable() 
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadSceneAsync(defaultStartScene, LoadSceneMode.Additive);
    }

    /// <summary>
    /// When the object this script is attached to is disabled
    /// it will remove eventhandlers from the scene loaded event 
    /// in the scene manager.
    /// </summary>
    private void OnDisable() 
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>
    /// Executes when scene is being loaded.
    /// </summary>
    /// <param name="scene">The scene that is being loaded</param>
    /// <param name="mode">The mode that the scene is being loaded with</param>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
    {
        GameObject origin = gameObject;
        GameObject[] sceneObjects = scene.GetRootGameObjects();

        Vector3 offset = origin.transform.position;
        Quaternion rotationOffset = origin.transform.rotation;
        foreach(GameObject sceneObject in sceneObjects) 
        {
            sceneObject.transform.position = rotationOffset * sceneObject.transform.position + offset;
            sceneObject.transform.rotation = rotationOffset * sceneObject.transform.rotation;
        }
    }
}