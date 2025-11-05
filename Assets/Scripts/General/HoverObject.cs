using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Floating Settings")]
    public float amplitude = 0.5f;   
    public float frequency = 1f;    

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * frequency * 2f * Mathf.PI) * amplitude;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
