using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DescriptionObject : MonoBehaviour, ITappable
{
    /// <summary>
    /// The prefab of the description box 
    /// in which to show the description.
    /// </summary>
    public GameObject descriptionBoxPrefab;
    /// <summary>
    /// The name of the title component in the prefab.
    /// </summary>
    public string titleName = "TitleBackground/Title";
    /// <summary>
    /// The name of the description component in the prefab.
    /// </summary>
    public string descriptionName = "DescriptionBackground/Description";

    /// <summary>
    /// The title to show.
    /// </summary>
    public string title = "N/A";
    /// <summary>
    /// The description to show.
    /// </summary>
    public string description = "N/A";

    /// <summary>
    /// The offset of the description box.
    /// </summary>
    public Vector3 offset = new Vector3(-0.2f, 0f, 0.2f);

    /// <summary>
    /// Camera used to rotate description towards user.
    /// </summary>
    private Camera mainCamera;

    /// <summary>
    /// The current instance of the description box object.
    /// </summary>
    private GameObject descriptionBoxInstance;

    /// <summary>
    /// Starts before first update. Gets the camera in the current scene.
    /// </summary>
    void Start() 
    {
        mainCamera = Camera.main;
    }

    /// <summary>
    /// Updates the rotation of the description box every frame.
    /// </summary>
    void Update() 
    {
        if (descriptionBoxInstance != null)
        {
            Vector3 lookDirection = mainCamera.transform.position - descriptionBoxInstance.transform.position;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                descriptionBoxInstance.transform.rotation = mainCamera.transform.rotation;
            }

            // Checks if description box renders. Else it hides the description box
            Renderer render = gameObject.GetComponent<Renderer>();
            if (render != null)
            {
                if (!render.isVisible)
                {
                    HideDescription();
                }
            }
        }
    }

    /// <summary>
    /// Shows/Hides description on object when tapped.
    /// </summary>
    public void OnTapped() 
    {
        if (descriptionBoxInstance == null) 
        {
            ShowDescription();
        }
        else 
        {
            HideDescription();
        }
    }

    /// <summary>
    /// Shows description on object.
    /// </summary>
    private void ShowDescription() 
    {
        descriptionBoxInstance = Instantiate(descriptionBoxPrefab, gameObject.transform);
        descriptionBoxInstance.transform.position += offset;

        Transform descriptionBoxTitle = descriptionBoxInstance.transform.Find(titleName);
        if (descriptionBoxTitle != null)
        {
            Text titleTextComponent = descriptionBoxTitle.GetComponent<Text>();
            if (titleTextComponent != null)
            {
                titleTextComponent.text = title;
                FitText(titleTextComponent);
            }
        }

        Transform descriptionBoxDescription = descriptionBoxInstance.transform.Find(descriptionName);
        if (descriptionBoxDescription != null)
        {
            Text descriptionTextComponent = descriptionBoxDescription.GetComponent<Text>();
            if (descriptionTextComponent != null)
            {
                descriptionTextComponent.text = description;
                FitText(descriptionTextComponent);
            }
        }

        StartCoroutine(SlideIn(descriptionBoxInstance.transform, 10, AnimationCurve.EaseInOut(0, 0, 1, 1)));
    }

    /// <summary>
    /// Hides description on object.
    /// </summary>
    private void HideDescription() 
    {
        if (descriptionBoxInstance != null) 
        {
            Destroy(descriptionBoxInstance);
        }
    }

    /// <summary>
    /// Fits text to textbox.
    /// </summary>
    private static void FitText(Text textComponent)
    {
        RectTransform rect = textComponent.GetComponent<RectTransform>();
        if (rect != null)
        {
            textComponent.resizeTextForBestFit = true;
        }
    }
    
    /// <summary>
    /// Slides in the description box slowly.
    /// </summary>
    /// <param name="canvasTf">The transform of the description box canvas</param>
    /// <param name="duration">The duration of the slide in</param>
    /// <param name="ease">The ease curve of the animation</param>
    private IEnumerator SlideIn(Transform canvasTf, float duration, AnimationCurve ease)
    {
        CanvasGroup cg = canvasTf.GetComponent<CanvasGroup>();
        if (cg == null)
        {
            cg = canvasTf.gameObject.AddComponent<CanvasGroup>();
        }

        cg.alpha = 0f;
        float fadeDuration = 0.65f;
        float elapsed = 0f;


        while (elapsed < Mathf.Max(duration, fadeDuration))
        {
            elapsed += Time.deltaTime;

            float slideK = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float k = ease.Evaluate(slideK);

            float fadeK = fadeDuration > 0f ? Mathf.Clamp01(elapsed / fadeDuration) : 1f;
            cg.alpha = fadeK;

            yield return null;
        }

        cg.alpha = 1f;

    }
}
