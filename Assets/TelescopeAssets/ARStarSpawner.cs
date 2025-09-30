using UnityEngine;

public class ARStarSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public Vector3 spawnPosition;
    private Vector3 spawnPos;
    public int amount = 40;
    public float minDistance = 0.3f;
    public float maxDistance = 1.0f;

    public void SpawnObject()
    {
        if (prefabToSpawn == null) return;
        int count = amount;
        while (count > 0)
        {
            // Spawn 1 meter in front of the AR camera
            Camera cam = Camera.main;
            if (spawnPosition == null)
            {
                spawnPosition = cam.transform.position + cam.transform.forward * 1.0f;
            }
            count -= 1;
            spawnPos = spawnPosition + Random.onUnitSphere * Random.Range(minDistance, maxDistance);
            if (spawnPos.y < 0){ spawnPos.y = -spawnPos.y; }
            GameObject star = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            float scale = Random.Range(.2f, 1.2f);
            star.transform.localScale *= scale;
        }
    }
}
