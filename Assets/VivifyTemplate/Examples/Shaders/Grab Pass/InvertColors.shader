Shader "Vivify/Grab Pass/InvertColors"
{
    Properties
    {

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        GrabPass { "_GrabPass" }

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 screenUV : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_GrabPass);

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
                // Get the position of this fragment on the screen
                float2 screenUV = (i.screenUV) / i.screenUV.w;

                // Get screen color
                float4 screenCol = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_GrabPass, screenUV);

                // Returns inverted screen color
                return 1 - screenCol;
            }
            ENDCG
        }
    }
}
