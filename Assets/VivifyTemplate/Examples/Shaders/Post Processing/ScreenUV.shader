Shader "Vivify/Post Processing/ScreenUV"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

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
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                // Screenspace textures are setup differently in Single Pass (2019) vs Single Pass Instanced (2021).
                // Single Pass works such that the texture for each eye is one big texture stretching across both of them
                // Single Pass Instanced works such that the texture for each eye is individual
                // "i.uv" in a post processing shader will always be 0-1 for each eye.
                // so this transformation is done so that when sampling textures in Single Pass, the UV is continuous across both eyes
                float2 uv = UnityStereoTransformScreenSpaceTex(i.uv);

                return float4(uv, 0, 0);
            }
            ENDCG
        }
    }
}
