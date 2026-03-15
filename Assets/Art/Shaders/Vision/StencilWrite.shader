// Vision/StencilWrite
// Renders the vision cone mesh invisibly while writing 1 to the stencil buffer.
// The DarkOverlay shader reads this stencil and skips those pixels, revealing the world beneath.
Shader "Vision/StencilWrite"
{
    SubShader
    {
        Tags
        {
            "Queue"          = "Transparent+100"
            "RenderType"     = "Transparent"
            "IgnoreProjector" = "True"
        }

        // Write 1 to the stencil wherever this mesh covers.
        Stencil
        {
            Ref  1
            Comp Always
            Pass Replace
        }

        // Invisible — only stencil output matters.
        ColorMask 0
        ZWrite    Off
        Cull      Off

        // URP 2D renderer pass
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }

        // URP 3D forward renderer pass (fallback)
        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
