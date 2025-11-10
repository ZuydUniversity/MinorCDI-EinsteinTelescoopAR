using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.SceneManagement;

public class ReturnToElevator : MonoBehaviour
{
    /// <summary>
    /// The movable point to move the user to.
    /// </summary>
    public MovablePoint point;

    /// <summary>
    /// The name of the tutorial scene to unload.
    /// </summary>
    public string tutorialScene = "TutorialScene";
    
    /// <summary>
    /// The xrOrigin in the scene to move.
    /// </summary>
    private XROrigin xrOrigin;

    /// <summary>
    /// Gets current XROrigin.
    /// </summary>
    void Start() 
    {
        xrOrigin = FindFirstObjectByType<XROrigin>();
    }

    /// <summary>
    /// Resets camera to elevator and unloads tutorial.
    /// </summary>
    public void ResetToElevator()
    {
        Vector3 offset = point.transform.position - Camera.main.transform.position;
        offset.y = 0;

        xrOrigin.transform.position += offset;

        SceneManager.UnloadSceneAsync(tutorialScene);
    }
}
