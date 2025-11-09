using UnityEngine;
using System.Collections;

/// <summary>
/// Class is used for the lever that toggles the laser.
/// <summary>
public class LaserLever : Lever
{
    /// <summary>
    /// Animator for the laser.
    /// <summary>
    public Animator laserAnimator;
    /// <summary>
    /// The animation to activate the laser.
    /// </summary>
    public string activateAnimation = "ActivateLaser";
    /// <summary>
    /// The animation to deactivate the laser.
    /// </summary>
    public string deactivateAnimation = "DeactivateLaser";

    /// <summary>
    /// The audio source containing the audio.
    /// </summary>
    public AudioSource audioSource;

    /// <summary>
    /// The laser object containing the indecator.
    /// </summary>
    public GameObject laser;
    /// <summary>
    /// Indicator of where the laser is.
    /// </summary>
    private HUDIndicator.IndicatorOffScreen indicator;

    /// <summary>
    /// Activates the laser by starting the animation and starts audio of the laser after 3 seconds.
    /// </summary>
    protected override void OnActivate()
    {
        if (indicator == null)
        {
            indicator = laser.GetComponent<HUDIndicator.IndicatorOffScreen>();
        }

        indicator.visible = true;

        laserAnimator.Play(activateAnimation);
        StartCoroutine(PlayAudioWithDelay(3));
    }

    /// <summary>
    /// Deactivates the laser by playing the animation in reverse.
    /// </summary>
    protected override void OnDeactivate()
    {
        indicator.visible = false;

        laserAnimator.Play(deactivateAnimation);
        audioSource.Stop();
    }

    /// <summary>
    /// Plays the audio after a certain delay.
    /// </summary>
    /// <param name="delayTime" type="float">How long to wait until to play the audio.</param>
    IEnumerator PlayAudioWithDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        audioSource.Play();
    }
}
