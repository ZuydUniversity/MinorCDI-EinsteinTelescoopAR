using UnityEngine;
using UnityEngine.UI;

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
}
