Shader "Custom/Backface Unlit"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)


        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Culling", Float) = 2
    }
    SubShader
    {

        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
   
            Tags { "LightMode"="UniversalForward" }

            Cull [_Cull]

  
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION; 
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION; 
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _Color;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return _Color;
            }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Unlit"
}