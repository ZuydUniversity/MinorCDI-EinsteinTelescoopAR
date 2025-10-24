using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.SceneManagement;

public class SelectionManager : MonoBehaviour
{
    public GameObject canvasPrefab;
    public Camera mainCamera;
    public GameObject lastClickedObject;
    public GameObject einsteinModel;

    void Start()
    {
        lastClickedObject = null;
    }

    /// <summary>
    /// Find the einstein telescope once the scene has loaded 
    /// </summary>
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

   private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Above Ground")
        {
            Debug.Log("Above Ground scene loaded — waiting to find EinsteinTelescoop...");
            StartCoroutine(FindEinsteinWhenReady());
        }
    }

    private IEnumerator FindEinsteinWhenReady()
    {
        float timer = 0f;

        while (einsteinModel == null && timer < 3f)
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene s = SceneManager.GetSceneAt(i);
                if (s.name == "Above Ground")
                {
                    foreach (GameObject rootObj in s.GetRootGameObjects())
                    {
                        if (rootObj.name == "EinsteinTelescoop")
                        {
                            einsteinModel = rootObj;
                            break;
                        }
                    }
                }
            }

            if (einsteinModel != null)
            {
                einsteinModel.SetActive(false);
                Debug.Log("✅ EinsteinTelescoop found and linked automatically (via additive scene search)!");
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        Debug.LogWarning("⚠️ EinsteinTelescoop still not found after waiting!");
    }

    /// <summary>
    /// Checkt elke frame of er geklikt wordt op het scherm
    /// </summary>
    void Update()
    {
        Vector2 touchPos;

        //  Check mobiele touchscreen
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // Fallback voor Editor testing
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPos = Mouse.current.position.ReadValue();
        }
        else
        {
            return;
        }

        Ray ray = mainCamera.ScreenPointToRay(touchPos);
        Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GameObject clicked = hit.collider.gameObject;
            if (clicked.GetComponent<ClickableObject>().isLever)
            {
                clicked.GetComponent<ClickableObject>().ToggleLever();
                ShowEinsteinModel();
            }
            else
            {
                if (lastClickedObject != clicked)
                {
                    if (lastClickedObject != null) DeleteText(lastClickedObject);
                    lastClickedObject = clicked;
                    SummonText(clicked);
                }
            }


        }

    }
    /// <summary>
    /// spawnt tekst wanneer op een clickable object geklikt wordt dat geen lever is
    /// </summary>
    /// <param name="gameObject"></param>
    void SummonText(GameObject gameObject)
    {
        GameObject canvasInstance = Instantiate(canvasPrefab, gameObject.transform);
        Vector3 lookDirection = mainCamera.transform.position - canvasInstance.transform.position;
        lookDirection.y = 0;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            canvasInstance.transform.rotation = Quaternion.LookRotation(-lookDirection);
        }


        ClickableObject data = gameObject.GetComponent<ClickableObject>();
        if (data != null)
        {
            Transform title = canvasInstance.transform.Find("TitleBackground/Title");
            if (title != null)
            {
                Text titleText = title.GetComponent<Text>();
                if (titleText != null)
                {
                    titleText.text = data.title;
                    TextResizer.FitText(titleText);
                }
            }

            Transform description = canvasInstance.transform.Find("DescriptionBackground/Description");
            if (description != null)
            {
                Text descText = description.GetComponent<Text>();
                if (descText != null)
                {
                    descText.text = data.description;
                    TextResizer.FitText(descText);
                }
            }
        }
    }

    /// <summary>
    /// Toggle the Einstein telescope on/off when the lever is clicked.
    /// </summary>
    void ShowEinsteinModel()
    {
        if (einsteinModel != null)
        {
            bool newState = !einsteinModel.activeSelf; // invert current state
            einsteinModel.SetActive(newState);

            if (newState)
            {
                Debug.Log("EinsteinTelescoop activated!");
            }
            else
            {
                Debug.Log("EinsteinTelescoop deactivated!");
            }
        }
        else
        {
            Debug.LogWarning("Einstein model reference is missing in SelectionManager!");
        }
    }

    /// <summary>
    /// is uitleg echt nodig??
    /// </summary>  
    /// <param name="gameObject"></param>
    void DeleteText(GameObject gameObject)
    {
        foreach (Transform t in gameObject.transform)
        {
            if (t.gameObject.CompareTag("TextCanvas"))
            {
                Destroy(t.gameObject);
            }
        }
    }
}
/// <summary>
/// past text size aan zodat het altijd binnen de lijntjes past
/// </summary>
public static class TextResizer
{
    public static void FitText(Text textComponent, int maxFontSize = 300, int minFontSize = 100)
    {
        RectTransform rect = textComponent.GetComponent<RectTransform>();
        if (rect == null) return;

        // Start van max grootte naar beneden werken tot het past
        textComponent.resizeTextForBestFit = true;
        textComponent.resizeTextMaxSize = maxFontSize;
        textComponent.resizeTextMinSize = minFontSize;
    }
}