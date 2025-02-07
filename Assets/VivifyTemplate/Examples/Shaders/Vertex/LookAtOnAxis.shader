Shader "Vivify/Vertex/LookAtOnAxis"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/VivifyTemplate/Utilities/Shader Functions/Math.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 normal : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Rotate Vertices
                float3 forward = directionToCamera(); // from Math.cginc
                float3 up = float3(0,1,0);
                v.vertex = rotateLookOnAxis(forward, up, v.vertex).xyzz; // from Math.cginc

                // Output Rotated Vertex
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Normal
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.normal = rotateLookOnAxis(forward, up, v.normal); // from Math.cginc

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 lightDirection = normalize(float3(-10,3,1));
                float d = dot(lightDirection, i.normal);
                return d * 0.5 + 0.5;
            }
            ENDCG
        }
    }
}
