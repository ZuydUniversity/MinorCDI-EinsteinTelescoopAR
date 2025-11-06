using UnityEngine;
using System.Collections;

/// <summary>
/// Base class for lever objects.
/// </summary>
public abstract class Lever : MonoBehaviour, ITappable
{
    
    /// <summary>
    /// Current state of lever.
    /// </summary>
    public bool isOn = false;

    /// <summary>
    /// Animator used to play animations.
    /// </summary>
    public Animator animator;
    /// <summary>
    /// Name of animation to turn on the lever.
    /// </summary>
    public string turnOnAnimationName = "TurnOnLever";
    /// <summary>
    /// Name of animation to turn off the lever.
    /// </summary>
    public string turnOffAnimationName = "TurnOffLever";
        
    /// <summary>
    /// Is animation playing.
    /// </summary>
    private bool animationIsPlaying = false;

    /// <summary>
    /// Will get called when lever is activated.
    /// </summary>
    public abstract void OnActivate();
    /// <summary>
    /// Will get called when lever is deactivated.
    /// </summary>
    public abstract void OnDeactivate();

    /// <summary>
    /// Gets executed when tapped on object.
    /// </summary>
    public void OnTapped()
    {
        StartCoroutine(StartAnimation());
    }

    /// <summary>
    /// Plays animation on tap.
    /// </summary>
    private IEnumerator StartAnimation() 
    {
        if (!animationIsPlaying && animator != null) 
        {
            if (isOn) 
            {
                animator.Play(turnOffAnimationName);
                animationIsPlaying = true;
            }
            else 
            {
                animator.Play(turnOnAnimationName);
                animationIsPlaying = true;
            }

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(state.length / animator.speed); // Waits till end of animation

            OnAnimationFinished();
        }
    }

    private void OnAnimationFinished() 
    {
        isOn = !isOn;
        if (isOn)
        {
            OnActivate();
            animationIsPlaying = false;
        }
        else 
        {
            OnDeactivate();
            animationIsPlaying = false;
        }
    }
}
