Shader "Vivify/CustomObjects/CustomNote"
{
    Properties
    {
        [Toggle(DEBRIS)] _Debris ("Debris", Int) = 0
        _CutoutEdgeWidth("Cutout Edge Width", Range(0,0.1)) = 0.02

        /*
        These are fed in by Vivify per note.
        In fact, Vivify will attempt to feed these values into every child of a note prefab.
        */
        _Color ("Note Color", Color) = (1,1,1)
        _Cutout ("Cutout", Range(0,1)) = 1
        _CutPlane ("Cut Plane", Vector) = (0, 0, 1, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        /*
        The model you're using should have 2-sided normals in order to get a hollow note inside.
        Blender typically exports like this.
        */
        Cull Off

        Pass
        {

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing // Insert for GPU instancing
            // Ensure to check "Enable GPU Instancing" on the material
            #pragma shader_feature DEBRIS

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

            // Register GPU instanced properties (apply per-note)
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float3, _Color)
            UNITY_DEFINE_INSTANCED_PROP(float, _Cutout)
            UNITY_DEFINE_INSTANCED_PROP(float4, _CutPlane)
            UNITY_INSTANCING_BUFFER_END(Props)

            // Register regular properties (apply to every note)
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

                // Cutout works differently depending on if this note is used as debris:
                // - using debris -> 0 = just hit, 1 = dissolved away
                // - not using debris (driven by "dissolve") -> 0 = visible, 1 = dissolved
                float Cutout = UNITY_ACCESS_INSTANCED_PROP(Props, _Cutout);

                // The color of the note
                float3 Color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);

                // This encodes the slice "plane" that the player hit the note through
                // - (x, y, z) -> plane normal
                // - (w) -> plane offset along it's normal
                float4 CutPlane = UNITY_ACCESS_INSTANCED_PROP(Props, _CutPlane);

                // This "c" value will quantify the note's visibility, where negatives are invisible
                float c = 0;

                #if DEBRIS
                    // Shift our local position along the slice normal by the cut offset
                    float3 samplePoint = i.localPos + CutPlane.xyz * CutPlane.w;

                    // Calculate the signed distance of our point to the cut plane
                    float planeDistance = dot(samplePoint, CutPlane.xyz) / length(CutPlane.xyz);

                    /*
                    This sets the visibility of our pixel based on it's signed distance to the plane
                    Negative values (points behind the plane) will not be visible
                    Cutout acts as an offset to this visibility so that the plane appears to consume the debris
                    */
                    c = planeDistance - Cutout * 0.25;
                #else
                    // Calculate 3D simplex noise based on the fragment position
                    float noise = simplex(i.localPos * 2);

                    // Use cutout to lower the values of the noise into the negatives, clipping them
                    c = noise - Cutout;
                #endif

                // Negative values of c will discard the pixel
                clip(c);

                // Positive values of c close to zero will return a border color (white)
                if (c < _CutoutEdgeWidth) {
                    return 1;
                }

                // Return some basic shading based on the note color
                float lighting = pow(i.localPos.y + 0.8, 4);
                return float4(Color * lighting, 0);
            }
            ENDCG
        }
    }
}
