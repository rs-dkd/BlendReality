Shader "Custom/ZebraStripes"
{
    Properties
    {
        [Header(Colors)]
        _StripeColor1 ("Stripe Color 1", Color) = (0.8, 0.8, 0.8, 1)
        _StripeColor2 ("Stripe Color 2", Color) = (0.1, 0.1, 0.1, 1)

        [Header(Settings)]
        _StripeCount ("Stripe Count", Float) = 20
        _StripeSharpness ("Sharpness", Range(0, 1)) = 0.95
        
        [Header(Transform)]
        _Rotation ("Rotation (Degrees)", Float) = 0
        _Scale ("Scale", Float) = 1.0
        _AnimationOffset ("Animation Offset", Float) = 0
        
        [Header(Config)]
        [Enum(Vertical, 0, Horizontal, 1, Diagonal45, 2, Diagonal135, 3, Radial, 4)] _Direction ("Direction", Int) = 0
        [Toggle] _UseWorldSpace ("Use Reflection (World)", Int) = 1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 100

        Pass
        {
            Name "ZebraAnalysis"
            Tags { "LightMode"="UniversalForward" }

            Cull Off
            
            ZWrite On 
            
            ZTest LEqual 
            

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _StripeColor1;
                float4 _StripeColor2;
                float _StripeCount;
                float _StripeSharpness;
                float _Rotation;
                float _Scale;
                float _AnimationOffset;
                int _Direction;
                int _UseWorldSpace;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, float4(0,0,0,0));

                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = normalInput.normalWS;
                output.uv = input.uv;

                return output;
            }

            float2 Rotate2D(float2 coord, float angleDeg)
            {
                float rad = angleDeg * PI / 180.0;
                float s, c;
                sincos(rad, s, c);
                return float2(coord.x * c - coord.y * s, coord.x * s + coord.y * c);
            }

            half4 frag(Varyings input, float facing : VFACE) : SV_Target
            {
                float3 normalWS = normalize(input.normalWS);
                normalWS *= (facing > 0) ? 1.0 : -1.0;

                float2 mappingCoords;

                if (_UseWorldSpace == 1)
                {
                    float3 viewDirWS = GetWorldSpaceNormalizeViewDir(input.positionWS);
                    float3 reflectionDir = reflect(-viewDirWS, normalWS);
                    mappingCoords = reflectionDir.xy * 0.5 + 0.5;
                }
                else
                {
                    mappingCoords = input.uv;
                }

                mappingCoords *= _Scale;
                mappingCoords -= 0.5;
                mappingCoords = Rotate2D(mappingCoords, _Rotation);
                mappingCoords += 0.5;

                float val = 0;
                if (_Direction == 0) val = mappingCoords.x;
                else if (_Direction == 1) val = mappingCoords.y;
                else if (_Direction == 2) val = (mappingCoords.x + mappingCoords.y) * 0.5;
                else if (_Direction == 3) val = (mappingCoords.x - mappingCoords.y) * 0.5;
                else if (_Direction == 4) val = length(mappingCoords); 

                float t = (val * _StripeCount + _AnimationOffset) * PI * 2;
                float sineVal = cos(t);
                float pattern = sineVal * 0.5 + 0.5;
                float deriv = fwidth(pattern);
                float softness = (1.0 - _StripeSharpness); 
                float edgeWidth = max(softness, deriv);
                float stripeMask = smoothstep(0.5 - edgeWidth, 0.5 + edgeWidth, pattern);

                return lerp(_StripeColor2, _StripeColor1, stripeMask);
            }
            ENDHLSL
        }
    }
}