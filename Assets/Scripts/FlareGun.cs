// FlareGun.cs
// This script controls the behavior of a single-shot flare gun.
// It instantiates a flare projectile prefab when fired.

using UnityEngine;
using System.Collections;

public class FlareGun : MonoBehaviour
{
    // --- Public Variables ---

    [Header("Flare Gun Components")]
    // The flare projectile prefab to be instantiated.
    public GameObject flarePrefab;

    // The point from which the flare will be launched. This should be a child object
    // of the FlareGun, positioned at the muzzle.
    public Transform firingPoint;

    [Header("Firing & Ammo")]
    // The number of flares the gun has.
    public int maxFlares = 100;

    // The time in seconds between shots.
    public float fireCooldown = 1.0f;

    [Header("Rocket Engine Settings")]
    [Tooltip("The initial short burst of force when the flare is fired.")]
    public float firingImpulse = 10f;

    [Tooltip("The sustained force applied while the rocket engine is active.")]
    public float engineThrust = 5f;

    [Tooltip("The time in seconds that the rocket engine will apply thrust.")]
    public float engineBurnTime = 3f;

    // --- Private Variables ---

    // A flag to check if the gun is ready to fire.
    private bool isLoaded = true;

    // The current number of flares remaining.
    private int currentFlares;

    // --- Unity Methods ---

    void Start()
    {
        // Initialize the current flare count.
        currentFlares = maxFlares;
    }

    // Update is called once per frame.
    // We check for player input here.
    void Update()
    {
        // This script will only run if its GameObject is active.
        // We check for player input and if the gun is currently loaded and has ammo.
        if (Input.GetButtonDown("Fire1") && isLoaded && currentFlares > 0)
        {
            StartCoroutine(Fire());
        }
    }

    // --- Custom Methods ---

    /// <summary>
    /// Fires the flare gun.
    /// This method is called when the player presses the fire button.
    /// </summary>
    private IEnumerator Fire()
    {
        // Prevent firing again until the cooldown is over.
        isLoaded = false;

        // Decrement the flare count.
        currentFlares--;

        // Instantiate a new flare object at the specified fire point's position and rotation.
        if (flarePrefab != null && firingPoint != null)
        {
            // Instantiate the flare at the firingPoint's world position and rotation.
            GameObject flareInstance = Instantiate(flarePrefab, firingPoint.position, firingPoint.rotation);


            // Get the Rigidbody component from a child object if the prefab hierarchy is structured that way.
            Rigidbody flareRb = flareInstance.GetComponentInChildren<Rigidbody>();

            if (flareRb != null)
            {
                // Apply a short, immediate impulse force to get the flare moving.
                flareRb.AddForce(firingPoint.forward * firingImpulse, ForceMode.Impulse);
                Debug.DrawRay(firingPoint.position, firingPoint.forward * 5, Color.red, 2f);

                // Get the FlareBullet script to pass the engine settings.
                FlareBullet flareScript = flareInstance.GetComponentInChildren<FlareBullet>();

                // Pass the engine settings to the flare script.
                if (flareScript != null)
                {
                    flareScript.InitializeEngine(engineThrust, engineBurnTime);
                }


                //Rigidbody bulletInstance;
                //bulletInstance = Instantiate(flareBullet, barrelEnd.position, barrelEnd.rotation) as Rigidbody; //INSTANTIATING THE FLARE PROJECTILE


                //bulletInstance.AddForce(barrelEnd.forward * bulletSpeed); //ADDING FORWARD FORCE TO THE FLARE PROJECTILE
            }
            else
            {
                Debug.LogWarning("The flare prefab is missing a Rigidbody component on its children. The flare will not move.");
            }
            Debug.Log($"Flare gun fired! Flares remaining: {currentFlares}");
        }
        else
        {
            Debug.LogError("Flare Prefab or Fire Point is not assigned in the Inspector! The flare cannot be fired.");
        }

        // Wait for the cooldown before allowing another shot.
        yield return new WaitForSeconds(fireCooldown);
        isLoaded = true;
    }

    /// <summary>
    /// Reloads the flare gun, making it ready to fire again.
    /// This method can be called from another script (e.g., a Player controller).
    /// </summary>
    public void Reload()
    {
        isLoaded = true;
        currentFlares = maxFlares;
        Debug.Log("Flare gun reloaded.");
    }
}
