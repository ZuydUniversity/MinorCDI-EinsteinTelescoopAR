using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARSubsystems;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    /// <summary>
    /// maincamera used for raycasting for tap detection
    /// </summary>
    public Camera mainCamera;

    /// <summary>
    /// Checks each frame if the screen is tapped
    /// </summary>
    void Update()
    {
        CheckScreenTap();
    }

    /// <summary>
    /// Checks if screen is tapped
    /// </summary>
    void CheckScreenTap()
    {
        Vector2 touchPos;
        //  Check for touchscreen
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            touchPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // Fallback for Editor testing
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            touchPos = Mouse.current.position.ReadValue();
        }
        else
        {
            return;
        }

        if (!isUIElement(touchPos))
        {
            Ray ray = mainCamera.ScreenPointToRay(touchPos);
            Debug.DrawRay(ray.origin, ray.direction * 10f, Color.red, 2f);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ITappable tappable = hit.transform.GetComponent<ITappable>();
                if (tappable != null) 
                {
                    tappable.OnTapped();
                }
            }
        }
    }

    /// <summary>
    /// Checks if an UI element is hit.
    /// </summary>
    /// <param name="screenPosition" type="Vector2">The position on the screen.</param>
    private bool isUIElement(Vector2 screenPosition)
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);
        return raycastResults.Count != 0;
    }
}
