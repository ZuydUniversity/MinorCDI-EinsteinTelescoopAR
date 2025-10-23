using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[System.Serializable]
public class ImagePrefabPair
{
    /// <summary>
    /// The name of the AR image that should be tracked
    /// </summary>
    [SerializeField] public string imageName;
    /// <summary>
    /// The 3D prefab that will be spawned when the image is detected
    /// </summary>
    [SerializeField] public GameObject prefab;
    /// <summary>
    /// Position offset relative to the tracked image marker
    /// </summary>
    [SerializeField] public Vector3 positionOffset = Vector3.zero;
    /// <summary>
    /// Rotation offset applied to the spawned object
    /// </summary>
    [SerializeField] public Vector3 rotationOffset = Vector3.zero;
    /// <summary>
    /// Scale multiplier for the spawned object
    /// </summary>
    [SerializeField] public Vector3 scale = Vector3.one;

    [Header("Animation Settings")]
    /// <summary>
    /// Enable bobbing animation for the spawned object
    /// </summary>
    [SerializeField] public bool enableBobbing = false;
    /// <summary>
    /// Speed of the bobbing animation
    /// </summary>
    [SerializeField] public float bobbingSpeed = 2f;
    /// <summary>
    /// Amplitude of the bobbing movement on each axis
    /// </summary>
    [SerializeField] public Vector3 bobbingAmplitude = new Vector3(0, 0.1f, 0);
    /// <summary>
    /// Enable rotation animation for the spawned object
    /// </summary>
    [SerializeField] public bool enableRotation = false;
    /// <summary>
    /// Rotation speed in degrees per second for each axis
    /// </summary>
    [SerializeField] public Vector3 rotationSpeed = new Vector3(0, 50, 0);
}

[System.Serializable]
public class AnimatedPrefabData
{
    /// <summary>
    /// The spawned GameObject that is being animated
    /// </summary>
    public GameObject gameObject;
    /// <summary>
    /// Configuration settings for the animated prefab
    /// </summary>
    public ImagePrefabPair config;
    /// <summary>
    /// Original world position for bobbing animation calculations
    /// </summary>
    public Vector3 originalPosition;
    /// <summary>
    /// Random phase offsets for natural bobbing movement
    /// </summary>
    public Vector3 bobbingOffsets;

    /// <summary>
    /// Initialize animated prefab data with random bobbing offsets for natural movement
    /// </summary>
    public AnimatedPrefabData(GameObject obj, ImagePrefabPair cfg)
    {
        gameObject = obj;
        config = cfg;
        originalPosition = obj.transform.position;
        bobbingOffsets = new Vector3(
            Random.Range(0f, Mathf.PI * 2f),
            Random.Range(0f, Mathf.PI * 2f),
            Random.Range(0f, Mathf.PI * 2f)
        );
    }
}

public class MultiPrefabImageTracker : MonoBehaviour
{
    /// <summary>
    /// List of image prefab pairs configured in the inspector
    /// </summary>
    [SerializeField] private List<ImagePrefabPair> imagePrefabPairs = new List<ImagePrefabPair>();

    /// <summary>
    /// Unity's AR component for image recognition
    /// </summary>
    private ARTrackedImageManager trackedImageManager;
    /// <summary>
    /// Fast lookup dictionary mapping image names to their prefab configurations
    /// </summary>
    private Dictionary<string, List<ImagePrefabPair>> imageNameToPrefabPairs = new Dictionary<string, List<ImagePrefabPair>>();
    /// <summary>
    /// Set of image names that have already been spawned to prevent duplicates
    /// </summary>
    private HashSet<string> spawnedImages = new HashSet<string>();
    /// <summary>
    /// List of spawned prefabs that require animation updates
    /// </summary>
    private List<AnimatedPrefabData> animatedPrefabs = new List<AnimatedPrefabData>();

    /// <summary>
    /// Initialize the AR image manager and build lookup dictionary for image prefab pairs
    /// </summary>
    void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();

        foreach (var pair in imagePrefabPairs)
        {
            if (!string.IsNullOrEmpty(pair.imageName) && pair.prefab != null)
            {
                if (!imageNameToPrefabPairs.ContainsKey(pair.imageName))
                {
                    imageNameToPrefabPairs[pair.imageName] = new List<ImagePrefabPair>();
                }
                imageNameToPrefabPairs[pair.imageName].Add(pair);
            }
        }
    }

    /// <summary>
    /// Enable AR image tracking events when component is enabled
    /// </summary>
    void OnEnable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnImageChanged);
        }
    }

    /// <summary>
    /// Disable AR image tracking events when component is disabled
    /// </summary>
    void OnDisable()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
        }
    }

    /// <summary>
    /// Handle AR image tracking events when images are added or updated
    /// </summary>
    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        foreach (var trackedImage in eventArgs.added)
        {
            HandleTrackedImage(trackedImage);
        }

        foreach (var trackedImage in eventArgs.updated)
        {
            HandleTrackedImage(trackedImage);
        }
    }

    /// <summary>
    /// Process a tracked AR image and spawn prefabs if it's a new detection
    /// </summary>
    void HandleTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage.referenceImage == null || trackedImage.trackingState != TrackingState.Tracking)
        {
            return;
        }

        string imageName = trackedImage.referenceImage.name;

        if (string.IsNullOrEmpty(imageName) || spawnedImages.Contains(imageName))
        {
            return;
        }

        if (imageNameToPrefabPairs.TryGetValue(imageName, out List<ImagePrefabPair> pairs))
        {
            SpawnPrefabs(trackedImage, pairs, imageName);
            spawnedImages.Add(imageName);
        }
    }

    /// <summary>
    /// Spawn and setup prefabs for a detected AR image
    /// </summary>
    void SpawnPrefabs(ARTrackedImage trackedImage, List<ImagePrefabPair> pairs, string imageName)
    {
        foreach (var pair in pairs)
        {
            if (pair.prefab == null)
            {
                continue;
            }

            GameObject spawnedObject = Instantiate(pair.prefab);
            
            spawnedObject.transform.SetParent(trackedImage.transform, false);
            spawnedObject.transform.localPosition = pair.positionOffset;
            spawnedObject.transform.localRotation = Quaternion.Euler(pair.rotationOffset);
            spawnedObject.transform.localScale = pair.scale;

            Vector3 worldPosition = spawnedObject.transform.position;
            Quaternion worldRotation = spawnedObject.transform.rotation;
            Vector3 worldScale = spawnedObject.transform.lossyScale;

            spawnedObject.transform.SetParent(null);
            spawnedObject.transform.position = worldPosition;
            spawnedObject.transform.rotation = worldRotation;
            spawnedObject.transform.localScale = worldScale;

            if (pair.enableBobbing || pair.enableRotation)
            {
                animatedPrefabs.Add(new AnimatedPrefabData(spawnedObject, pair));
            }
        }
    }

    /// <summary>
    /// Update animations for all spawned animated prefabs each frame
    /// </summary>
    void Update()
    {
        for (int i = animatedPrefabs.Count - 1; i >= 0; i--)
        {
            var animData = animatedPrefabs[i];

            if (animData.gameObject == null)
            {
                animatedPrefabs.RemoveAt(i);
                continue;
            }

            if (!animData.gameObject.activeInHierarchy)
            {
                continue;
            }

            var config = animData.config;
            var transform = animData.gameObject.transform;

            if (config.enableBobbing)
            {
                Vector3 bobOffset = new Vector3(
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.x) * config.bobbingAmplitude.x,
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.y) * config.bobbingAmplitude.y,
                    Mathf.Sin(Time.time * config.bobbingSpeed + animData.bobbingOffsets.z) * config.bobbingAmplitude.z
                );
                transform.position = animData.originalPosition + bobOffset;
            }

            if (config.enableRotation)
            {
                Vector3 rotationDelta = config.rotationSpeed * Time.deltaTime;
                transform.Rotate(rotationDelta, Space.Self);
            }
        }
    }
}