using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ARObjectSceneSwitcher : MonoBehaviour
{
    /// <summary>
    /// The name of the scene that is loaded
    /// </summary>
    [SerializeField] private string targetSceneName = "BovengrondScene";
    /// <summary>
    /// The maximum distance at which a raycast can be performed
    /// </summary>
    [SerializeField] private float maxRaycastDistance = 50f;
    /// <summary>
    /// The collider that is linked to the object for touch interaction
    /// </summary>
    [SerializeField] private Collider targetCollider;

    /// <summary>
    /// AR Camera used for raycasting
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// Initializes camera and creates a BoxCollider for the specific prefab
    /// </summary>
    void Awake()
    {
        mainCamera = Camera.main;

        if (targetCollider == null)
        {
            if (!TryGetComponent(out targetCollider))
            {
                var box = gameObject.AddComponent<BoxCollider>();
                var renderers = GetComponents<Renderer>();
                if (renderers.Length > 0)
                {
                    Bounds b = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
                    box.center = transform.InverseTransformPoint(b.center);
                    box.size = transform.InverseTransformVector(b.size);
                }
                targetCollider = box;
            }
        }
    }

    /// <summary>
    /// Check if it runs on phone/tablet or in Unity Editor and use the appropriate touch
    /// </summary>
    void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            CheckTouch(Touchscreen.current.primaryTouch.position.ReadValue());
        }

    #if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            CheckTouch(Mouse.current.position.ReadValue());
        }
    #endif
    }

    /// <summary>
    /// Checks whether the touch/raycast hits a Collider that is set
    /// </summary>
    void CheckTouch(Vector2 screenPos)
    {
        if (mainCamera != null && targetCollider != null)
        {
            Ray ray = mainCamera.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out var hit, maxRaycastDistance) && hit.collider == targetCollider)
            {
                LoadTargetScene();
            }
        }
    }

    /// <summary>
    /// Load the targetScene that has been set
    /// </summary>
    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
