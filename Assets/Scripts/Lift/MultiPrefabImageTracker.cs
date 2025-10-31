using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// The alignment mode to spawn the prefabs with.
/// </summary>
public enum SpawnAlignmentMode
{
    GroundAligned,   // Upright in world space
    MarkerSurface    // Matches the image’s plane orientation
}

[System.Serializable]
public class ImagePrefabPair
{
    [Header("Marker Settings")]
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
    /// Gives the model an offset relative to the axis it is looking at.
    /// </summary>
    [SerializeField] public float forwardOffset = 0f;
    /// <summary>
    /// Gives the model an offset relative to the axis on it's right side.
    /// </summary>
    [SerializeField] public float rightOffset = 0f;
    /// <summary>
    /// Rotation offset applied to the spawned object
    /// </summary>
    [SerializeField] public Vector3 rotationOffset = Vector3.zero;
    /// <summary>
    /// Scale multiplier for the spawned object
    /// </summary>
    [SerializeField] public Vector3 scale = Vector3.one;

    [Header("Spawn Alignment")]
    /// <summary>
    /// The alignment setting of the prefab.
    /// </summary>
    [SerializeField] public SpawnAlignmentMode alignmentMode = SpawnAlignmentMode.GroundAligned;

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

[RequireComponent(typeof(ARTrackedImageManager))]
public class MultiPrefabImageTracker : MonoBehaviour
{
    /// <summary>
    /// The qr scanner ui that gives visual indication of the scanning.
    /// </summary>
    public QRScanner scannerUI;

    /// <summary>
    /// List of image prefab pairs configured in the inspector
    /// </summary>
    [SerializeField] private List<ImagePrefabPair> imagePrefabPairs = new List<ImagePrefabPair>();

    /// <summary>
    /// Unity's AR component for image recognition
    /// </summary>
    private ARTrackedImageManager trackedImageManager;
    /// <summary>
    /// Used for anchoring the spawned prefabs.
    /// </summary>
    private ARAnchorManager anchorManager;
    /// <summary>
    /// Fast lookup dictionary mapping image names to their prefab configurations
    /// </summary>
    private Dictionary<string, List<ImagePrefabPair>> imageNameToPrefabPairs = new Dictionary<string, List<ImagePrefabPair>>();
    
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
        anchorManager = FindFirstObjectByType<ARAnchorManager>();

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
            if (scannerUI != null) 
            {
                scannerUI.OnScanSuccess();
            }

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

            Vector3 position = trackedImage.transform.position + trackedImage.transform.TransformVector(pair.positionOffset);
            Quaternion rotation;

            if (pair.alignmentMode == SpawnAlignmentMode.MarkerSurface)
            {
                rotation = trackedImage.transform.rotation * Quaternion.Euler(pair.rotationOffset);
            }
            else
            {
                Vector3 euler = trackedImage.transform.rotation.eulerAngles;
                euler.x = 0f;
                euler.z = 0f;
                rotation = Quaternion.Euler(euler) * Quaternion.Euler(pair.rotationOffset);
            }

            spawnedObject.transform.position = position;
            spawnedObject.transform.rotation = rotation;
            spawnedObject.transform.localScale = pair.scale;

            if (anchorManager != null)
            {
                ARAnchor anchor = null;

                if (trackedImage != null)
                {
                    anchor = trackedImage.gameObject.GetComponent<ARAnchor>();
                    if (anchor == null)
                    {
                        anchor = trackedImage.gameObject.AddComponent<ARAnchor>();
                    }
                }

                
                if (anchor == null) // If for some reason trackedImage is null, create a standalone world anchor
                {
                    GameObject anchorGO = new GameObject($"Anchor_{imageName}");
                    anchorGO.transform.position = position;
                    anchorGO.transform.rotation = rotation;
                    anchor = anchorGO.AddComponent<ARAnchor>();
                }

                if (anchor != null)
                {
                    spawnedObject.transform.SetParent(anchor.transform);
                }
            }
            else
            {
                spawnedObject.transform.SetParent(null);
            }

            if (pair.enableBobbing || pair.enableRotation)
            {
                var animData = new AnimatedPrefabData(spawnedObject, pair);
                animData.originalPosition = spawnedObject.transform.position;
                animatedPrefabs.Add(animData);
            }

            spawnedObject.transform.position += spawnedObject.transform.right * pair.rightOffset;
            spawnedObject.transform.position += spawnedObject.transform.forward * pair.forwardOffset;
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

            var cfg = animData.config;
            var t = animData.gameObject.transform;

            if (cfg.enableBobbing)
            {
                Vector3 bobOffset = new Vector3(
                    Mathf.Sin(Time.time * cfg.bobbingSpeed + animData.bobbingOffsets.x) * cfg.bobbingAmplitude.x,
                    Mathf.Sin(Time.time * cfg.bobbingSpeed + animData.bobbingOffsets.y) * cfg.bobbingAmplitude.y,
                    Mathf.Sin(Time.time * cfg.bobbingSpeed + animData.bobbingOffsets.z) * cfg.bobbingAmplitude.z
                );
                t.position = animData.originalPosition + bobOffset;
            }

            if (cfg.enableRotation)
            {
                Vector3 rotationDelta = cfg.rotationSpeed * Time.deltaTime;
                t.Rotate(rotationDelta, Space.Self);
            }
        }
    }
}
