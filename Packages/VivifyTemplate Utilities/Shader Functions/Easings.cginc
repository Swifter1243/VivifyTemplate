#ifndef VIVIFY_EASING_FUNCTIONS_INCLUDED
#define VIVIFY_EASING_FUNCTIONS_INCLUDED
// Easing functions from https://easings.net/ aggressively optimized for shaders.
// All functions are branchless, continuous (no jumping), f(0) = 0, f(1) = 1, and unless specified, 0 <= y <= 1.
// A direct comparison is given in the form of a desmos graph lol https://www.desmos.com/calculator/ywcku5k4mp

float easeInSine(float x) {
    const float hpi = 1.57079632679;
    return 1 - cos(hpi * x);
}

float easeOutSine(float x) {
    const float hpi = 1.57079632679;
    return sin(hpi * x);
}

float easeInOutSine(float x) {
    const float hpi = 1.57079632679;
    return pow(sin(hpi * x), 2);
}

float easeInQuad(float x) {
    return x * x;
}

float easeOutQuad(float x) {
    return (2 - x) * x;
}

float easeInOutQuad(float x) {
    x -= 0.5;
    return (x - x * abs(x)) * 2 + 0.5;
}

float easeInCubic(float x) {
    return x * x * x;
}

float easeOutCubic(float x) {
    return x * ((x - 3) * x + 3);
}

float easeInOutCubic(float x) {
    float f;
    x -= 0.5;
    f = 4 * abs(x) - 6;
    f = f * abs(x) + 3;
    return f * x + 0.5;
}

float easeInQuart(float x) {
    return x * x * x * x;
}

float easeOutQuart(float x) {
    return 1 - pow(1 - x, 4);
}

float easeInOutQuart(float x) {
    float f;
    x -= 0.5;
    f = -8 * abs(x) + 16;
    f =  f * abs(x) - 12;
    f =  f * abs(x) + 4;
    return f * x + 0.5;
}

float easeInQuint(float x) {
    return x * x * x * x * x;
}

float easeOutQuint(float x) {
    return 1 - pow(1 - x, 5);
}

float easeInOutQuint(float x) {
    float f;
    x -= 0.5;
    f = 16 * abs(x) - 40;
    f =  f * abs(x) + 40;
    f =  f * abs(x) - 20;
    f =  f * abs(x) + 5;
    return f * x + 0.5;
}

float easeInExpo(float x) {
    const float S = 1.0 / 1023.0;
    return exp2(10 * x) * S - S;
}

float easeOutExpo(float x) {
    const float S = 1024.0 / 1023.0;
    return -S * exp2(-10 * x) + S;
}

float easeInOutExpo(float x) {
    x = x * 20 - 10;
    const float S = 512.0 / 1023.0;
    float m = 1 - 2 * float(x < abs(x)); //sign(x)
    return (S - S * exp2(-abs(x))) * m + 0.5;
}

float easeInCirc(float x) {
    return 1 - sqrt(1 - pow(x, 2));
}

float easeOutCirc(float x) {
    return sqrt((2 - x) * x);
}

float easeInOutCirc(float x) {
    x -= 0.5;
    float m = 1 - 2 * float(x < abs(x)); //sign(x)
    return sqrt(abs(x) - abs(x) * abs(x)) * m + 0.5;
}

// RANGE: -0.100004 <= y <= 1 (0.419897, -0.100004)
float easeInBack(float x) {
    return x * x * (2.70158 * x - 1.70158);
}

// RANGE: 0 <= y <= 1.100004 (0.580103, 1.100004)
float easeOutBack(float x) {
    return x * ((2.70158 * x - 6.40316) * x + 4.70158);
}

// RANGE: -0.100151 <= y <= 1.100151
// lo: (0.24061, -0.100151)
// hi: (0.75939, 1.100151)
float easeInOutBack(float x) {
    float f;
    x -= 0.5;
    f = 14.379638 * abs(x) - 16.379638;
    f = f * abs(x) + 5.5949095;
    return f * x + 0.5;
}

// RANGE: -0.372428 <= y <= 1 (0.86526, -0.372428)
float easeInElastic(float x) {
    const float pi = 3.14159265359;
    const float sin_m = 20.0 * pi / 3.0;
    const float sin_b = -43.0 * pi / 6.0;
    const float m = -2048.0 / 2049.0;
    const float b = 1.0 / 2049.0;

    float f = exp2(x * 10 - 10) * sin(sin_m * x + sin_b);
    return f * m + b;
}

// RANGE: 0 <= y <= 1.372428 (0.13474, 1.372428)
float easeOutElastic(float x) {
    const float pi = 3.14159265359;
    const float sin_m = 20.0 * pi / 3.0;
    const float sin_b = -pi * 0.5;
    const float m = 2.0 / 2049.0;
    const float b = 2048.0 / 2049.0;

    float f = exp2(-10 * x + 10) * sin(sin_m * x + sin_b);
    return f * m + b;
}

// RANGE: -0.118453 <= y <= 1.118453
// lo: (0.404001, -0.118453)
// hi: (0.595999, 1.118453)
float easeInOutElastic(float x) {
    const float pi = 3.14159265359;
    const float sin_m = 80.0 * pi / 9.0;
    const float sin_b = -89.0 * pi / 18.0;
    const float m = 512.0 / (1024.0 - sin(pi / 18.0));
    const float b = 0.5;

    float v = 20 * x - 10;
    float s = 1 - 2 * float(v < abs(v)); //sign(v)
    float f = exp2(-abs(v)) * sin(sin_m * x + sin_b);
    return (f * s + s) * m + b;
}

float easeInBounce(float x) {
    const float M = -121.0 / 16.0;
    const float4 OFFSET = float4(11, 44, 110, 242) / 16.0;
    const float4 BIAS = float4(0, 3, 21, 105) / 16.0;

    float4 p = (M * x + OFFSET) * x - BIAS;
    return max(max(p.x, p.y), max(p.z, p.w));
}

float easeOutBounce(float x) {
    const float M = 121.0 / 16.0;
    const float4 OFFSET = float4(0, 132, 198, 231) / 16.0;
    const float4 BIAS = float4(0, 3, 6, 63.0 / 8.0);

    float4 p = (M * x - OFFSET) * x + BIAS;
    return min(min(p.x, p.y), min(p.z, p.w));
}

float easeInOutBounce(float x) {
    float v = x * 2 - 1;
    float f = sign(v) * easeOutBounce(abs(v));
    return f * 0.5 + 0.5;
}

// EaseInOut but generalized to the `n`th power.
// easeInOutNth(x, 2) -> Quad
// easeInOutNth(x, 3) -> Cubic
// easeInOutNth(x, 4) -> Quart
// etc.
float easeInOutNth(float x, float n) {
    //x = saturate(x);
    float2 v = min(float2(2 - 2 * x, 2 * x), 1);
    return (pow(v.y, n) - pow(v.x, n)) * 0.5 + 0.5;
}

#endif //VIVIFY_EASING_FUNCTIONS_INCLUDED
