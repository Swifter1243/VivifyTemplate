Shader "Vivify/Opacity/BrightnessTransparency"
{
    Properties
    {

    }
    SubShader
    {
        // Renders this material after opaque geometry
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Blend One OneMinusSrcColor
        // You could read this as:
        /*
        float3 existingPixel = <the existing pixel>
        float3 sourcePixel = <output of our fragment shader>

        >>            (One)               (OneMinusSrcColor)
        float3 output = 1 * sourcePixel + (1 - sourcePixel) * existingPixel;
        */

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

            float3 _BaseColor;
            float3 _HorizonColor;

            v2f vert (appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return float4(i.uv.xxx, 0);
            }
            ENDCG
        }
    }
}
