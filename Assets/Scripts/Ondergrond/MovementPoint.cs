using System.Collections.Generic;
using UnityEngine;

public class MovementPoint : MonoBehaviour
{
    /// <summary>
    /// Id for point recognisation
    /// </summary>
    public string pointID;
    /// <summary>
    /// Point the player can move to from this point
    /// </summary>
    public List<MovementPoint> connectedPoints = new List<MovementPoint>();


}

