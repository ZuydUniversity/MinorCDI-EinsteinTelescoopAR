using System.Collections.Generic;
using UnityEngine;

public class MovementPoint : MonoBehaviour
{
    public string pointID;
    public List<MovementPoint> connectedPoints = new List<MovementPoint>();


    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.WorldSpace || canvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            canvas.worldCamera = Camera.main;
        }
    }

}

