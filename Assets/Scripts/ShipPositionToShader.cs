using UnityEngine;

public class ShipPositionToShader : MonoBehaviour
{
    public Material waterMaterial;
    public float shipWidth = 5f;
    public float shipLength = 10f;
    public int renderQueue = 3001; // The Render Queue value

    void Update()
    {
        // Pass the ship's world position to the shader.
        waterMaterial.SetVector("_ShipPosition", new Vector4(transform.position.x, 0, transform.position.z, 0));

        // Pass the ship's size for the clipping box.
        waterMaterial.SetFloat("_ShipWidth", shipWidth);
        waterMaterial.SetFloat("_ShipLength", shipLength);

        // Set the material's render queue to make the water transparent
        waterMaterial.renderQueue = renderQueue;
    }
}