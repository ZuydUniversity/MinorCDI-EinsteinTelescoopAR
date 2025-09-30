using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.InputSystem; // new Input System

public class ClickObject : MonoBehaviour
{
    public GameObject cube;
    public TMP_Text popupText; 

    void Start()
    {
        popupText.gameObject.SetActive(false);
    }

    void Update()
    {
        // Left click/tap
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (cube == GetClickedObject(out RaycastHit hit))
            {
                ShowPopup("Clicked!");
            }
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            HidePopup();
        }
    }

    void ShowPopup(string message)
    {
        popupText.gameObject.SetActive(true);
        popupText.text = message;
    }

    void HidePopup()
    {
        popupText.gameObject.SetActive(false);
    }

    GameObject GetClickedObject(out RaycastHit hit)
    {
        GameObject target = null;
        Vector2 mousePos = Mouse.current.position.ReadValue();
        var ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out hit, 10f))
        {
            if (!IsPointerOverUIObject())
                target = hit.collider.gameObject;
        }
        return target;
    }

    private bool IsPointerOverUIObject()
    {
        PointerEventData ped = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(ped, results);
        return results.Count > 0;
    }
}
