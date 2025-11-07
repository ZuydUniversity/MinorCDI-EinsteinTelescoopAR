using UnityEngine;

public class ProximityTrigger : MonoBehaviour
{
    public Transform player;
    public GameObject modalPanel;
    private bool hasTriggered = false;

    /// <summary>
    /// Checks if the player is within a certain distance. If that is the case show the selected modal
    /// </summary>
    /// <param name="player">Player transform. Puts the main camera in player.</param>
    /// <param name="modalPanel">The activated modal once the player reaches the distance.</param>

    void Update()
    {
        if (!player)
        {
            player = Camera.main.transform;
        } 

        if (hasTriggered)
        {
            return;
        } 

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 2f)
        {
            hasTriggered = true;
            if (modalPanel != null)
            {
                modalPanel.SetActive(true);
            }
            else
            {
                Debug.LogWarning("Geen modalPanel gekoppeld aan " + name);
            }
        }
    }
}
