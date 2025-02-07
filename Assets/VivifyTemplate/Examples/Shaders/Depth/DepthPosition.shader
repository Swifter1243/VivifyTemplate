Shader "Vivify/Depth/DepthPosition"
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
            #include "Assets/VivifyTemplate/Utilities/Shader Functions/Math.cginc"

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
                // Get the position of this fragment on the screen
                float2 screenUV = (i.screenUV) / i.screenUV.w;

                // Gets the value of the depth texture at this fragment
                float depth = UNITY_SAMPLE_SCREENSPACE_TEXTURE(_CameraDepthTexture, screenUV);

                // Convert the depth into a range between 0 to the clipping plane distance
                float eyeDepth = LinearEyeDepth(depth);

                // I'm not too sure on how this works still
                // But I know it fixes descrepancies between the view vector and the projection plane
                float3 viewPlane = unwarpViewVector(i.viewVector);

                // Multiply the forward direction by the depth and add the camera position to get world space
                float3 worldPos = viewPlane * eyeDepth + _WorldSpaceCameraPos;

                // Return the world position if the depth isn't at the far clipping planes
                return worldPos.xyzz * (1 != Linear01Depth(depth));
            }
            ENDCG
        }
    }
}
