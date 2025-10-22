using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ARObjectSceneSwitcher : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "LiftSceneUI";
    [SerializeField] private float maxRaycastDistance = 50f;
    [SerializeField] private Collider targetCollider;

    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;

        // If not assigned, try to use a collider on this object only (NOT children/siblings)
        if (targetCollider == null)
        {
            if (!TryGetComponent(out targetCollider))
            {
                // Add a BoxCollider sized to THIS object's own renderer(s), not all children in the prefab.
                var box = gameObject.AddComponent<BoxCollider>();
                var renderers = GetComponents<Renderer>(); // <- note: not InChildren
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

    void LoadTargetScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            SceneManager.LoadScene(targetSceneName);
        }
    }
}
