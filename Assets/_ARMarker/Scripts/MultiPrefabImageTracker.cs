using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

// Klasse die een image koppelt aan een prefab met instellingen
[System.Serializable]
public class ImagePrefabPair
{
    [SerializeField] public string imageName; // Naam van de image dat herkend moet worden
    [SerializeField] public GameObject prefab; // Het 3D object dat gespawnd wordt
    [SerializeField] public Vector3 positionOffset = Vector3.zero; // Waar het object verschijnt t.o.v. de marker
    [SerializeField] public Vector3 rotationOffset = Vector3.zero; // Hoe het object gedraaid wordt
    [SerializeField] public Vector3 scale = Vector3.one; // Hoe groot het object is
    
    [Header("Animation Settings")]
    [SerializeField] public bool enableBobbing = false; // Of het object op en neer beweegt
    [SerializeField] public float bobbingSpeed = 2f; // Hoe snel het beweegt
    [SerializeField] public Vector3 bobbingAmplitude = new Vector3(0, 0.1f, 0); // Hoe ver het beweegt
    [SerializeField] public bool enableRotation = false; // Of het object draait
    [SerializeField] public Vector3 rotationSpeed = new Vector3(0, 50, 0); // Hoe snel het draait
}

// Klasse die animatie data bijhoudt voor elk gespawnd object
[System.Serializable]
public class AnimatedPrefabData
{
    public GameObject gameObject; // Het gespawnde object
    public ImagePrefabPair config; // De instellingen voor dit object
    public Vector3 originalPosition; // Originele positie voor animaties
    public Quaternion originalRotation; // Originele rotatie voor animaties
    public Vector3 bobbingOffsets; // Random offsets voor natuurlijke beweging
    
    public AnimatedPrefabData(GameObject obj, ImagePrefabPair cfg)
    {
        gameObject = obj;
        config = cfg;
        originalPosition = obj.transform.localPosition;
        originalRotation = obj.transform.localRotation;
        bobbingOffsets = new Vector3(
            Random.Range(0f, Mathf.PI * 2f),
            Random.Range(0f, Mathf.PI * 2f),
            Random.Range(0f, Mathf.PI * 2f)
        );
    }
}

// Hoofdscript dat meerdere prefabs kan spawnen per herkende image
public class MultiPrefabImageTracker : MonoBehaviour
{
    [SerializeField] private List<ImagePrefabPair> imagePrefabPairs = new List<ImagePrefabPair>(); // Lijst van alle image prefab koppelingen
    
    private ARTrackedImageManager trackedImageManager; // Unity's AR component voor image herkenning
    private Dictionary<string, List<ImagePrefabPair>> imageNameToPrefabPairs = new Dictionary<string, List<ImagePrefabPair>>(); // Snelle lookup van prefabs per image
    private Dictionary<ARTrackedImage, List<GameObject>> spawnedPrefabs = new Dictionary<ARTrackedImage, List<GameObject>>(); // Bijhouden welke objecten bij welke image horen
    private List<AnimatedPrefabData> animatedPrefabs = new List<AnimatedPrefabData>(); // Lijst van objecten die geanimeerd worden

    // Setup functie die draait voordat de scene start
    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>(); // Vind de AR component
        
        Debug.Log($"MultiPrefabImageTracker: Found {imagePrefabPairs.Count} image-prefab pairs in configuration");
        
        // Maak een snelle lookup tabel van alle image prefab koppelingen
        foreach (var pair in imagePrefabPairs)
        {
            if (!string.IsNullOrEmpty(pair.imageName) && pair.prefab != null)
            {
                if (!imageNameToPrefabPairs.ContainsKey(pair.imageName))
                {
                    imageNameToPrefabPairs[pair.imageName] = new List<ImagePrefabPair>();
                }
                imageNameToPrefabPairs[pair.imageName].Add(pair);
                Debug.Log($"Registered prefab for image: {pair.imageName} -> {pair.prefab.name}");
            }
            else
            {
                Debug.LogWarning($"Skipping invalid pair: imageName='{pair.imageName}', prefab={pair.prefab}");
            }
        }
        
        int totalMappings = imageNameToPrefabPairs.Values.Sum(list => list.Count);
        Debug.Log($"MultiPrefabImageTracker: Successfully registered {totalMappings} prefab mappings across {imageNameToPrefabPairs.Count} unique images");
    }

    // Luister naar image veranderingen wanneer script actief wordt
    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged += OnImageChanged;
        }
    }

    // Stop met luisteren naar image veranderingen wanneer script uitgeschakeld wordt
    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackedImagesChanged -= OnImageChanged;
        }
    }

    // Wordt aangeroepen wanneer er iets verandert met herkende images
    void OnImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Nieuwe images gevonden - spawn prefabs
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        // Bestaande images geupdate - toon/verberg objecten
        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }

        // Images niet meer zichtbaar - verwijder objecten
        foreach (ARTrackedImage trackedImage in eventArgs.removed)
        {
            RemoveTrackedImage(trackedImage);
        }
    }

    // Spawn prefabs voor een nieuw herkende image
    void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage == null)
        {
            Debug.LogWarning("TrackedImage has null referenceImage");
            return;
        }
        
        string imageName = trackedImage.referenceImage.name; // Haal de naam van de image op
        
        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogWarning("TrackedImage referenceImage has null or empty name");
            return;
        }
        
        Debug.Log($"Attempting to handle tracked image: {imageName}");
        
        // Kijk of we prefabs hebben voor deze image
        if (imageNameToPrefabPairs.TryGetValue(imageName, out List<ImagePrefabPair> pairs))
        {
            List<GameObject> spawnedObjects = new List<GameObject>();
            
            // Spawn alle prefabs die bij deze image horen
            foreach (var pair in pairs)
            {
                GameObject spawnedObject = Instantiate(pair.prefab); // Maak een kopie van de prefab
                spawnedObject.transform.SetParent(trackedImage.transform, false); // Koppel aan de image
                
                // Pas positie, rotatie en grootte aan
                spawnedObject.transform.localPosition = pair.positionOffset;
                spawnedObject.transform.localRotation = Quaternion.Euler(pair.rotationOffset);
                spawnedObject.transform.localScale = pair.scale;
                
                spawnedObjects.Add(spawnedObject);
                
                // Voeg toe aan animatie lijst als er animaties zijn
                if (pair.enableBobbing || pair.enableRotation)
                {
                    AnimatedPrefabData animData = new AnimatedPrefabData(spawnedObject, pair);
                    animatedPrefabs.Add(animData);
                }
            }
            
            spawnedPrefabs[trackedImage] = spawnedObjects;
            
            Debug.Log($"Spawned {pairs.Count} prefab(s) for image: {imageName}");
        }
        else
        {
            Debug.LogWarning($"No prefabs found for image: {imageName}. Available images: {string.Join(", ", imageNameToPrefabPairs.Keys)}");
        }
    }

    // Toon of verberg objecten afhankelijk van of de image nog zichtbaar is
    void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (spawnedPrefabs.TryGetValue(trackedImage, out List<GameObject> spawnedObjects))
        {
            bool isTracking = trackedImage.trackingState == TrackingState.Tracking; // Is de image nog zichtbaar?
            foreach (var spawnedObject in spawnedObjects)
            {
                if (spawnedObject != null)
                {
                    spawnedObject.SetActive(isTracking); // Toon/verberg het object
                }
            }
        }
    }

    // Verwijder alle objecten wanneer een image niet meer herkend wordt
    void RemoveTrackedImage(ARTrackedImage trackedImage)
    {
        if (spawnedPrefabs.TryGetValue(trackedImage, out List<GameObject> spawnedObjects))
        {
            foreach (var spawnedObject in spawnedObjects)
            {
                if (spawnedObject != null)
                {
                    animatedPrefabs.RemoveAll(data => data.gameObject == spawnedObject); // Verwijder uit animatie lijst
                    Destroy(spawnedObject); // Verwijder het object uit de scene
                }
            }
            spawnedPrefabs.Remove(trackedImage); // Vergeet deze image
        }
    }

    // Update functie die elke frame draait voor animaties
    void Update()
    {
        // Loop achterwaarts door de lijst zodat we items kunnen verwijderen
        for (int i = animatedPrefabs.Count - 1; i >= 0; i--)
        {
            var animData = animatedPrefabs[i];
            
            // Verwijder objecten die niet meer bestaan
            if (animData.gameObject == null)
            {
                animatedPrefabs.RemoveAt(i);
                continue;
            }
            
            // Skip objecten die uitgeschakeld zijn
            if (!animData.gameObject.activeInHierarchy)
                continue;
                
            var config = animData.config;
            var transform = animData.gameObject.transform;
            
            // Bobbing animatie
            if (config.enableBobbing)
            {
                // Bereken wave beweging voor elke as
                Vector3 bobOffset = new Vector3(
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.x) * config.bobbingAmplitude.x,
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.y) * config.bobbingAmplitude.y,
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.z) * config.bobbingAmplitude.z
                );
                Vector3 newPosition = animData.originalPosition + bobOffset;
                transform.localPosition = newPosition;
            }
            
            // Rotatie animatie
            if (config.enableRotation)
            {
                Vector3 rotationDelta = config.rotationSpeed * Time.deltaTime; // Bereken hoeveel er gedraaid wordt dit frame
                transform.Rotate(rotationDelta, Space.Self); // Draai het object
            }
        }
    }
}