using UnityEngine;
using System.Collections;

/// <summary>
/// Class is used for the lever that toggles the laser for telescope.
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
    /// Activates the laser by starting the animation.
    /// </summary>
    public override void OnActivate()
    {
        laserAnimator.Play(activateAnimation);
    }

    /// <summary>
    /// Deactivates the laser by playing the animation in reverse.
    /// </summary>
    public override void OnDeactivate()
    {
        laserAnimator.Play(deactivateAnimation);
    }
}
