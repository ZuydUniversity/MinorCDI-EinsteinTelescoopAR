using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;

public class SelectionManager : MonoBehaviour
{
    public GameObject canvasPrefab;
    public Camera mainCamera;
    public GameObject lastClickedObject = null;

    private void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject clicked = hit.collider.gameObject;

                if (lastClickedObject != clicked)
                {
                    if (lastClickedObject != null)
                        DeleteText(lastClickedObject);

                    lastClickedObject = clicked;
                    SummonText(clicked);
                }
            }
        }
    }
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
