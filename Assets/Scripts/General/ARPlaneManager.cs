using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class FloorLock : MonoBehaviour
{
    public ARPlaneManager planeManager;
    public GameObject floorIndicator; // Optional visual cue
    private bool floorLocked = false;
    private Pose floorPose;

    void OnEnable()
    {
        planeManager.planesChanged += OnPlanesChanged;
    }

    void OnDisable()
    {
        planeManager.planesChanged -= OnPlanesChanged;
    }

    void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        if (floorLocked) return;

        foreach (var plane in args.added)
        {
            // Check if plane is horizontal and facing up
            if (plane.alignment == PlaneAlignment.HorizontalUp)
            {
                LockFloor(plane);
                break;
            }
        }
    }

    void LockFloor(ARPlane plane)
    {
        floorLocked = true;
        floorPose = new Pose(plane.center, Quaternion.identity);

        // Optionally show a floor indicator at the locked position
        if (floorIndicator)
            floorIndicator.transform.SetPositionAndRotation(floorPose.position, floorPose.rotation);

        // Disable further plane updates
        foreach (var p in planeManager.trackables)
            p.gameObject.SetActive(p == plane);

        planeManager.enabled = false; // stop new planes from appearing
    }

    public Vector3 GetFloorPosition()
    {
        return floorPose.position;
    }
}

