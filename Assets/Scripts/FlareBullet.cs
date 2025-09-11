// FlareBullet.cs
// This script is for the flare projectile prefab.
// It controls the flare's light, particles, and its destruction.

using System.Collections;
using UnityEngine;

public class FlareBullet : MonoBehaviour
{
    [Header("Flare Components")]
    // The particle system for the flare's light trail and smoke.
    public ParticleSystem flareParticles;

    // The Light component that will emit from the flare.
    public Light flareLight;

    [Header("Destruction")]
    [Tooltip("Select the layers that will cause the flare to destruct upon collision.")]
    public LayerMask layersToDestroyOn;

    [Tooltip("The time in seconds before the flare self-destructs if it doesn't hit anything.")]
    public float selfDestructTime = 15f;

    // Private reference to the Coroutine so we can stop it.
    private Coroutine selfDestructCoroutine;

    void Start()
    {
        // Ensure the flare's effects are on as soon as it's instantiated.
        if (flareParticles != null)
        {
            flareParticles.Play();
        }
        if (flareLight != null)
        {
            flareLight.enabled = true;
        }

        // Start the self-destruct timer immediately.
        selfDestructCoroutine = StartCoroutine(DestroyAfterDelay(selfDestructTime));
    }

    /// <summary>
    /// This method is called when the flare collides with another object.
    /// </summary>
    /// <param name="collision">The collision data.</param>
    void OnCollisionEnter(Collision collision)
    {
        // Check if the collided object's layer is one of the layers specified in the Inspector.
        // This is done using a bitwise operation on the LayerMask.
        if (((1 << collision.gameObject.layer) & layersToDestroyOn) != 0)
        {
            // If the flare hits a valid destruction layer, stop the self-destruct timer.
            if (selfDestructCoroutine != null)
            {
                StopCoroutine(selfDestructCoroutine);
            }

            // Stop the particle effect and turn off the light.
            if (flareParticles != null)
            {
                flareParticles.Stop();
            }
            if (flareLight != null)
            {
                flareLight.enabled = false;
            }

            // Immediately destroy the flare object after hitting a valid layer.
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Coroutine to destroy the flare object after a set delay.
    /// </summary>
    /// <param name="delay">The time in seconds to wait before destruction.</param>
    private IEnumerator DestroyAfterDelay(float delay)
    {
        // Wait for the specified amount of time.
        yield return new WaitForSeconds(delay);

        // Destroy the flare object.
        Destroy(gameObject);
    }
}
