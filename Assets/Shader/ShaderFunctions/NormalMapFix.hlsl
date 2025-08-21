void NormalMapFix_float(
    in float3 BaseNormal,
    in float4 TextureSample,
    in float3 Tangent,
    in float3 Bitangent,
    out float3 FinalNormal)
{
    FinalNormal = normalize(
        (TextureSample.x * Tangent) +
        (TextureSample.y * Bitangent) +
        (TextureSample.z * BaseNormal)
    );
}