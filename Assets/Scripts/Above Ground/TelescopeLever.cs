using UnityEngine;

/// <summary>
/// Class is used for the lever that toggles xray for telescope.
/// <summary>
public class TelescopeLever : Lever
{

    /// <summary>
    /// The telescope to hide/show.
    /// </summary>
    public GameObject TelescopeObject;

    /// <summary>
    /// Hides telescope when activated.
    /// </summary>
    public override void OnActivate()
    {
        TelescopeObject.SetActive(true);
    }

    /// <summary>
    /// Shows telescope when activated.
    /// </summary>
    public override void OnDeactivate()
    {
        TelescopeObject.SetActive(false);
    }
}
