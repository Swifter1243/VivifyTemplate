Shader "Vivify/CustomObjects/CustomSaberTrail"
{
    Properties
    {
        /*
        The trail color can be passed in 2 ways. Remove lines from the option you aren't using.
        
        A) As demonstrated in this shader, they can be passed in through the vertex color.
        The issue with this approach is that there is some potentially unwanted desaturation of the colors toward the saber.
        
        B) As demonstrated in the note and saber base shaders, the colors get passed through the instanced "_Color" property.
        You'll need to add the necessary macros and enable instancing on the material for GPU instancing.
       
        */
        _Color ("Saber Color", Color) = (1,1,1) // Option B
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
            #pragma multi_compile_instancing // Insert for GPU instancing (if going with option B)
            // Ensure to check "Enable GPU Instancing" on the material

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 color : COLOR; // Import vertex color (if going with option A)
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float3 color : TEXCOORD1; // Pass vertex color (if going with option A)
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Insert for GPU instancing (if going with option B)
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata v)
            {
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // Insert for GPU instancing (if going with option B)
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.color = v.color; // pass vertex color to fragment shader (if going with option A)
                o.uv = v.uv;
                return o;
            }

            // Register GPU instanced _Color (apply per-saber) (if going with option B)
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float3, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // Insert for GPU instancing (if going with option B)

                // The color of the saber (if going with option B)
                float3 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
                
                //float3 col = i.color; // option A
                float3 col = Color; // option B

                /*
                The UV value is a little unintuitive for trails, so here's the breakdown:
                i.uv.x: 0 <-- top                             bottom --> 1
                i.uv.y: 0 <-- closest to saber   furthest from saber --> 1
                */
                
                col *= pow(1 - i.uv.y, 7); // brighter to the left and fall off quickly
                return float4(col, 0);
            }
            ENDCG
        }
    }
}
