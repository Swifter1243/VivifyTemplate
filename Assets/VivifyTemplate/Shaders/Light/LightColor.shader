Shader "Vivify/Light/LightColor"
{
    Properties
    {

    }
    SubShader {
        Tags { "RenderType" = "Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert // We are now in funny surface shader land

        struct Input {
            float4 color : COLOR;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o) {
            // I'm intentionally ignoring the directional light here lol
            // Set albedo to 1 if you want contribution from every light
            o.Albedo = _WorldSpaceLightPos0.w;

            o.Alpha = 0; // do this for beat saber since alpha = bloom

            // I'm also fairly certain you need to set _pixelLightCount in quality settings in the map, in order for lights to work
            // but feel free to check this for yourself
        }
        
        ENDCG
    }
    Fallback "Diffuse"
}
