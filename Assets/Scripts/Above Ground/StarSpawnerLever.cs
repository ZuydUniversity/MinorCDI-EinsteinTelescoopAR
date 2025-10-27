using UnityEngine;

/// <summary>
/// Class is used for the lever that toggles show waves for telescope.
/// <summary>
public class StarSpawnerLever : Lever
{
    /// <summary>
    /// The telescope to hide/show.
    /// </summary>
    public StarSpawner spawner;

    /// <summary>
    /// Hides telescope when activated.
    /// </summary>
    public override void OnActivate()
    {
        spawner.StartAnimation();
    }

    /// <summary>
    /// Shows telescope when activated.
    /// </summary>
    public override void OnDeactivate() {}
}
