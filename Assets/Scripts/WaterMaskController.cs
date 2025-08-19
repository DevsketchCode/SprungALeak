using UnityEngine;

public class WaterMaskController : MonoBehaviour
{
    public Material waterMaterial;
    public RenderTexture waterMaskRT;

    void Start()
    {
        if (waterMaterial == null || waterMaskRT == null)
        {
            Debug.LogError("WaterMaskController is missing required components. Disabling script.");
            this.enabled = false;
            return;
        }

        // Pass the render texture to the material once.
        waterMaterial.SetTexture("_MaskTexture", waterMaskRT);
    }
}