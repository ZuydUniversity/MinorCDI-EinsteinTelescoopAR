using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class ARObjectSceneSwitcher : MonoBehaviour
{
    [SerializeField] private string targetSceneName = "LiftSceneUI";
    [SerializeField] private float maxRaycastDistance = 50f;
    
    private Camera mainCamera;
    private Collider objectCollider;
    
    void Start()
    {
        mainCamera = Camera.main;
        objectCollider = GetComponent<Collider>();
        
        if (objectCollider == null)
        {
            BoxCollider collider = gameObject.AddComponent<BoxCollider>();
            
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            if (renderers.Length > 0)
            {
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++)
                {
                    bounds.Encapsulate(renderers[i].bounds);
                }
                
                collider.center = transform.InverseTransformPoint(bounds.center);
                collider.size = bounds.size;
            }
            
            objectCollider = collider;
        }
    }
    
    void Update()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            CheckTouch(touchPosition);
        }
        
        #if UNITY_EDITOR
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();
            CheckTouch(mousePosition);
        }
        #endif
    }
    
    void CheckTouch(Vector2 screenPosition)
    {
        if (mainCamera == null)
            return;
        
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, maxRaycastDistance))
        {
            if (hit.collider == objectCollider || hit.collider.transform.IsChildOf(transform))
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