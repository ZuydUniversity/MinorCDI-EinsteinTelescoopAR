using UnityEngine;

public class ProximityTrigger : MonoBehaviour
{
    public Transform player;

    void Update()
    {
        if (!player) player = Camera.main.transform;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance < 2f)
        {
            Debug.Log("Player binnen triggerafstand!");
        }
    }
}
