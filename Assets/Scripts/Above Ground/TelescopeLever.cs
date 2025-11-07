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
    /// The ground to hide/show.
    /// </summary>
    public GameObject groundObject;

    /// <summary>
    /// The audio source containing the audio.
    /// </summary>
    public AudioSource audioSource;


    /// <summary>
    /// Shows telescope and plays when activated.
    /// </summary>
    public override void OnActivate()
    {
        TelescopeObject.SetActive(true);
        groundObject.SetActive(false);

        audioSource.Play();

    }

    /// <summary>
    /// Hides telescope and stops sound when deactivated.
    /// </summary>
    public override void OnDeactivate()
    {
        audioSource.Stop();

        TelescopeObject.SetActive(false);
        groundObject.SetActive(true);
    }
}
