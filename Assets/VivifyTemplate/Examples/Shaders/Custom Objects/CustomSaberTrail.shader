Shader "Vivify/CustomObjects/CustomSaberTrail"
{
    Properties
    {
        /*
        The trail color can be passed in 2 ways.
        
        A) As demonstrated in this shader, they can be passed in through the vertex color.
        The issue with this approach is that there is some potentially unwanted desaturation of the colors toward the saber.
        
        B) As demonstrated in the note and saber base shaders, the colors get passed through the instanced _Color property.
        You'll need to add the necessary macros and enable instancing on the material.
        Also remove "color" from the appdata and v2f structs.
        */
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend One OneMinusSrcColor // Blend by brightness
        Cull Off // Make trail two-sided
        ZWrite Off // Don't write to Z-Buffer so that the trails don't block transparent things behind it

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR; // import vertex color (if going with option A)
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 color : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color; // pass vertex color to fragment shader (if going with option A)
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                /*
                The UV value is a little unintuitive for trails, so here's the breakdown:
                i.uv.x: 0 <-- top   bottom --> 1
                i.uv.y: 0 <-- left   right --> 1
                */
                float3 col = i.color; 
                col *= pow(1 - i.uv.y, 7);
                return float4(col, 0);
            }
            ENDCG
        }
    }
}
