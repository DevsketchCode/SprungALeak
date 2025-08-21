Shader "Custom/StencilMask"
{
    Properties
    {
        [IntRange] _StencilID ("Stencil ID", Range(0, 255)) = 1
    }
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        
        Pass
        {
            Blend Zero One // Still don't write color directly
            ColorMask 0    // Still don't write color directly
            ZWrite On      // <--- IMPORTANT CHANGE: Write to the depth buffer
            ZTest LEqual   // Keep this as it is

            // This is the key part: setting up the Stencil Buffer
            Stencil
            {
                Ref [_StencilID]
                Comp Always
                Pass Replace
            }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                return OUT;
            }

            float4 frag(Varyings IN) : SV_Target
            {
                // This shader does not output any color
                return 0;
            }
            ENDHLSL
        }
    }
}