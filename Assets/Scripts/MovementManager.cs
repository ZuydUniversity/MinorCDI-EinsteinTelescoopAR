using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;
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
            print("point found");
            movementPoints[point.pointID] = point;
        }

        if (movementPoints.TryGetValue("1", out var start))
        {
            print("tping to start");
            MoveToPoint(start);
        }
        fixedPosition = transform.position;
    }
    private void LateUpdate()
    {
        xrOrigin.transform.localPosition = fixedPosition;
        transform.rotation = Quaternion.Euler(0, mainCamera.transform.eulerAngles.y, 0);
    }
    public void MoveToPoint(MovementPoint newPoint)
    {
        currentPoint = newPoint;
        print("movemove");
        StartCoroutine(SmoothMove(xrOrigin.transform, newPoint.transform.position, newPoint.transform.rotation, 0.5f));
        fixedPosition = currentPoint.transform.localPosition;

        // Oude pijlen weg
        foreach (var arrow in GameObject.FindGameObjectsWithTag("MoveArrow"))
        {
            Destroy(arrow);
            print("pijl weg");
        }

        // Nieuwe pijlen
        foreach (var target in newPoint.connectedPoints)
        {
            CreateArrowTo(target);
            print("nieuwe pijl");
        }
    }

    private void CreateArrowTo(MovementPoint target)
    {
        var arrow = Instantiate(arrowButtonPrefab, target.transform.position, Quaternion.identity);
        arrow.tag = "MoveArrow";
        arrow.GetComponent<Canvas>().worldCamera = Camera.main;

        Vector3 lookDirection = mainCamera.transform.position - arrow.transform.position;
        lookDirection.x = 90;
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            arrow.transform.rotation = Quaternion.LookRotation(-lookDirection);
        }

        var btn = arrow.GetComponentInChildren<UnityEngine.UI.Button>();
        if (btn != null)
        {
            btn.onClick.AddListener(() => MoveToPoint(target));
        }
    }
    private IEnumerator SmoothMove(Transform obj, Vector3 targetPos, Quaternion targetRot, float duration)
    {
        Vector3 startPos = obj.position;
        Quaternion startRot = obj.rotation;
        float t = 0;
        print("starting to move");
        while (t < 1)
        {
            t += Time.deltaTime / duration;
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            obj.rotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }
    }


}
