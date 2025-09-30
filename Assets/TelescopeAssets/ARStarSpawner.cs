using UnityEngine;

public class ARStarSpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public GameObject Telescope;
    public Vector3 spawnPosition;
    private Vector3 spawnPos;
    public int amount = 40;
    public float minDistance = 0.4f;
    public float maxDistance = 1.2f;

    public void SpawnObject()
    {
        if (prefabToSpawn == null) return;
        int count = amount;
        while (count > 0)
        {
            // Spawn 1 meter in front of the AR camera if no spawnPosition is given
            Camera cam = Camera.main;
            if (Telescope == null)
            {
                spawnPosition = cam.transform.position + cam.transform.forward * 1.0f;
            }
            else { spawnPosition = Telescope.transform.position + new Vector3(0, 1.0f, 0); }
            count -= 1;

            Vector3 dir = Random.onUnitSphere;
            dir = new Vector3(dir.x, dir.y * 0.5f, dir.z);
            dir.Normalize();
            spawnPos = spawnPosition + dir * Random.Range(minDistance, maxDistance);
            if (spawnPos.y < 0) { spawnPos.y = -spawnPos.y; }

            GameObject star = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            float scale = Random.Range(.2f, 1.2f);
            star.transform.localScale *= scale;
        }
    }
}
