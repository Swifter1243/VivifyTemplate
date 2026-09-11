#ifndef VIVIFY_COLOR_FUNCTIONS_INCLUDED
#define VIVIFY_COLOR_FUNCTIONS_INCLUDED

float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
{
    const float tau = 6.28318530718;
    return a + b*cos( tau*(c*t+d) );
}

float3 rainbow( in float t)
{
    return palette(t, 0.5, 0.5, 1, float3(0, 0.33, 0.66));
}

// Linear to gamma conversion
float3 gammaCorrect( in float3 col)
{
    return pow(saturate(col), 2.2);
}

// https://www.chilliant.com/rgb2hsv.html
float3 HUEtoRGB(in float H)
{
    float R = abs(H * 6 - 3) - 1;
    float G = 2 - abs(H * 6 - 2);
    float B = 2 - abs(H * 6 - 4);
    return saturate(float3(R,G,B));
}

float Epsilon = 1e-10;

float3 RGBtoHCV(in float3 RGB)
{
    // Based on work by Sam Hocevar and Emil Persson
    float4 P = float4( max(RGB.g, RGB.b), min(RGB.g, RGB.b), -(RGB.g < RGB.b), (RGB.g < RGB.b) - (1.0/3.0) );
    float4 Q = float4( max(RGB.r, P.x), P.y, (RGB.r < P.x) ? P.w : P.z, min(RGB.r, P.x) );
    float C = max(Q.x - Q.w, Q.x - Q.y);
    float H = abs((Q.w - Q.y) / (6 * C + Epsilon) + Q.z);
    return float3(H, C, Q.x);
}

float3 HSVtoRGB(in float3 HSV)
{
    float3 RGB = HUEtoRGB(HSV.x);
    return ((RGB - 1) * HSV.y + 1) * HSV.z;
}

float3 RGBtoHSV(in float3 RGB)
{
    float3 HCV = RGBtoHCV(RGB);
    float S = HCV.y / (HCV.z + Epsilon);
    return float3(HCV.x, S, HCV.z);
}

float3 HSVLerp(float3 col1, float3 col2, float t)
{
    col1 = RGBtoHSV(col1);
    col2 = RGBtoHSV(col2);
    return HSVtoRGB(lerp(col1, col2, t));
}

// Hue shift by rotating the white-point inline with the Z-axis, rotating around Z, then reverting the orientation.
// percent: 0 would be 0% hue shift, 1 would be 360 degree rotation
float3 hueShift(float3 color, float percent)
{
    const float PI = 3.14159265359;
    const float3 OFFSETS = PI * float3(0.0/3.0, 2.0/3.0, 4.0/3.0);
    float3 ct = (1.0/3.0) + (2.0/3.0) * cos(percent * (2*PI) + OFFSETS);
    return (color.rgb * ct.x) + (color.gbr * ct.y) + (color.brg * ct.z);
}

#endif //VIVIFY_COLOR_FUNCTIONS_INCLUDED
