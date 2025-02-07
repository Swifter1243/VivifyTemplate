Shader "Vivify/Vertex/Rotation"
{
    Properties
    {
        _Rotation ("Rotation", Vector) = (0,0,0,0)
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

            float3 _Rotation;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Rotate Vertices
                _Rotation += float3(_Time.y, _Time.y, _Time.y);
                v.vertex = rotatePoint(_Rotation, v.vertex.xyz).xyzz; // from Math.cginc

                // Output Rotated Vertex
                o.vertex = UnityObjectToClipPos(v.vertex);

                // Normal
                o.normal = UnityObjectToWorldNormal(v.normal);
                o.normal = rotatePoint(_Rotation, v.normal); // from Math.cginc

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
