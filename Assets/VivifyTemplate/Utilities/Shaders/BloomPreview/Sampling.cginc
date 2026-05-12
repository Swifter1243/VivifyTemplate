#pragma once

#if defined(UNITY_STEREO_INSTANCING_ENABLED) || defined(UNITY_STEREO_MULTIVIEW_ENABLED)
    #define BS_SCREENSPACE_TEXTURE_ARGS UNITY_ARGS_TEX2DARRAY
    #define BS_SCREENSPACE_TEXTURE_PASS UNITY_PASS_TEX2DARRAY
#else
    #define BS_SCREENSPACE_TEXTURE_ARGS(tex) sampler2D_float tex
    #define BS_SCREENSPACE_TEXTURE_PASS(tex) tex
#endif

#define BS_SAMPLE_SCREENSPACE_TEX(tex, uv) UNITY_SAMPLE_SCREENSPACE_TEXTURE(tex, UnityStereoTransformScreenSpaceTex(uv))

// https://github.com/Unity-Technologies/Graphics/blob/7ae5ef9ae60f5267e7457b836b28e4e9e3ba285d/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl#L43
half4 DownsampleBox4Tap(BS_SCREENSPACE_TEXTURE_ARGS(tex), float2 uv, float2 texelSize)
{
    float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);

    half4 s;
    s  = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xw);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zw);

    return s * (1.0 / 4.0);
}

// Better, temporally stable box filtering
// https://github.com/Unity-Technologies/Graphics/blob/7ae5ef9ae60f5267e7457b836b28e4e9e3ba285d/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl#L15
half4 DownsampleBox13Tap(BS_SCREENSPACE_TEXTURE_ARGS(tex), float2 uv, float2 texelSize)
{
    half4 A = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2(-1.0, -1.0));
    half4 B = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 0.0, -1.0));
    half4 C = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 1.0, -1.0));
    half4 D = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2(-0.5, -0.5));
    half4 E = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 0.5, -0.5));
    half4 F = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2(-1.0,  0.0));
    half4 G = BS_SAMPLE_SCREENSPACE_TEX(tex, uv                                 );
    half4 H = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 1.0,  0.0));
    half4 I = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2(-0.5,  0.5));
    half4 J = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 0.5,  0.5));
    half4 K = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2(-1.0,  1.0));
    half4 L = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 0.0,  1.0));
    half4 M = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + texelSize * float2( 1.0,  1.0));

    half2 div = (1.0 / 4.0) * half2(0.5, 0.125);

    half4 o = (D + E + I + J) * div.x;
    o += (A + B + G + F) * div.y;
    o += (B + C + H + G) * div.y;
    o += (F + G + L + K) * div.y;
    o += (G + H + M + L) * div.y;

    return o;
}

// 9-tap bilinear upsampler (tent filter)
// https://github.com/Unity-Technologies/Graphics/blob/7ae5ef9ae60f5267e7457b836b28e4e9e3ba285d/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl#L57
half4 UpsampleTent(BS_SCREENSPACE_TEXTURE_ARGS(tex), float2 uv, float2 texelSize, float4 sampleScale)
{
    float4 d = texelSize.xyxy * float4(1.0, 1.0, -1.0, 0.0) * sampleScale;

    half4 s;
    s  = BS_SAMPLE_SCREENSPACE_TEX(tex, uv - d.xy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv - d.wy) * 2;
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv - d.zy);

    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zw) * 2;
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv       ) * 4;
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xw) * 2;

    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.wy) * 2;
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xy);

    return s * (1.0 / 16.0);
}

// Standard box filtering
// https://github.com/Unity-Technologies/Graphics/blob/7ae5ef9ae60f5267e7457b836b28e4e9e3ba285d/com.unity.postprocessing/PostProcessing/Shaders/Sampling.hlsl#L78
half4 UpsampleBox(BS_SCREENSPACE_TEXTURE_ARGS(tex), float2 uv, float2 texelSize, float4 sampleScale)
{
    float4 d = texelSize.xyxy * float4(-0.5, -0.5, 0.5, 0.5) * sampleScale;

    half4 s;
    s  = BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zy);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.xw);
    s += BS_SAMPLE_SCREENSPACE_TEX(tex, uv + d.zw);

    return s * (1.0 / 4.0);
}

// Krzysztof Narkowicz's ACES approximation
float3 ACESFilm(float3 x)
{
    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;
    return saturate( (x*(x*a+b)) / ((x*c+d)*x+e) );
}
float4 ACESFilm(float4 x)
{
    return float4(ACESFilm(x.rgb), x.a);
}

half getBrightness(half3 color)
{
    return dot(color, half3(0.3, 0.59, 0.11));
}