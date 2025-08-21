#ifndef VIVIFY_COLOR_FUNCTIONS_INCLUDED
#define VIVIFY_COLOR_FUNCTIONS_INCLUDED

float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
{
    return a + b*cos( 6.28318*(c*t+d) );
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

// Hue shift by transforming rgb to a chroma rotatable domain (YIQ), rotate chroma, then convert back to rgb
// percent: 0 would be 0% hue shift, 1 would be 360 degree rotation
float3 hueShift( float3 color, float percent) {
    // https://en.wikipedia.org/wiki/YIQ#From_RGB_to_YIQ
    const float3x3 rgb_to_yiq = float3x3(
        0.2990,  0.5870,  0.1140,
        0.5959, -0.2746, -0.3213,
        0.2115, -0.5227,  0.3112
    );
    // https://en.wikipedia.org/wiki/YIQ#From_YIQ_to_RGB
    const float3x3 yiq_to_rgb = float3x3(
        1.0,  0.956,  0.619,
        1.0, -0.272, -0.647,
        1.0, -1.106,  1.703
    );

    // Scale the angle from 0-1 to 0-2pi
    // which is the cycle length of sin and cos
    const float tau = 6.28318530718;
    float theta = percent * tau;

    // Convert the colour to the yiq colour system
    float3 yiq = mul(rgb_to_yiq, color);

    // Rotate around the x axis (IQ plane)
    // This rotates the chrominance plane / hue shifts
    float s = sin(theta), c = cos(theta);
    float3x3 rotor = float3x3(
        1, 0,  0,
        0, c, -s,
        0, s,  c
    );
    float3 yiq_new = mul(rotor, yiq);

    // Convert back to RGB
    return mul(yiq_to_rgb, yiq_new);
}

#endif //VIVIFY_COLOR_FUNCTIONS_INCLUDED
