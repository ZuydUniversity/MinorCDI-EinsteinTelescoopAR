using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class MovementManager : MonoBehaviour
{
    public GameObject xrOrigin; 
    public Camera mainCamera;
    public GameObject arrowButtonPrefab;

    private Dictionary<string, MovementPoint> movementPoints = new();
    private MovementPoint currentPoint;
    private Vector3 fixedPosition;

    void Start()
    {
        if (xrOrigin == null)
        {
            xrOrigin = GameObject.Find("XR Origin (AR Rig)");
        }

        foreach (var point in gameObject.GetComponentsInChildren<MovementPoint>())
        {
            movementPoints[point.pointID] = point;
        }

        if (movementPoints.TryGetValue("1", out var start))
        {
            MoveToPoint(start);
        }
    }
    private void LateUpdate()
    {
        xrOrigin.transform.localPosition = fixedPosition;

        Vector3 euler = xrOrigin.transform.eulerAngles;
        euler.y = mainCamera.transform.eulerAngles.y;
        xrOrigin.transform.eulerAngles = euler;
    }

    public void MoveToPoint(MovementPoint newPoint)
    {
        currentPoint = newPoint;
        StartCoroutine(SmoothMove(xrOrigin.transform, newPoint, 2f));

        // Oude pijlen weg
        foreach (var arrow in GameObject.FindGameObjectsWithTag("MoveArrow"))
        {
            Destroy(arrow);
        }
    }

    private void CreateArrowTo(MovementPoint target)
    {
        var arrow = Instantiate(arrowButtonPrefab, target.transform.position, Quaternion.identity);
        arrow.tag = "MoveArrow";
        arrow.GetComponent<Canvas>().worldCamera = Camera.main;

        // zodat de pijl altijd naar de camera kijkt
        Vector3 cameraPosition = mainCamera.transform.position;
        cameraPosition.y = arrow.transform.position.y; 
        arrow.transform.LookAt(cameraPosition);

        // button listener
        Button[] buttons = arrow.GetComponentsInChildren<Button>();
        foreach (var button in buttons)
        {
            if (button != null)
            {
                button.onClick.AddListener(() => MoveToPoint(target));
            }
        }
    }
    
    private IEnumerator SmoothMove(Transform obj, MovementPoint newPoint, float duration)
        {
            Vector3 startPos = obj.position;
            float t = 0;
            while (t < 1)
            {
                t += Time.deltaTime / duration;
                fixedPosition = Vector3.Lerp(startPos, newPoint.transform.position, t);
                yield return null;
            }

            fixedPosition = newPoint.transform.position;

            // Nieuwe pijlen
            foreach (var target in newPoint.connectedPoints)
            {
                CreateArrowTo(target);
            }
        }
    }
