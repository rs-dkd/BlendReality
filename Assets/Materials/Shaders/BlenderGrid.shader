Shader "Custom/BlenderGridAdvanced"
{
    Properties
    {
        _GridColor ("Grid Color", Color) = (0.5, 0.5, 0.5, 0.3)
        _BaseColor ("Base Color", Color) = (0.24, 0.24, 0.24, 1)
        _MajorGridColor ("Major Grid Color", Color) = (0.7, 0.7, 0.7, 0.6)
        _GridSize ("Grid Size", Float) = 1.0
        _MajorGridSize ("Major Grid Size", Float) = 10.0
        _LineWidth ("Line Width", Range(0, 1)) = 0.02
        
        _FadeStartDistance ("Fade Start Distance", Float) = 20.0
        _FadeEndDistance ("Fade End Distance", Float) = 30.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing 

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD1; 
                float3 worldPos : TEXCOORD2; 
                UNITY_VERTEX_OUTPUT_STEREO 
            };

            float4 _GridColor;
            float4 _BaseColor;
            float4 _MajorGridColor;
            float _GridSize;
            float _MajorGridSize;
            float _LineWidth;
            
            float _FadeStartDistance;
            float _FadeEndDistance;

            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v); 
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); 

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.localPos = v.vertex.xyz; 
                
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {

                float2 gridCoords = i.worldPos.xz;
                
                // Minor grid
                float2 gridUV = gridCoords / _GridSize;
                float2 grid = abs(frac(gridUV - 0.5) - 0.5) / fwidth(gridUV);
                float minorLine = min(grid.x, grid.y);
                float minorGrid = 1.0 - saturate(minorLine * (1/_LineWidth));
                
                // Major grid
                float2 majorGridUV = gridCoords / _MajorGridSize;
                float2 majorGrid = abs(frac(majorGridUV - 0.5) - 0.5) / fwidth(majorGridUV);
                float majorLine = min(majorGrid.x, majorGrid.y);
                float majorGridLine = 1.0 - saturate(majorLine * (1/_LineWidth));
                
                // Combine grids
                fixed4 col = _BaseColor;
                col = lerp(col, _GridColor, minorGrid * _GridColor.a);
                col = lerp(col, _MajorGridColor, majorGridLine * _MajorGridColor.a);
                col.a = _BaseColor.a;
                col.a = max(col.a, minorGrid * _GridColor.a);
                col.a = max(col.a, majorGridLine * _MajorGridColor.a);
                

                float dist = distance(i.worldPos, _WorldSpaceCameraPos.xyz);
                
                float fadeRange = _FadeEndDistance - _FadeStartDistance;
                float fadeFactor = 1.0 - saturate((dist - _FadeStartDistance) / max(fadeRange, 0.0001));

                // Apply the fade to the final alpha
                col.a *= fadeFactor;

                return col;
            }
            ENDCG
        }
    }
}
