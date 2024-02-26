Shader "VivifyTemplate/DepthPosition"
{
    Properties
    {

    }
    SubShader
    {
        // Render this material after opaque geometry
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        // Doesn't write to the depth texture
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "UnityCG.cginc"
            #include "../Includes/Math.cginc" // If you move this shader, update this
            #include "../Includes/Noise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 viewVector : TEXCOORD1;
                float4 screenUV : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Screen UV
                o.screenUV = ComputeGrabScreenPos(o.vertex);

                // World Position
                o.viewVector = viewVectorFromLocal(v.vertex); // from Math.cginc
                
                return o;
            }

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = (i.screenUV) / i.screenUV.w;
                float rawDepth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, screenUV);
                float depth = LinearEyeDepth(rawDepth);

                float3 viewPlane = unwarpViewVector(i.viewVector);
                float3 worldPos = viewPlane * depth + _WorldSpaceCameraPos;

                return worldPos.xyzz * (1 != Linear01Depth(rawDepth));
            }
            ENDCG
        }
    }
}
