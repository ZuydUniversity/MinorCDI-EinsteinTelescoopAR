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
    /// The audio clip to play when lever is turned off/on.
    /// </summary>
    public AudioClip audioClip;
    /// <summary>
    /// The audio source containing the audio.
    /// </summary>
    private AudioSource audioSource;

    /// <summary>
    /// Is animation playing.
    /// </summary>
    private bool animationIsPlaying = false;

    /// <summary>
    /// Will get called when lever is activated.
    /// </summary>
    protected abstract void OnActivate();
    /// <summary>
    /// Will get called when lever is deactivated.
    /// </summary>
    protected abstract void OnDeactivate();

    /// <summary>
    /// Loads the audio clip into the audio source.
    /// </summary>
    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.clip = audioClip;
    }

    /// <summary>
    /// Starts turning on/off the lever when tapped.
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
        if (!animationIsPlaying) 
        {
            if (isOn) 
            {
                animator.Play(turnOffAnimationName);
            }
            else 
            {
                animator.Play(turnOnAnimationName);
            }

            audioSource.Play();
            animationIsPlaying = true;

            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(state.length / animator.speed); // Waits till end of animation

            OnAnimationFinished();
        }
    }

    /// <summary>
    /// When the animation is finished swiches isOn and calls
    /// abstract functions.
    /// <summary>
    private void OnAnimationFinished() 
    {
        isOn = !isOn;
        if (isOn)
        {
            OnActivate();
        }
        else 
        {
            OnDeactivate();
        }

        animationIsPlaying = false;
    }
}
