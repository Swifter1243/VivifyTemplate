Shader "Vivify/ExampleSkybox"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (1, 1, 1)
        _HorizonColor ("Horizon Color", Color) = (1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Cull Front // Render only the back of triangles

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localPosition : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float3 _BaseColor;
            float3 _HorizonColor;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPosition = v.vertex;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float3 up = float3(0, 1, 0);
                float3 forward = normalize(i.localPosition);

                float3 skyColor = _BaseColor;
                skyColor += saturate(pow(1 - (dot(forward, up)), 4)) * _HorizonColor;
                return float4(skyColor, Luminance(skyColor));
            }
            ENDCG
        }
    }
}
