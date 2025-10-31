using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This script is use to create the animation for showing the detecting of gravitons and showing
/// the stars and blackholes that have been detected.
/// </summary>
public class StarSpawner : MonoBehaviour
{

    /// <summary>
    /// The star object to spawn.
    /// </summary>
    public GameObject star;
    /// <summary>
    /// The dwarf star object to spawn.
    /// </summary>
    public GameObject dwarfStar;
    /// <summary>
    /// The blackhole object to spawn.
    /// </summary>
    public GameObject blackhole;

    /// <summary>
    /// The prefab of the partical system for the gravity waves.
    /// </summary>
    public ParticleSystem waveParticlesPrefab;

    /// <summary>
    /// Amount of objects to spawn.
    /// </summary>
    public int count = 40;
    /// <summary>
    /// maximum offset of spawned object.
    /// </summary>
    public float maxSpawnOffset = 1.0f;
    /// <summary>
    /// minimum offset of spawned object.
    /// </summary>
    public float minSpawnOffset = 0.0f;
    /// <summary>
    /// Maximum change in scale of object that is spawned.
    /// </summary>
    public float maxScaleOffset = 1.0f;
    /// <summary>
    /// Minimum change in scale of object that is spawned.
    /// </summary>
    public float minScaleOffset = 0.0f;

    /// <summary>
    /// How long the animation should take before spawning
    /// in the stars and blackholes.
    /// </summary>
    public float duration = 10.0f;

    /// <summary>
    /// Used to check if animation is already playing.
    /// </summary>
    private bool playing = false;
    /// <summary>
    /// The objects that have been spawned.
    /// </summary>
    private List<GameObject> spawnedObjects = new List<GameObject>();
    /// <summary>
    /// The partical system that has been created from the prefab.
    /// </summary>
    private ParticleSystem waveParticles;

    /// <summary>
    /// Audio source of the particles
    /// </summary>
    private AudioSource waveParticalAudio;

    /// <summary>
    /// Runs before the first update call after this script is created.
    /// Creates the wave particals at the position of the spawner and gets audiosource of particles.
    /// </summary>
    public void Start()
    {
        if (waveParticlesPrefab != null) 
        {
            waveParticles = Instantiate(waveParticlesPrefab, gameObject.transform.position, Quaternion.identity);
            waveParticles.transform.rotation = waveParticlesPrefab.transform.rotation;
            waveParticalAudio = waveParticles.GetComponent<AudioSource>();
        }
    }

    /// <summary>
    /// Starts playing the animation and audio if it is not already
    /// playing and spawns the celestial bodies (stars and blackholes).
    /// </summary>
    public void StartSpawner() 
    {
        if (!playing)
        { 
            foreach (GameObject spawnedObject in spawnedObjects)
            {
                if (spawnedObject)
                {
                    Destroy(spawnedObject);
                }
            }

            spawnedObjects.Clear();
            waveParticles.Play();
            waveParticalAudio.Play();

            playing = true;
            Invoke(nameof(ShowCelestialBodies), duration); // Waits specified duration before stopping animation
        }
    }

    /// <summary>
    /// Spawns celestial bodies (stars and blackholes). 
    /// </summary>
    public void ShowCelestialBodies() 
    {
        for (int index = 0; index < count; index++)
        {
            SpawnObject();
        }
    }

    /// <summary>
    /// Stops playing the animation and audio if it is already playing
    /// and removes spawned objects.
    /// </summary>
    public void StopSpawner()
    {
        if (playing) 
        {
            waveParticles.Stop();
            waveParticalAudio.Stop();
            playing = false;
        }

        foreach (GameObject spawnedObject in spawnedObjects)
        {
            if (spawnedObject)
            {
                Destroy(spawnedObject);
            }
        }
    }

    /// <summary>
    /// Spawns object at a semi random location.
    /// </summary>
    private void SpawnObject() 
    {
        if (star != null || blackhole != null) 
        {
            GameObject origin = gameObject;
            
            Vector3 randomizedSpawnPosition = new Vector3(origin.transform.position.x, origin.transform.position.y, origin.transform.position.z);
            randomizedSpawnPosition.x += Random.Range(minSpawnOffset, maxSpawnOffset);
            randomizedSpawnPosition.y += Random.Range(minSpawnOffset, maxSpawnOffset);
            randomizedSpawnPosition.z += Random.Range(minSpawnOffset, maxSpawnOffset);

            GameObject prefabToSpawn;

            /// randomized star spawning for show waves, switch calculates the position
            float spawnSelection = Random.Range(0, 3);
            switch (spawnSelection) 
            {
                case 0:
                    prefabToSpawn = dwarfStar;
                    break;

                case 1:
                    prefabToSpawn = star;
                    break;

                case 2:
                    prefabToSpawn = blackhole;
                    break;

                default:
                    prefabToSpawn = null;
                    break;
            }

            GameObject newObject = Instantiate(prefabToSpawn, randomizedSpawnPosition, Quaternion.identity);
            newObject.transform.localScale *= Random.Range(minScaleOffset, maxScaleOffset);
            newObject.transform.SetParent(origin.transform);
            spawnedObjects.Add(newObject);
        }
    }
}
