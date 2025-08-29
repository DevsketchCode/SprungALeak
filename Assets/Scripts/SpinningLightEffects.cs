using UnityEngine;
using System.Collections;

public class SpinningLightEffect : MonoBehaviour
{
    [Header("Spin Settings")]
    [Tooltip("Speed of rotation in degrees per second.")]
    public float spinSpeed = 360f; // Full rotation per second
    [Tooltip("Axis to spin around (e.g., Vector3.right for X-axis).")]
    public Vector3 spinAxis = Vector3.right; // Spin around the X-axis by default

    [Header("Light Control")]
    [Tooltip("The direct child GameObject containing the actual Unity Light components to be enabled/disabled.")]
    public GameObject lightContainerChild; // Reference to the 'Lights' GameObject

    private bool isSpinning = false;

    void Start()
    {
        // Ensure the light container child starts disabled
        if (lightContainerChild != null)
        {
            lightContainerChild.SetActive(false);
        }
        // The GameObject this script is on (the Reflector) always stays active
    }

    void Update()
    {
        if (isSpinning)
        {
            // Spin this GameObject (the Reflector) around its specified axis
            transform.Rotate(spinAxis * spinSpeed * Time.deltaTime);
        }
    }

    // Call this method to start this GameObject spinning and activate its child lights
    public void StartSpinAndLight()
    {
        if (!isSpinning)
        {
            isSpinning = true;
            Debug.Log($"{gameObject.name} started spinning.");

            // Activate the light container child
            if (lightContainerChild != null)
            {
                lightContainerChild.SetActive(true);
                Debug.Log($"Child '{lightContainerChild.name}' activated.");
            }
        }
    }

    // Call this method to stop this GameObject spinning and deactivate its child lights
    public void StopSpinAndLight()
    {
        if (isSpinning)
        {
            isSpinning = false;
            // Optionally, reset rotation to an initial state or smooth stop
            // transform.localRotation = Quaternion.identity; // If you want to reset rotation on stop
            Debug.Log($"{gameObject.name} stopped spinning.");

            // Deactivate the light container child
            if (lightContainerChild != null)
            {
                lightContainerChild.SetActive(false);
                Debug.Log($"Child '{lightContainerChild.name}' deactivated.");
            }
        }
    }
}
