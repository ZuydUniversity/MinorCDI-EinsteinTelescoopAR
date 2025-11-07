using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    // the button that needs to hover - neccesary so the descriptionbox doesnt hover.
    public GameObject button;
    /// <summary>
    /// Set the height of the floating. Higher means the object moves farther up and down from its
    /// starting position.
    /// </summary>
    public float amplitude = 0.1f;
    /// <summary>
    /// the speed of the floating, higher means faster.
    /// </summary>
    public float frequency = 0.3f;
    /// <summary>
    /// Starting position of the object containing this script.
    /// Base for the movement
    /// </summary>
    private Vector3 startPos;

    /// <summary>
    /// Runs on start, records initial position of the object so the floating
    /// can occur from here.
    /// </summary>
    void Start()
    { 
        startPos = button.transform.position;
    }
    /// <summary>
    /// It calculates a new vertical position and updates the objects
    /// position to create a smooth floating effect.
    /// </summary>
    void Update()
    {
        //calculate the new Y position
        float newY = startPos.y + Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        //apply the updated floating position while keeping X and Z unchanged.
        button.transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
