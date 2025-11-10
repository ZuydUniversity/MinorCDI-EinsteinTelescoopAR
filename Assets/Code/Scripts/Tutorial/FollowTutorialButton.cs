using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
    /// Loads the tutorial on click.
    /// </summary>
    public void LoadTutorialOnClick() 
    {
        StartCoroutine(LoadTutorial());
    }

    /// <summary>
    /// Loads the tutorial scene and disables the Lift object in the MainScene.
    /// </summary>
    private IEnumerator LoadTutorial() 
    {
        AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(tutorialScene, LoadSceneMode.Additive);
        while (!loadSceneOperation.isDone)
        {
            yield return null;
        }

        GameObject lift = FindFirstObjectByType<LiftSceneLoader>()?.gameObject;
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

        // Call function that shows the indicator and moving point modals
    }
}