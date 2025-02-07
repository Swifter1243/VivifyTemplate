Shader "Vivify/CustomObjects/CustomBomb"
{
    Properties
    {
        _CutoutEdgeWidth("Cutout Edge Width", Range(0,0.1)) = 0.02

        /*
        These are fed in by Vivify per note.
        In fact, Vivify will attempt to feed these values into every child of a bomb prefab.
        */
        _Color ("Bomb Color", Color) = (1,1,1)
        _Cutout ("Cutout", Range(0,1)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // Insert for GPU instancing
            // Ensure to check "Enable GPU Instancing" on the material

            #include "UnityCG.cginc"
            #include "Assets/VivifyTemplate/Utilities/Shader Functions/Noise.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 localPos : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID // Insert for GPU instancing
                UNITY_VERTEX_OUTPUT_STEREO
            };

            // Register GPU instanced properties (apply per-bomb)
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float, _Cutout)
            UNITY_DEFINE_INSTANCED_PROP(float3, _Color)
            UNITY_INSTANCING_BUFFER_END(Props)

            // Register regular properties (apply to every bomb)
            float _CutoutEdgeWidth;

            v2f vert (appdata v)
            {
                UNITY_INITIALIZE_OUTPUT(v2f, v2f o);
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o); // Insert for GPU instancing
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.localPos = v.vertex;

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i); // Insert for GPU instancing

                // Cutout represents dissolve
                // 0 = visible, 1 = dissolved
                float Cutout = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);

                // The color of the bomb
                float3 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                // Calculate 3D simplex noise based on the fragment position
                float noise = simplex(i.localPos * 2);

                // Use cutout to lower the values of the noise into the negatives, clipping them
                float c = noise - Cutout;

                // Negative values of c will discard the pixel
                clip(c);

                // Positive values of c close to zero will return a border color (white)
                if (c < _CutoutEdgeWidth) {
                    return 1;
                }

                // Return some basic shading with bomb color
                float lighting = pow(i.localPos.y + 0.8, 8);
                float3 col = lighting * 0.4 * Color;
                return float4(col, 0);
            }
            ENDCG
        }
    }
}
