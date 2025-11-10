using UnityEngine;

/// <summary>
/// Class is used for the lever that toggles show waves.
/// <summary>
public class StarSpawnerLever : Lever
{
    /// <summary>
    /// The star spawner to activate/deactivate.
    /// </summary>
    public StarSpawner spawner;

    /// <summary>
    /// Activates the star spawner.
    /// </summary>
    protected override void OnActivate()
    {
        spawner.StartSpawner();
    }

    /// <summary>
    /// Deactivates the star spawner.
    /// </summary>
    protected override void OnDeactivate() 
    {
        spawner.StopSpawner();
    }
}
