using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Localization.Components; 
using UnityEngine.Localization;            

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

    public float dynamicPixelSize = 1.8f;


    [Header("AutoScale")]
    public float referenceScreenHeight = 1080f;
    public float referenceFOV = 60f;
    [Range(0f, 1f)] public float distanceEffect = 0.7f;

    private Transform descriptionCanvasTf;
    private float baseDistance ;
    private Vector3 baseScale;

    /// DYNAMIC TEXTSIZE
    [Header("Dynamic Text Size")]
    public int baseTitleFontSize = 32;        // kies jouw basisgroottes
    public int baseDescriptionFontSize = 24;
    public float fontScaleMultiplier = 50.0f;  // extra globale factor
    public int minFontSize = 12;
    public int maxFontSize = 96;


    private Text titleTextComponent;
    private Text descriptionTextComponent;





    /// <summary>
    /// Starts before first update. Gets the camera in the current scene.
    /// </summary>
    void Start() 
    {
        mainCamera = Camera.main;

        /// <summary>
        /// Initializes the localization system for this object.
        /// If a <see cref="LocalizeStringEvent"/> component is found,
        /// it connects its update event to <see cref="SetDescription"/> 
        /// so that localized text updates automatically when the language changes.
        /// It also forces an immediate refresh to ensure the correct 
        /// localized value is applied after the scene loads.
        /// </summary>
        var localizeEvent = GetComponent<LocalizeStringEvent>();
        if (localizeEvent != null)
        {
            
            localizeEvent.OnUpdateString.AddListener(SetDescription);

            
            localizeEvent.RefreshString();
        }
    }

    /// <summary>
    /// Updates the rotation of the description box every frame.
    /// </summary>
    void Update() 
    {
        if (descriptionBoxInstance != null)
        {
            Vector3 lookDirection = mainCamera.transform.position - descriptionCanvasTf.transform.position;
            lookDirection.y = 0;
            if (lookDirection.sqrMagnitude > 0.001f)
            {
                descriptionCanvasTf.transform.rotation = mainCamera.transform.rotation;
            }


            ApplyScaleNow();

        

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


        var canvas = descriptionBoxInstance.GetComponentInChildren<Canvas>(true);
        if (canvas == null) {
            canvas = descriptionBoxInstance.AddComponent<Canvas>();
        }
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = mainCamera;

        descriptionCanvasTf = canvas.transform;

        var rt = canvas.GetComponent<RectTransform>();
        if (rt != null && (rt.sizeDelta.x <= 0f || rt.sizeDelta.y <= 0f))
        {
            rt.sizeDelta = new Vector2(1.6f, 0.88f);
        }

        var scaler = canvas.GetComponent<CanvasScaler>() ?? canvas.gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        scaler.dynamicPixelsPerUnit = dynamicPixelSize;


        // Tekstrefs cachen
        var titleTf = descriptionBoxInstance.transform.Find(titleName);
        if (titleTf != null)
        {
            titleTextComponent = titleTf.GetComponent<Text>();
        }
        var descTf = descriptionBoxInstance.transform.Find(descriptionName);
        if (descTf != null)
        {
            descriptionTextComponent = descTf.GetComponent<Text>();
        }



        if (titleTextComponent != null)
        {
            titleTextComponent.text = title;
            PrepareTextForDynamicSizing(titleTextComponent);
        }
        if (descriptionTextComponent != null)
        {
            descriptionTextComponent.text = description;
            PrepareTextForDynamicSizing(descriptionTextComponent);
        }


        baseDistance = Vector3.Distance(mainCamera.transform.position, descriptionCanvasTf.position);
        baseScale = descriptionCanvasTf.localScale;

        StartCoroutine(ApplyScaleNextFrame());



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
            descriptionBoxInstance = null;
            descriptionCanvasTf = null;
        }
    }


    private static void PrepareTextForDynamicSizing(Text t)
    {
        // BestFit uitzetten zodat we zelf fontSize kunnen sturen
        t.resizeTextForBestFit = false;
        // (optioneel) overflow netjes houden
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Truncate;
    }


    /// <summary>
    /// Fits text to textbox.
    /// </summary>
    private static void FitText(Text textComponent)
    {
        RectTransform rect = textComponent.GetComponent<RectTransform>();
        if (rect != null)
        {
            textComponent.resizeTextForBestFit = false;
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


    private IEnumerator ApplyScaleNextFrame()
    {
        yield return null; // 1 frame wachten voor layout/best fit
        ApplyScaleNow();
    }

    private void ApplyScaleNow()
    {
        if (descriptionCanvasTf == null || mainCamera == null) return;

        // zelfde schaalfactoren als voor het canvas
        float d = Vector3.Distance(mainCamera.transform.position, descriptionCanvasTf.position);
        float distanceRatio = d / Mathf.Max(0.0001f, baseDistance);
        distanceRatio = Mathf.Lerp(1f, distanceRatio, distanceEffect);

        float screenRatio = (float)Screen.height / Mathf.Max(1f, referenceScreenHeight);

        float fovRatio = Mathf.Tan(mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) /
                         Mathf.Tan(referenceFOV * 0.5f * Mathf.Deg2Rad);

        float scaleFactor = distanceRatio * screenRatio * fovRatio;

        // canvas schalen
        descriptionCanvasTf.localScale = baseScale * scaleFactor;

        //  tekstgrootte dynamisch mee laten schalen
        UpdateTextSizes(scaleFactor);
    }

    private void UpdateTextSizes(float scaleFactor)
    {
        if (titleTextComponent != null)
        {
            int titleMaxSize = titleTextComponent.resizeTextMaxSize > 0 ? titleTextComponent.resizeTextMaxSize : maxFontSize;
            
            int fs = Mathf.Clamp(
                Mathf.RoundToInt(baseTitleFontSize * scaleFactor * fontScaleMultiplier),
                minFontSize, titleMaxSize);
            titleTextComponent.fontSize = fs;
        }

        if (descriptionTextComponent != null)
        {
            int fs = Mathf.Clamp(
                Mathf.RoundToInt(baseDescriptionFontSize * scaleFactor * fontScaleMultiplier),
                minFontSize, maxFontSize);
            descriptionTextComponent.fontSize = fs;
        }
    }

    public void SetDescription(string newDescription)
    {
        if (string.IsNullOrEmpty(newDescription))
        {
            Debug.Log($"[Localization] Lege string ontvangen van LocalizeStringEvent voor {name}");
            return;
        }

        Debug.Log($"[Localization] Ontvangen tekst voor {name}: {newDescription}");
        description = newDescription;

        if (descriptionTextComponent != null)
            descriptionTextComponent.text = newDescription;
    }





}
