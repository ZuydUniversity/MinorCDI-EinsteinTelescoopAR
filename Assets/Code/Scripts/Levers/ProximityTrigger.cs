using UnityEngine;
using UnityEngine.SceneManagement;

public class ProximityTrigger : MonoBehaviour
{
    public Transform player;
    public GameObject modalPanel;
    private bool hasTriggered = false;
    private bool liftReactivated = false;

    void Update()
    {
        if (!player)
        {
            player = Camera.main.transform;
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (!hasTriggered && distance < 2f)
        {
            hasTriggered = true;
            if (modalPanel != null)
            {
                modalPanel.SetActive(true);
                Debug.Log("Modal shown: " + modalPanel.name);

                // Zodra de Elevator Modal actief is, lift weer activeren
                if (!liftReactivated && modalPanel.name.Contains("Elevator"))
                {
                    GameObject lift = FindLiftInAllScenes("Lift(Scripted+Textured)(Clone)");
                    if (lift != null)
                    {
                        lift.SetActive(true);
                        liftReactivated = true;
                        Debug.Log("Lift(Scripted+Textured)(Clone) is reactivated.");
                    }
                    else
                    {
                        Debug.LogWarning("Lift(Scripted+Textured)(Clone) not found to reactivate.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("Geen modalPanel gekoppeld aan " + name);
            }
        }
    }

    private GameObject FindLiftInAllScenes(string objectName)
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);
            if (!scene.isLoaded) continue;

            foreach (GameObject go in scene.GetRootGameObjects())
            {
                if (go.name == objectName)
                    return go;
            }
        }
        return null;
    }
}
