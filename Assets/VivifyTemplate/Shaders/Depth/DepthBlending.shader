Shader "VivifyTemplate/DepthBlending"
{
    Properties
    {
        _DepthFade ("Depth Fade", Float) = 2
    }
    SubShader
    {
        // Render this material after opaque geometry
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Blend One OneMinusSrcColor
        // You could read this as:
        /*
        float3 existingPixel = <the existing pixel>
        float3 sourcePixel = <output of our fragment shader>

        >>            (One)               (OneMinusSrcColor)
        float3 output = 1 * sourcePixel + (1 - sourcePixel) * existingPixel;
        */

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

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float4 screenUV : TEXCOORD3;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            float _DepthFade;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);

                // Screen UV
                o.screenUV = ComputeGrabScreenPos(o.vertex);

                // World Position
                o.worldPos = localToWorld(v.vertex); // from Math.cginc
                
                return o;
            }

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_CameraDepthTexture);

            fixed4 frag (v2f i) : SV_Target
            {
                float2 screenUV = (i.screenUV) / i.screenUV.w;
                float farZ = _ProjectionParams.z;
                float depth = Linear01Depth(UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, screenUV)) * farZ;
                float viewLength = -UnityWorldToViewPos(i.worldPos).z;
                float depthFade = saturate((depth - viewLength) / _DepthFade);
                return depthFade;
            }
            ENDCG
        }
    }
}
