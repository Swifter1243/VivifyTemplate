Shader "Hidden/Vivify/Templates/BareBones"
{
    Properties
    {

    }
    SubShader
    {
        Tags {
            "RenderType"="Opaque"
        }

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
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return 1;
            }
            ENDCG
        }
    }
}
