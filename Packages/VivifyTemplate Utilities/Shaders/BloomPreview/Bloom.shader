Shader "Hidden/VivifyTemplate/PostProcessing/Bloom"
{
    Properties
    {
        [HideInInspector] _MainTex("Input Texture", 2D) = "white" {}
        [NoScaleOffset] _BlueNoiseTex("Blue Noise", 2D) = "black" {}

        _BloomGain("Bloom Gain", Float) = 4.0
        _BloomCurve("Bloom Curve", Range(0, 1)) = 1.0
        _BloomBias("Bloom Bias", Range(0, 1)) = 0.0
        _BloomWeight("Bloom Contribution", Range(0, 1)) = 0.3

        [Space(5)]
        _UpsampleScale("Upsample Scale", Range(0, 4)) = 1.357981
        _UpsampleBlend("Upsample Blend", Range(0, 1)) = 0.99
        //_LastBlend("Previous Pass Blend", Range(0, 1)) = 1.0

        _FinalBrightness("Final Brightness", Range(0, 1)) = 1.0
    }
    SubShader
    {
        ZTest Always
        ZWrite Off
        ZClip False

        CGINCLUDE
        #include "UnityCG.cginc"

        UNITY_DECLARE_SCREENSPACE_TEXTURE(_MainTex);
        float4 _MainTex_TexelSize;

        sampler2D _BlueNoiseTex;
        float3 _BlueNoiseParams;

        UNITY_DECLARE_SCREENSPACE_TEXTURE(_LastTex);

        UNITY_DECLARE_SCREENSPACE_TEXTURE(_BloomTex);

        float _BloomGain;
        float _BloomCurve;
        float _BloomBias;
        float _BloomWeight;

        half _UpsampleScale;
        half _UpsampleBlend;
        half _LastBlend;

        half _FinalBrightness;

        struct inputSig
        {
            float3 vertex : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct v2f
        {
            float4 vertex : SV_POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        v2f vert(inputSig i)
        {
            v2f o;

            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            o.vertex = UnityObjectToClipPos(i.vertex);
            o.uv = i.uv;

            return o;
        }
        #include "Sampling.cginc"

        ENDCG

        Pass // 0
        {
            Name "Bloom/Downsample"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ RUNTIME_FETCH

            half3 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 downsample = DownsampleBox13Tap(
                    BS_SCREENSPACE_TEXTURE_PASS(_MainTex),
                    i.uv.xy,
                    _MainTex_TexelSize.xy
                );

            #ifdef RUNTIME_FETCH
                downsample.rgb *= saturate(downsample.a * _BloomGain);
            #endif //RUNTIME_FETCH

                return max(0, downsample.rgb);
            }
            ENDCG
        }
        Pass // 1
        {
            Name "Bloom/Upsample"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half3 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float4 upsample = UpsampleTent(
                    BS_SCREENSPACE_TEXTURE_PASS(_MainTex),
                    i.uv.xy,
                    _MainTex_TexelSize.xy,
                    _UpsampleScale
                );

                float4 last = BS_SAMPLE_SCREENSPACE_TEX(_LastTex, i.uv.xy);
                float4 blend = upsample * _UpsampleBlend + (last * _LastBlend);

                return max(0, blend.rgb);
            }
            ENDCG
        }
        Pass // 2
        {
            Name "Bloom/Composite"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ BLOOM_ONLY

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float2 uv = i.uv.xy;
                float2 mainTS = _MainTex_TexelSize.xy;

                /* Triangle sample pattern:
                 * NOTE: Each offset is half a pixel
                 * D  .  C
                 * .  B  .
                 * .  A  .
                */
                half4 C = BS_SAMPLE_SCREENSPACE_TEX(_MainTex, uv                              );
                half4 B = BS_SAMPLE_SCREENSPACE_TEX(_MainTex, uv + mainTS * float2( 0.0, -0.5));
                half4 R = BS_SAMPLE_SCREENSPACE_TEX(_MainTex, uv + mainTS * float2( 0.5,  0.5));
                half4 L = BS_SAMPLE_SCREENSPACE_TEX(_MainTex, uv + mainTS * float2(-0.5,  0.5));

                // Average the alpha channel over the triangle kernel
                float luma = (C.a + B.a + R.a + L.a) / 4.0;

                // Overlay the bloom onto the framebuffer pixel
                luma = pow(luma, 2) * _BloomCurve - _BloomBias;
                C.rgb = saturate(C.rgb + luma);

                // Calculate the noise UV
                float2 noiseUV = uv + float2(0.1, 0.2);
                noiseUV = noiseUV * _BlueNoiseParams.xy + _BlueNoiseParams.zz;

                // Remap [0 | 1] to [-1/510 | +1/510]
                half noise = tex2D(_BlueNoiseTex, noiseUV).r;
                noise = (noise - 0.5) / 255.0;

                // Overlay the bloom stack from previous calculations
                half3 stack = BS_SAMPLE_SCREENSPACE_TEX(_BloomTex, uv).rgb;

            #ifdef BLOOM_ONLY
                C.rgb  = stack * _BloomWeight;
            #else
                C.rgb += stack * _BloomWeight + noise;
            #endif //BLOOM_ONLY

                half4 finalColor = half4(C.rgb * _FinalBrightness, C.a);

                return max(0, finalColor);
            }
            ENDCG
        }
        Pass // 3
        {
            Name "Debug/AlphaOnly"
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                return BS_SAMPLE_SCREENSPACE_TEX(_MainTex, i.uv.xy).a;
            }
            ENDCG
        }
    }
}