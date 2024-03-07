﻿Shader "Vivify/Vector/CameraForward"
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
            #include "Assets/VivifyTemplate/Shaders/Includes/Math.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 cameraForward : TEXCOORD0;
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
                o.cameraForward = getCameraForward(); // from Math.cginc

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.cameraForward, 0);
            }
            ENDCG
        }
    }
}
