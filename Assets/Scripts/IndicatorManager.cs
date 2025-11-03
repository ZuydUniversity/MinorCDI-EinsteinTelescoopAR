using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class IndicatorManager : MonoBehaviour
{
    [Header("Tracked objects")]
    public List<Transform> targets = new List<Transform>();

    [Header("Prefabs & References")]
    [Tooltip("Zet hier de indicator prefab in.")]
    public GameObject indicatorPrefab;
    [Tooltip("Zet hier de main camera van de scene in als die afwijkt.")]
    public Camera mainCamera;
    [Tooltip("Zet hier de canvas van de scene in.")]
    public Canvas canvas;

    [Header("Screen edge padding")]
    public float edgePadding = 50f;

    private Dictionary<Transform, GameObject> indicators = new Dictionary<Transform, GameObject>();

    void Start()
    {
        if (!mainCamera)
            mainCamera = Camera.main;

        foreach (Transform target in targets)
            CreateIndicator(target);
    }

    void CreateIndicator(Transform target)
    {
        if (!indicatorPrefab || indicators.ContainsKey(target))
            return;

        GameObject newIndicator = Instantiate(indicatorPrefab, canvas.transform);
        indicators[target] = newIndicator;
    }

    void Update()
    {
        if (!mainCamera)
            mainCamera = Camera.main;
        
        foreach (var pair in indicators)
        {
            Transform target = pair.Key;
            GameObject indicator = pair.Value;

            if (target == null)
            {
                Destroy(indicator);
                continue;
            }

            UpdateIndicator(target, indicator);
        }
    }

    void UpdateIndicator(Transform target, GameObject indicator)
    {
        Vector3 screenPoint = mainCamera.WorldToScreenPoint(target.position);

        RectTransform indicatorRect = indicator.GetComponent<RectTransform>();
        //Transform arrow = indicator.transform.Find("Arrow");
        //Transform pulse = indicator.transform.Find("Pulse"); //TODO: Split prefab into two separate parts

        bool isVisible = screenPoint.z > 0 &&
                         screenPoint.x > 0 && screenPoint.x < Screen.width &&
                         screenPoint.y > 0 && screenPoint.y < Screen.height;

        //arrow.gameObject.SetActive(!isVisible);
        //pulse.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            indicatorRect.position = screenPoint;
        }
        else
        {
            // Project direction onto screen edge
            Vector3 fromCenter = screenPoint - new Vector3(Screen.width / 2, Screen.height / 2, 0);
            fromCenter.z = 0;
            fromCenter.Normalize();

            float slope = fromCenter.y / fromCenter.x;
            Vector3 edgePos = Vector3.zero;

            // Default edge positions
            float halfW = Screen.width / 2 - edgePadding;
            float halfH = Screen.height / 2 - edgePadding;

            // Find intersection with screen bounds
            if (Mathf.Abs(slope) < (halfH / halfW)) //TODO: Bug where arrow sometimes points wrong way (probably when behind camera)
            {
                // Hit left or right edge
                edgePos.x = fromCenter.x > 0 ? halfW : -halfW;
                edgePos.y = edgePos.x * slope;
            }
            else
            {
                // Hit top or bottom edge
                edgePos.y = fromCenter.y > 0 ? halfH : -halfH;
                edgePos.x = edgePos.y / slope;
            }

            // Translate back to screen space
            Vector3 screenCenter = new Vector3(Screen.width / 2, Screen.height / 2, 0);
            indicatorRect.position = screenCenter + edgePos;

            // Rotate arrow toward target
            float angle = Mathf.Atan2(fromCenter.y, fromCenter.x) * Mathf.Rad2Deg;
            indicatorRect.localRotation = Quaternion.Euler(0, 0, angle - 90); //TODO: Only turn arrow, not whole indicator
        }
    }
}

