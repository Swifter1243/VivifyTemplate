Shader "Vivify/Grab Pass/Distort"
{
    Properties
    {

    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
            "Queue"="Transparent"
        }

        GrabPass { "_GrabTexture1" } // Base game also uses this grab pass texture name so it's best to reuse it

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Assets/VivifyTemplate/Utilities/Shader Functions/Noise.cginc"

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
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Clip position
                o.vertex = UnityObjectToClipPos(v.vertex);

                // UV
                o.uv = v.uv;

                // screenUV
                o.screenUV = ComputeGrabScreenPos(o.vertex);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Get the position of this fragment on the screen
                float4 screenUV = (i.screenUV) / i.screenUV.w;

                // Distorts based on object UV
                float2 uv = i.uv;
                float noise = cos(simplex(uv * 3) * 6 + _Time.y * 0.6);
                screenUV += noise * 0.04;

                // Get screen color
                float4 screenCol = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabTexture1, screenUV);

                // Add border
                float borderX = max(pow(1 - i.uv.x, 40), pow(i.uv.x, 40));
                float borderY = max(pow(1 - i.uv.y, 40), pow(i.uv.y, 40));
                screenCol += (borderX + borderY) * 0.7;

                // Add some color for the noise
                screenCol += (noise * 0.5 + 0.5) * 0.1;

                return screenCol;
            }
            ENDCG
        }
    }
}
