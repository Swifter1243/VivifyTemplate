// https://easings.net/

float easeInSine(float x) {
    return 1 - cos((x * UNITY_PI) / 2);
}

float easeOutSine(float x) {
    return sin((x * UNITY_PI) / 2);
}

float easeInOutSine(float x) {
    return - (cos(UNITY_PI * x) - 1) / 2;
}

float easeInQuad(float x) {
    return x * x;
}

float easeOutQuad(float x) {
    return 1 - (1 - x) * (1 - x);
}

float easeInOutQuad(float x) {
    return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
}

float easeInCubic(float x) {
    return x * x * x;
}

float easeOutCubic(float x) {
    return 1 - pow(1 - x, 3);
}

float easeInOutCubic(float x) {
    return x < 0.5 ? 4 * x * x * x : 1 - pow(-2 * x + 2, 3) / 2;
}

float easeInQuart(float x) {
    return x * x * x * x;
}

float easeOutQuart(float x) {
    return 1 - pow(1 - x, 4);
}

float easeInOutQuart(float x) {
    return x < 0.5 ? 8 * x * x * x * x : 1 - pow(-2 * x + 2, 4) / 2;
}

float easeInQuint(float x) {
    return x * x * x * x * x;
}

float easeOutQuint(float x) {
    return 1 - pow(1 - x, 5);
}

float easeInOutQuint(float x) {
    return x < 0.5 ? 16 * x * x * x * x * x : 1 - pow(-2 * x + 2, 5) / 2;
}

float easeInExpo(float x) {
    return pow(2, 10 * x - 10);
}

float easeOutExpo(float x) {
    return pow(2, -10 * x);
}

float easeInOutExpo(float x) {
    return x == 0
        ? 0
        : x == 1
        ? 1
        : x < 0.5
        ? pow(2, 20 * x - 10) / 2
        : (2 - pow(2, -20 * x + 10)) / 2;
}

float easeInCirc(float x) {
    return 1 - sqrt(1 - pow(x, 2));
}

float easeOutCirc(float x) {
    return sqrt(1 - pow(x - 1, 2));
}

float easeInOutCirc(float x) {
    return x < 0.5
        ? (1 - sqrt(1 - pow(2 * x, 2))) / 2
        : (sqrt(1 - pow(-2 * x + 2, 2)) + 1) / 2;
}

float easeInBack(float x) {
    float c1 = 1.70158;
    float c3 = c1 + 1;

    return c3 * x * x * x - c1 * x * x;
}

float easeOutBack(float x) {
    float c1 = 1.70158;
    float c3 = c1 + 1;

    return 1 + c3 * pow(x - 1, 3) + c1 * pow(x - 1, 2);
}

float easeInOutBack(float x) {
    float c1 = 1.70158;
    float c2 = c1 * 1.525;

    return x < 0.5
        ? (pow(2 * x, 2) * ((c2 + 1) * 2 * x - c2)) / 2
        : (pow(2 * x - 2, 2) * ((c2 + 1) * (x * 2 - 2) + c2) + 2) / 2;
}

float easeInElastic(float x) {
    float c4 = (2 * UNITY_PI) / 3;

    return x == 0
        ? 0
        : x == 1
        ? 1
        : -pow(2, 10 * x - 10) * sin((x * 10 - 10.75) * c4);
}

float easeOutElastic(float x) {
    float c4 = (2 * UNITY_PI) / 3;

    return x == 0
        ? 0
        : x == 1
        ? 1
        : pow(2, -10 * x) * sin((x * 10 - 0.75) * c4) + 1;
}

float easeInOutElastic(float x) {
    float c5 = (2 * UNITY_PI) / 4.5;

    return x == 0
        ? 0
        : x == 1
        ? 1
        : x < 0.5
        ? -(pow(2, 20 * x - 10) * sin((20 * x - 11.125) * c5)) / 2
        : (pow(2, -20 * x + 10) * sin((20 * x - 11.125) * c5)) / 2 + 1;
}

float easeOutBounce(float x) {
    float n1 = 7.5625;
    float d1 = 2.75;

    if (x < 1 / d1) {
        return n1 * x * x;
    } else if (x < 2 / d1) {
        return n1 * (x -= 1.5 / d1) * x + 0.75;
    } else if (x < 2.5 / d1) {
        return n1 * (x -= 2.25 / d1) * x + 0.9375;
    } else {
        return n1 * (x -= 2.625 / d1) * x + 0.984375;
    }
}

float easeInBounce(float x) {
    return 1 - easeOutBounce(1 - x);
}

float easeInOutBounce(float x) {
    return x < 0.5
        ? (1 - easeOutBounce(1 - 2 * x)) / 2
        : (1 + easeOutBounce(2 * x - 1)) / 2;
}