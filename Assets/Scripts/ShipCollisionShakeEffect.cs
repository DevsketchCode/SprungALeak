using UnityEngine;
using System.Collections; // Required for Coroutines

public class ShipCollisionShakeEffect : MonoBehaviour
{
    [Header("Shake Properties")]
    [Tooltip("How long the shake effect lasts.")]
    public float shakeDuration = 0.5f;
    [Tooltip("How intense the shake is.")]
    public float shakeMagnitude = 0.1f;
    [Tooltip("How quickly the shake dampens over time.")]
    public float dampingSpeed = 1.0f;

    private Vector3 originalCameraLocalPosition; // Stores the camera's local position relative to its parent
    private bool isShaking = false;
    private float initialShakeMagnitude; // Store the initial magnitude

    void Awake()
    {
        // Store the camera's original local position relative to its parent
        originalCameraLocalPosition = transform.localPosition;
        initialShakeMagnitude = shakeMagnitude; // Save the inspector value
    }

    void OnEnable()
    {
        // Re-capture local position in case parent or local offset changes
        originalCameraLocalPosition = transform.localPosition;
        shakeMagnitude = initialShakeMagnitude; // Reset magnitude when enabled
    }

    // Public method to start the shake effect
    public void Shake()
    {
        if (!isShaking)
        {
            // Reset shakeMagnitude to its initial value before starting a new shake
            shakeMagnitude = initialShakeMagnitude;
            StartCoroutine(DoShake());
        }
    }

    private IEnumerator DoShake()
    {
        isShaking = true;
        float elapsed = 0f;

        // Capture current local position for this shake cycle
        Vector3 currentStartLocalPos = transform.localPosition;

        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeMagnitude;
            float y = Random.Range(-1f, 1f) * shakeMagnitude;

            // Apply a dampened shake effect relative to the current starting position
            transform.localPosition = currentStartLocalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime * dampingSpeed;
            // Gradually reduce the magnitude over time
            shakeMagnitude = Mathf.Lerp(initialShakeMagnitude, 0f, elapsed / shakeDuration);

            yield return null; // Wait for the next frame
        }

        transform.localPosition = currentStartLocalPos; // Reset to the position before this shake started
        isShaking = false;
        shakeMagnitude = initialShakeMagnitude; // Ensure magnitude is fully reset for next shake
    }

    // Ensure shake is stopped and camera reset if script is disabled or object destroyed
    void OnDisable()
    {
        if (isShaking)
        {
            StopAllCoroutines();
            transform.localPosition = originalCameraLocalPosition; // Reset to original local position
            isShaking = false;
        }
    }
}
