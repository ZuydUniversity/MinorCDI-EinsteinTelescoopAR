using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SelectionManager : MonoBehaviour
{
    public GameObject interaction_Info_UI;
    TMP_Text interaction_text;

    private InteractableObject lastClickedObject = null;

    private void Start()
    {
        interaction_text = interaction_Info_UI.GetComponent<TMP_Text>();
        interaction_Info_UI.SetActive(false); 
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) 
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                InteractableObject interactable = hit.transform.GetComponent<InteractableObject>();

                if (interactable != null)
                {
                    if (lastClickedObject == interactable && interaction_Info_UI.activeSelf)
                    {
                        interaction_Info_UI.SetActive(false);
                        lastClickedObject = null;
                    }
                    else
                    {
                        interaction_text.text = interactable.GetItemName();
                        interaction_Info_UI.SetActive(true);
                        lastClickedObject = interactable;
                    }
                }
            }
        }
    }
}
