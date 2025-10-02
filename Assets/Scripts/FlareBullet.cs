// FlareBullet.cs
// This script is for the flare projectile prefab.
// It controls the flare's light, particles, destruction, and flight rotation.

using System.Collections;
using UnityEngine;

public class FlareBullet : MonoBehaviour
{
    [Header("Flare Components")]
    public ParticleSystemRenderer smokeParticleSystemRenderer;
    public Light flareLight;

    private Rigidbody rb;
    // New: Reference to the actual ParticleSystem component for control
    private ParticleSystem smokeParticleSystem;

    [Header("Flight Characteristics")]
    [Tooltip("The speed at which the flare will rotate to match its velocity.")]
    public float rotationSpeed = 5f;

    [Header("Rocket Engine Settings")]
    private float currentEngineThrust;
    private float currentEngineBurnTime;
    private bool isEngineActive = false;

    [Header("Custom Gravity")]
    [Tooltip("Adjust this value to control how fast the bullet falls.")]
    public float customGravityForce = 9.81f;

    [Header("Destruction")]
    public LayerMask layersToDestroyOn;
    public float selfDestructTime = 15f; // Kept for reference, but using flareTimer + 1f for timed destruction
    private Coroutine selfDestructCoroutine;

    public AudioSource flaresound;
    public AudioClip flareBurningSound;
    public float flareTimer = 9;
    private bool myCoroutine;
    private float smooth = 2.4f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("FlareBullet script requires a Rigidbody component on the same GameObject!");
            enabled = false;
        }
    }

    void Start()
    {
        // Initialize components
        if (flareLight != null) flareLight.enabled = true;

        flareLight = GetComponentInChildren<Light>();
        flaresound = GetComponent<AudioSource>();

        // Get the ParticleSystem component from the renderer's GameObject
        if (smokeParticleSystemRenderer != null)
        {
            smokeParticleSystem = smokeParticleSystemRenderer.gameObject.GetComponent<ParticleSystem>();
        }

        flaresound.PlayOneShot(flareBurningSound);

        // Start the flare's entire lifecycle coroutine
        StartCoroutine(FlareLifeCycle());
    }

    public void InitializeEngine(float thrust, float burnTime)
    {
        currentEngineThrust = thrust;
        currentEngineBurnTime = burnTime;
        isEngineActive = true;

        StartCoroutine(EngineBurnTimer());
    }

    private IEnumerator EngineBurnTimer()
    {
        yield return new WaitForSeconds(currentEngineBurnTime);
        isEngineActive = false;
    }

    void FixedUpdate()
    {
        // Apply physics
        if (isEngineActive)
        {
            rb.AddForce(transform.forward * currentEngineThrust, ForceMode.Force);
        }
        rb.AddForce(Vector3.down * customGravityForce, ForceMode.Acceleration);

        // Handle rotation to follow velocity
        if (rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.linearVelocity);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        // Handle light and sound state based on myCoroutine
        if (myCoroutine == true)
        {
            // Initial burn: flickering light
            flareLight.intensity = Random.Range(2f, 6.0f);
        }
        else
        {
            // Fade out sequence: light, sound, and particle size decrease
            flareLight.intensity = Mathf.Lerp(flareLight.intensity, 0f, Time.deltaTime * smooth);
            flareLight.range = Mathf.Lerp(flareLight.range, 0f, Time.deltaTime * smooth);
            flaresound.volume = Mathf.Lerp(flaresound.volume, 0f, Time.deltaTime * smooth);

            // Note: We only fade the renderer's size here, the final destruction waits for particles to vanish.
            smokeParticleSystemRenderer.maxParticleSize = Mathf.Lerp(smokeParticleSystemRenderer.maxParticleSize, 0f, Time.deltaTime * 5);
        }
    }

    // --- Collision and Trigger Destruction ---

    void OnCollisionEnter(Collision collision)
    {
        // THE BITWISE CHECK FOR LAYER
        // Checks if the colliding object's layer is contained within the 'layersToDestroyOn' LayerMask.
        // 1. (1 << other.gameObject.layer): Shifts the number 1 left by the layer index. 
        //    This creates a number where only the bit corresponding to the object's layer is set to 1.
        // 2. & layersToDestroyOn: Performs a bitwise AND between the single-bit number 
        //    and the LayerMask (which is also a bitmask).
        // 3. != 0: If the result is not zero, it means the bits overlapped, 
        //    and therefore the object's layer is included in the mask.
        if (((1 << collision.gameObject.layer) & layersToDestroyOn) != 0)
        {
            HandleDestruction();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // Bitwise check: check if the triggered layer is in layersToDestroyOn
        if (((1 << other.gameObject.layer) & layersToDestroyOn) != 0)
        {
            HandleDestruction();
        }
    }

    // --- Destruction Handlers ---

    // Initiates the destruction sequence, whether from collision or time.
    void HandleDestruction()
    {
        // Stop all existing coroutines (e.g., FlareLifeCycle) to prevent multiple destroy calls.
        StopAllCoroutines();

        // Disable the visual mesh of Bullet
        GetComponentInChildren<MeshRenderer>().enabled = false;

        // Disable the collider (so it doesn't hit anything else)
        GetComponentInChildren<Collider>().enabled = false;

        // Immediately start the fade out sequence in FixedUpdate to fade out Particle System
        myCoroutine = false;

        // Start the controlled sequence that waits for the particles to clear.
        StartCoroutine(ControlledDestroySequence());
    }

    IEnumerator ControlledDestroySequence()
    {
        if (smokeParticleSystem != null)
        {
            // 1. Stop generating new particles, but let existing ones continue to live.
            smokeParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmitting);

            // 2. Wait until all remaining particles have finished their lifespan.
            // IsAlive(true) checks if *any* particles are currently alive.
            yield return new WaitWhile(() => smokeParticleSystem.IsAlive(true));
        }

        // 3. Final step: Destroy the entire GameObject.
        Destroy(gameObject);
    }

    // --- Lifecycle Coroutine ---

    // Manages the timed burn and fade process.
    IEnumerator FlareLifeCycle()
    {
        // 1. Initial full burn state (controlled by flareTimer)
        myCoroutine = true;
        yield return new WaitForSeconds(flareTimer);

        // 2. Signal the start of the fade out (FixedUpdate handles the light/sound fade)
        myCoroutine = false;

        // 3. Wait for the light/sound fade duration (1 second, matching your old Destroy(..., + 1f))
        yield return new WaitForSeconds(1.0f);

        // 4. Initiate the final, controlled destruction (waiting for particles)
        StartCoroutine(ControlledDestroySequence());
    }
}
