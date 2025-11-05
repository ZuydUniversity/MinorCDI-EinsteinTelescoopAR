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
    /// The offset the particles should have from the origin.
    /// </summary>
    public Vector3 particleOffset = new Vector3(0f, 5f, 0f);

    /// <summary>
    /// The max amount of black holes that can spawn.
    /// </summary>
    public int maxBlackholeCount = 2;
    /// <summary>
    /// The current amount of black holes spawned in.
    /// </summary>
    private int currentBlackHoleCount = 0;

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
    /// Indicator for where the stars will spawn.
    /// </summary>
    private HUDIndicator.IndicatorOffScreen indicator;

    /// <summary>
    /// Runs before the first update call after this script is created.
    /// Creates the wave particals at the position of the spawner and gets audiosource of particles.
    /// </summary>
    public void Start()
    {
        if (waveParticlesPrefab != null) 
        {
            waveParticles = Instantiate(waveParticlesPrefab, gameObject.transform.position, Quaternion.identity);
            waveParticles.transform.SetParent(gameObject.transform);
            waveParticles.transform.position += particleOffset;
            waveParticles.transform.rotation = waveParticlesPrefab.transform.rotation;
            waveParticalAudio = waveParticles.GetComponent<AudioSource>();
        }

        indicator = gameObject.GetComponent<HUDIndicator.IndicatorOffScreen>();
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

            indicator.visible = true;

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
        currentBlackHoleCount = 0;
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
            indicator.visible = false;    

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
            if (currentBlackHoleCount == maxBlackholeCount) 
            {
                /// randomized star spawning for show waves, switch calculates the position
                float spawnSelection = Random.Range(0, 2);
                switch (spawnSelection) 
                {
                    case 0:
                        prefabToSpawn = dwarfStar;
                        break;

                    case 1:
                        prefabToSpawn = star;
                        break;

                    default:
                        prefabToSpawn = null;
                        break;
                }
            }
            else 
            {
                prefabToSpawn = blackhole;
                currentBlackHoleCount += 1;
            }

            GameObject newObject = Instantiate(prefabToSpawn, randomizedSpawnPosition, Quaternion.identity);
            newObject.transform.localScale *= Random.Range(minScaleOffset, maxScaleOffset);
            newObject.transform.SetParent(origin.transform);
            spawnedObjects.Add(newObject);
        }
    }
}
