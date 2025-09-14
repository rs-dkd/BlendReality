
Shader "Custom/Wireframe"
{
    Properties
    {
        _WireThickness ("Wire Thickness", RANGE(0, 800)) = 100
        _FaceAlpha ("Face Transparency", Range(0, 1)) = 0.0
        _WireColor ("Wire Color", Color) = (1, 1, 1, 1)
        _FaceColor ("Face Color", Color) = (0, 0, 0, 1)
        [Toggle] _ShowBackfaces ("Show Backfaces", Float) = 0
    }

    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            Name "Wireframe Pass"

            HLSLPROGRAM
            #pragma require geometry
            #pragma vertex vert
            #pragma geometry geom
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma shader_feature _SHOWBACKFACES_ON

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            float _WireThickness;
            float _FaceAlpha;
            float4 _WireColor;
            float4 _FaceColor;
            float _ShowBackfaces;

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2g
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2g vert(Attributes input)
            {
                v2g o = (v2g)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                o.projectionSpaceVertex = vertexInput.positionCS;
                o.worldSpacePosition = mul(UNITY_MATRIX_M, input.positionOS);
                return o;
            }

            struct g2f
            {
                float4 projectionSpaceVertex : SV_POSITION;
                float4 worldSpacePosition : TEXCOORD0;
                float4 dist : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            [maxvertexcount(3)]
            void geom(triangle v2g i[3], inout TriangleStream<g2f> triangleStream)
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i[0]);

                float2 p0 = i[0].projectionSpaceVertex.xy / i[0].projectionSpaceVertex.w;
                float2 p1 = i[1].projectionSpaceVertex.xy / i[1].projectionSpaceVertex.w;
                float2 p2 = i[2].projectionSpaceVertex.xy / i[2].projectionSpaceVertex.w;

                float2 edge0 = p2 - p1;
                float2 edge1 = p2 - p0;
                float2 edge2 = p1 - p0;

                float area = abs(edge1.x * edge2.y - edge1.y * edge2.x);
                float wireThickness = 800 - _WireThickness;

                g2f o;
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.worldSpacePosition = i[0].worldSpacePosition;
                o.projectionSpaceVertex = i[0].projectionSpaceVertex;
                o.dist.xyz = float3(area / length(edge0), 0.0, 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);

                o.worldSpacePosition = i[1].worldSpacePosition;
                o.projectionSpaceVertex = i[1].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, area / length(edge1), 0.0) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);

                o.worldSpacePosition = i[2].worldSpacePosition;
                o.projectionSpaceVertex = i[2].projectionSpaceVertex;
                o.dist.xyz = float3(0.0, 0.0, area / length(edge2)) * o.projectionSpaceVertex.w * wireThickness;
                o.dist.w = 1.0 / o.projectionSpaceVertex.w;
                triangleStream.Append(o);
            }


            half4 frag(g2f i, float facing : VFACE) : SV_Target
            {

                if (_ShowBackfaces < 0.5 && facing < 0)
                    discard;

                float minDistanceToEdge = min(i.dist[0], min(i.dist[1], i.dist[2])) * i.dist[3];
                float t = exp2(-2 * minDistanceToEdge * minDistanceToEdge);

                half4 faceColor = half4(_FaceColor.rgb, _FaceAlpha);
                return lerp(faceColor, _WireColor, t);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
