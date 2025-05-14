Shader "Skybox/Solid Color" {
Properties {
    _Color ("Color", Color) = (0.5, 0.5, 0.5, 1)
}
SubShader {
    Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
    Cull Off ZWrite Off
    
    Pass {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        
        fixed4 _Color;
        
        struct appdata {
            float4 vertex : POSITION;
        };
        
        struct v2f {
            float4 vertex : SV_POSITION;
        };
        
        v2f vert (appdata v) {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            return o;
        }
        
        fixed4 frag (v2f i) : SV_Target {
            return _Color;
        }
        ENDCG
    }
}
}