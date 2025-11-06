using UnityEngine;

public class ProximityTrigger : MonoBehaviour
{
    public Transform player;
    public GameObject modalPanel;
    private bool hasTriggered = false;

    void Update()
    {
        if (!player) player = Camera.main.transform;
        if (hasTriggered) return;

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
