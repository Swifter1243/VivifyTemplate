float3 palette( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
{
    return a + b*cos( 6.28318*(c*t+d) );
}

float3 rainbow( in float t)
{
    return palette(t, 0.5, 0.5, 1, float3(0, 0.33, 0.66));
}

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
    float4 P = (RGB.g < RGB.b) ? float4(RGB.bg, -1.0, 2.0/3.0) : float4(RGB.gb, 0.0, -1.0/3.0);
    float4 Q = (RGB.r < P.x) ? float4(P.xyw, RGB.r) : float4(RGB.r, P.yzx);
    float C = Q.x - min(Q.w, Q.y);
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

// https://gist.github.com/mairod/a75e7b44f68110e1576d77419d608786
float3 hueShift( float3 color, float hueAdjust ){
    hueAdjust *= UNITY_PI * 2;

    const float3  kRGBToYPrime = float3 (0.299, 0.587, 0.114);
    const float3  kRGBToI      = float3 (0.596, -0.275, -0.321);
    const float3  kRGBToQ      = float3 (0.212, -0.523, 0.311);

    const float3  kYIQToR     = float3 (1.0, 0.956, 0.621);
    const float3  kYIQToG     = float3 (1.0, -0.272, -0.647);
    const float3  kYIQToB     = float3 (1.0, -1.107, 1.704);

    float   YPrime  = dot (color, kRGBToYPrime);
    float   I       = dot (color, kRGBToI);
    float   Q       = dot (color, kRGBToQ);
    float   hue     = atan2 (Q, I);
    float   chroma  = sqrt (I * I + Q * Q);

    hue += hueAdjust;

    Q = chroma * sin (hue);
    I = chroma * cos (hue);

    float3    yIQ   = float3 (YPrime, I, Q);

    return float3( dot (yIQ, kYIQToR), dot (yIQ, kYIQToG), dot (yIQ, kYIQToB) );

}