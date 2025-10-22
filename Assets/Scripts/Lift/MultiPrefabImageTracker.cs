using System.Collections;
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
    [SerializeField] private float hideDelay = 10f; // Tijd in seconden voordat prefabs verdwijnen na verlies van tracking

    private ARTrackedImageManager trackedImageManager; // Unity's AR component voor image herkenning
    private Dictionary<string, List<ImagePrefabPair>> imageNameToPrefabPairs = new Dictionary<string, List<ImagePrefabPair>>(); // Snelle lookup van prefabs per image
    private Dictionary<ARTrackedImage, List<GameObject>> spawnedPrefabs = new Dictionary<ARTrackedImage, List<GameObject>>(); // Bijhouden welke objecten bij welke image horen
    private Dictionary<ARTrackedImage, Coroutine> hideCoroutines = new Dictionary<ARTrackedImage, Coroutine>(); // Bijhouden van hide timers
    private Dictionary<string, ARTrackedImage> activeTrackedImages = new Dictionary<string, ARTrackedImage>(); // Bijhouden van actieve images per naam
    private Dictionary<string, List<GameObject>> fixedPrefabs = new Dictionary<string, List<GameObject>>(); // Prefabs met vaste posities per image naam
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
            trackedImageManager.trackablesChanged.AddListener(OnImageChanged);
        }
    }

    // Stop met luisteren naar image veranderingen wanneer script uitgeschakeld wordt
    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
        }
    }

    // Wordt aangeroepen wanneer er iets verandert met herkende images
    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Debug logging om te zien wat er gebeurt
        Debug.Log($"OnImageChanged called - Added: {eventArgs.added.Count}, Updated: {eventArgs.updated.Count}, Removed: {eventArgs.removed.Count}");

        // Nieuwe images gevonden - spawn prefabs
        foreach (var trackedImage in eventArgs.added)
        {
            string imageName = trackedImage.referenceImage != null ? trackedImage.referenceImage.name : "Unknown";
            Debug.Log($"New tracked image added: {imageName}");
            HandleTrackedImage(trackedImage);
        }

        // Bestaande images geupdate - toon/verberg objecten
        foreach (var trackedImage in eventArgs.updated)
        {
            string imageName = trackedImage.referenceImage != null ? trackedImage.referenceImage.name : "Unknown";
            Debug.Log($"Tracked image updated: {imageName}, State: {trackedImage.trackingState}");
            UpdateTrackedImage(trackedImage);
        }

        // Images niet meer zichtbaar - verwijder objecten
        foreach (var kvp in eventArgs.removed)
        {
            string imageName = kvp.Value.referenceImage != null ? kvp.Value.referenceImage.name : "Unknown";
            Debug.Log($"Tracked image removed: {imageName}");
            RemoveTrackedImage(kvp.Value);
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

        string imageName = trackedImage.referenceImage.name;

        if (string.IsNullOrEmpty(imageName))
        {
            Debug.LogWarning("TrackedImage referenceImage has null or empty name");
            return;
        }

        // Controleer of er al prefabs bestaan voor deze image naam
        if (fixedPrefabs.ContainsKey(imageName))
        {
            Debug.Log($"Fixed prefabs already exist for image: {imageName}, skipping spawn. Fixed prefabs count: {fixedPrefabs[imageName].Count}");

            // Update alleen de tracking referenties, maar laat prefabs op hun vaste positie
            activeTrackedImages[imageName] = trackedImage;

            // Zorg dat de prefabs zichtbaar zijn
            foreach (var prefab in fixedPrefabs[imageName])
            {
                if (prefab != null)
                {
                    prefab.SetActive(true);
                }
            }

            // Stop eventuele hide timer
            if (hideCoroutines.ContainsKey(trackedImage))
            {
                StopCoroutine(hideCoroutines[trackedImage]);
                hideCoroutines.Remove(trackedImage);
            }

            return;
        }

        Debug.Log($"Attempting to handle tracked image: {imageName}");

        if (imageNameToPrefabPairs.TryGetValue(imageName, out List<ImagePrefabPair> pairs))
        {
            // Registreer deze image als actief
            activeTrackedImages[imageName] = trackedImage;
            StartCoroutine(SafeInstantiatePrefabs(trackedImage, pairs, imageName));
        }
        else
        {
            Debug.LogWarning($"No prefabs found for image: {imageName}. Available images: {string.Join(", ", imageNameToPrefabPairs.Keys)}");
        }
    }

    // Veilige instantiatie van prefabs op de main thread
    private System.Collections.IEnumerator SafeInstantiatePrefabs(ARTrackedImage trackedImage, List<ImagePrefabPair> pairs, string imageName)
    {
        yield return null; // Wacht een frame om zeker te zijn dat we op de main thread zijn

        List<GameObject> spawnedObjects = new List<GameObject>();
        bool hasError = false;

        foreach (var pair in pairs)
        {
            if (pair.prefab == null)
            {
                Debug.LogWarning($"Prefab is null for image: {imageName}");
                continue;
            }

            GameObject spawnedObject = null;

            try
            {
                spawnedObject = Instantiate(pair.prefab);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error instantiating prefab for image {imageName}: {ex.Message}");
                hasError = true;
                continue;
            }

            if (spawnedObject == null)
            {
                Debug.LogError($"Failed to instantiate prefab for image: {imageName}");
                continue;
            }

            try
            {
                // Zet de prefab op de juiste positie relatief tot de tracked image
                spawnedObject.transform.SetParent(trackedImage.transform, false);
                spawnedObject.transform.localPosition = pair.positionOffset;
                spawnedObject.transform.localRotation = Quaternion.Euler(pair.rotationOffset);
                spawnedObject.transform.localScale = pair.scale;

                // Ontkoppel van de tracked image en zet in wereldco�rdinaten voor vaste positie
                Vector3 worldPosition = spawnedObject.transform.position;
                Quaternion worldRotation = spawnedObject.transform.rotation;
                Vector3 worldScale = spawnedObject.transform.lossyScale;

                spawnedObject.transform.SetParent(null); // Ontkoppel van tracked image
                spawnedObject.transform.position = worldPosition;
                spawnedObject.transform.rotation = worldRotation;
                spawnedObject.transform.localScale = worldScale;

                // Deactiveer Canvas componenten tijdelijk om DPI errors te voorkomen
                Canvas[] canvases = spawnedObject.GetComponentsInChildren<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (canvas != null)
                    {
                        canvas.enabled = false;
                        StartCoroutine(ReenableCanvasAfterFrame(canvas));
                    }
                }

                spawnedObjects.Add(spawnedObject);

                if (pair.enableBobbing || pair.enableRotation)
                {
                    AnimatedPrefabData animData = new AnimatedPrefabData(spawnedObject, pair);
                    animatedPrefabs.Add(animData);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error configuring spawned object for image {imageName}: {ex.Message}");
                if (spawnedObject != null)
                {
                    Destroy(spawnedObject);
                }
                hasError = true;
            }

            yield return null; // Wacht een frame tussen elke instantiatie
        }

        if (hasError)
        {
            // Cleanup bij errors
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            Debug.LogError($"Failed to safely instantiate all prefabs for image: {imageName}");
        }
        else
        {
            spawnedPrefabs[trackedImage] = spawnedObjects;

            // Voeg ook toe aan fixed prefabs voor vaste posities
            if (!fixedPrefabs.ContainsKey(imageName))
            {
                fixedPrefabs[imageName] = new List<GameObject>();
            }
            fixedPrefabs[imageName].AddRange(spawnedObjects);

            Debug.Log($"Safely spawned {pairs.Count} prefab(s) for image: {imageName} at fixed world positions");
        }
    }

    // Heractiveer Canvas na een frame om threading issues te voorkomen
    private System.Collections.IEnumerator ReenableCanvasAfterFrame(Canvas canvas)
    {
        yield return new WaitForEndOfFrame();
        if (canvas != null)
        {
            canvas.enabled = true;
        }
    }

    // Toon of verberg objecten afhankelijk van of de image nog zichtbaar is
    void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage != null ? trackedImage.referenceImage.name : "Unknown";

        // Gebruik fixed prefabs in plaats van spawned prefabs voor consistentie
        if (fixedPrefabs.ContainsKey(imageName))
        {
            bool isTracking = trackedImage.trackingState == TrackingState.Tracking;
            Debug.Log($"UpdateTrackedImage for {imageName}: isTracking = {isTracking}, fixed prefabs count = {fixedPrefabs[imageName].Count}");

            if (isTracking)
            {
                // Image wordt getrackt - toon objecten en stop eventuele hide timer
                foreach (var prefab in fixedPrefabs[imageName])
                {
                    if (prefab != null)
                    {
                        prefab.SetActive(true);
                    }
                }

                // Stop de hide coroutine als die loopt
                if (hideCoroutines.TryGetValue(trackedImage, out Coroutine hideCoroutine))
                {
                    if (hideCoroutine != null)
                    {
                        StopCoroutine(hideCoroutine);
                    }
                    hideCoroutines.Remove(trackedImage);
                    Debug.Log($"Stopped hide timer for {imageName}");
                }
            }
            else
            {
                // Image wordt niet meer getrackt - start hide timer als die nog niet loopt
                if (!hideCoroutines.ContainsKey(trackedImage))
                {
                    Debug.Log($"Starting hide timer for {imageName}");
                    Coroutine hideCoroutine = StartCoroutine(HideFixedObjectsAfterDelay(imageName));
                    hideCoroutines[trackedImage] = hideCoroutine;
                }
            }
        }
        else
        {
            Debug.Log($"UpdateTrackedImage for {imageName}: No fixed prefabs found for this image");

            // Controleer of er prefabs zijn voor deze image naam maar met een ander ARTrackedImage object
            if (activeTrackedImages.ContainsKey(imageName))
            {
                Debug.Log($"Found active image {imageName} with different ARTrackedImage object, treating as new detection");
                HandleTrackedImage(trackedImage);
            }
            else
            {
                Debug.Log($"No active image found for {imageName}, treating as completely new detection");
                HandleTrackedImage(trackedImage);
            }
        }
    }

    // Coroutine om objecten te verbergen en verwijderen na een vertraging
    private IEnumerator HideObjectsAfterDelay(ARTrackedImage trackedImage, List<GameObject> spawnedObjects)
    {
        yield return new WaitForSeconds(hideDelay);

        // Verwijder de objecten definitief na de vertraging
        foreach (var spawnedObject in spawnedObjects)
        {
            if (spawnedObject != null)
            {
                // Deactiveer Canvas componenten voor veilige verwijdering
                Canvas[] canvases = spawnedObject.GetComponentsInChildren<Canvas>();
                foreach (var canvas in canvases)
                {
                    if (canvas != null)
                    {
                        canvas.enabled = false;
                    }
                }

                // Verwijder uit animatie lijst
                animatedPrefabs.RemoveAll(data => data.gameObject == spawnedObject);

                // Vernietig het object
                Destroy(spawnedObject);
            }
        }

        // Verwijder de tracked image uit de spawned prefabs dictionary
        spawnedPrefabs.Remove(trackedImage);

        // Verwijder de coroutine uit de dictionary
        hideCoroutines.Remove(trackedImage);

        // Verwijder uit actieve images zodat nieuwe spawns mogelijk zijn
        string imageName = trackedImage.referenceImage != null ? trackedImage.referenceImage.name : "Unknown";
        bool removed = activeTrackedImages.Remove(imageName);

        Debug.Log($"Objects completely removed after {hideDelay} seconds for tracked image: {imageName}. Removed from active images: {removed}. Active images count: {activeTrackedImages.Count}. Ready for new spawn.");
    }

    // Coroutine om fixed objects te verbergen en verwijderen na een vertraging
    private IEnumerator HideFixedObjectsAfterDelay(string imageName)
    {
        yield return new WaitForSeconds(hideDelay);

        if (fixedPrefabs.ContainsKey(imageName))
        {
            // Verwijder de fixed objects definitief na de vertraging
            foreach (var fixedObject in fixedPrefabs[imageName])
            {
                if (fixedObject != null)
                {
                    // Deactiveer Canvas componenten voor veilige verwijdering
                    Canvas[] canvases = fixedObject.GetComponentsInChildren<Canvas>();
                    foreach (var canvas in canvases)
                    {
                        if (canvas != null)
                        {
                            canvas.enabled = false;
                        }
                    }

                    // Verwijder uit animatie lijst
                    animatedPrefabs.RemoveAll(data => data.gameObject == fixedObject);

                    // Vernietig het object
                    Destroy(fixedObject);
                }
            }

            // Verwijder uit fixed prefabs dictionary
            fixedPrefabs.Remove(imageName);

            // Verwijder uit actieve images
            activeTrackedImages.Remove(imageName);

            // Cleanup hide coroutines
            var keysToRemove = new List<ARTrackedImage>();
            foreach (var kvp in hideCoroutines)
            {
                string trackingImageName = kvp.Key.referenceImage != null ? kvp.Key.referenceImage.name : "Unknown";
                if (trackingImageName == imageName)
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                hideCoroutines.Remove(key);
                if (spawnedPrefabs.ContainsKey(key))
                {
                    spawnedPrefabs.Remove(key);
                }
            }

            Debug.Log($"Fixed objects completely removed after {hideDelay} seconds for image: {imageName}. Ready for new spawn.");
        }
    }

    // Verwijder alle objecten wanneer een image niet meer herkend wordt
    void RemoveTrackedImage(ARTrackedImage trackedImage)
    {
        // Stop eventuele hide coroutine
        if (hideCoroutines.TryGetValue(trackedImage, out Coroutine hideCoroutine))
        {
            if (hideCoroutine != null)
            {
                StopCoroutine(hideCoroutine);
            }
            hideCoroutines.Remove(trackedImage);
        }

        if (spawnedPrefabs.TryGetValue(trackedImage, out List<GameObject> spawnedObjects))
        {
            try
            {
                foreach (var spawnedObject in spawnedObjects)
                {
                    if (spawnedObject != null)
                    {
                        // Deactiveer Canvas componenten voor veilige verwijdering
                        Canvas[] canvases = spawnedObject.GetComponentsInChildren<Canvas>();
                        foreach (var canvas in canvases)
                        {
                            if (canvas != null)
                            {
                                canvas.enabled = false;
                            }
                        }

                        animatedPrefabs.RemoveAll(data => data.gameObject == spawnedObject);
                        Destroy(spawnedObject);
                    }
                }
                spawnedPrefabs.Remove(trackedImage);

                // Verwijder ook uit actieve images en fixed prefabs
                string imageName = trackedImage.referenceImage != null ? trackedImage.referenceImage.name : "Unknown";
                activeTrackedImages.Remove(imageName);

                // Cleanup fixed prefabs als ze bestaan
                if (fixedPrefabs.ContainsKey(imageName))
                {
                    fixedPrefabs.Remove(imageName);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error removing tracked image objects: {ex.Message}");
            }
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