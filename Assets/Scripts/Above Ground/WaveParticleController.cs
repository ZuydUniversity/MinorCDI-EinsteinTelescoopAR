using UnityEngine;

public class WaveParticleController : MonoBehaviour
{
    public ParticleSystem waveParticles;    // particle system
    public ARStarSpawner starSpawner;       // link to StarSpawner
    private bool isRunning = false;

    public void StartWaveAnimation()
    {
        if (isRunning) return;

        isRunning = true;
        waveParticles.Play();
        Invoke(nameof(StopWaveAnimation), 10f); // 10 sec duration
    }

    private void StopWaveAnimation()
    {
        waveParticles.Stop();

        if (starSpawner != null)
        {
            starSpawner.StartSpawningStars(); // start staranimation 
        }

        isRunning = false;
    }
}
