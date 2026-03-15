// Vision/DarkOverlay
// Full-screen dark quad that renders everywhere the vision cone stencil (ref=1) was NOT written.
// This produces the "fog of war" effect: the world is visible only inside the player's cone.
Shader "Vision/DarkOverlay"
{
    Properties
    {
        _Color ("Overlay Color", Color) = (0, 0, 0, 0.95)
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Transparent+101"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
        }

        // Only render pixels where the vision cone did NOT write stencil.
        Stencil
        {
            Ref  1
            Comp NotEqual
        }

        ZWrite Off
        Blend  SrcAlpha OneMinusSrcAlpha
        Cull   Off

        // URP 2D renderer pass
        Pass
        {
            Name "Universal2D"
            Tags { "LightMode" = "Universal2D" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target { return _Color; }
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

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
            CBUFFER_END

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target { return _Color; }
            ENDHLSL
        }
    }
}
