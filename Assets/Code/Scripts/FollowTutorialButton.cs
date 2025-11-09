using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Used to determine if to load the tutorial scene.
/// </summary>
public class FollowTutorialButton : MonoBehaviour
{
    /// <summary>
    /// The scene to load as the tutorial.
    /// </summary>
    public string tutorialScene = "TutorialScene";

    /// <summary>
    /// Loads the tutorial scene and disables the Lift object in the MainScene.
    /// </summary>
    public void LoadTutorial() 
    {
        SceneManager.LoadSceneAsync(tutorialScene, LoadSceneMode.Additive);

        GameObject lift = GameObject.Find("Lift(Scripted+Textured)(Clone)");
        if (lift != null)
        {
            lift.SetActive(false);
        }

        #if UNITY_EDITOR
        if (lift != null)
        {
            Debug.Log("Lift(Scripted+Textured)(Clone) is now inactive.");
        }
        else
        {
            Debug.LogWarning("Lift(Scripted+Textured)(Clone) not found in MainScene.");
        }
        #endif
    }
}