﻿Shader "Hidden/Vivify/Templates/Blit"
{
    Properties
    {

    }
    SubShader
    {
        Tags {
            "RenderType" = "Opaque"
            "Queue" = "Transparent"
        }
        GrabPass { "_GrabTexture1" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            // VivifyTemplate Libraries
            // #include "Assets/VivifyTemplate/CGIncludes/Noise.cginc"
            // #include "Assets/VivifyTemplate/CGIncludes/Colors.cginc"
            // #include "Assets/VivifyTemplate/CGIncludes/Math.cginc"
            // #include "Assets/VivifyTemplate/CGIncludes/Easings.cginc"

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
                float4 screenUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_GrabTexture1);

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o)

                o.vertex = UnityObjectToClipPos(v.vertex);

                o.uv = v.uv;

                o.screenUV = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float2 screenUV = (i.screenUV) / i.screenUV.w;

                float4 col = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture1, screenUV);

                return col;
            }
            ENDCG
        }
    }
}
