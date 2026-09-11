#ifndef VIVIFY_MATH_FUNCTIONS_INCLUDED
#define VIVIFY_MATH_FUNCTIONS_INCLUDED

// Transforms a position in object/local space to worldspace
float3 localToWorld(float3 pos)
{
    return mul(unity_ObjectToWorld, float4(pos, 1));
}

// Transforms a position in worldspace to object/local space
float3 worldToLocal(float3 pos)
{
    return mul(unity_WorldToObject, float4(pos, 1));
}

// Constructs a vector from the worldspace camera position (as the tail) towards the `worldPos` position (as the tip)
float3 viewVectorFromWorld(float3 worldPos)
{
    return worldPos - _WorldSpaceCameraPos;
}

// Transforms an object/local space position into world space, and returns the vector from the camera to that position
float3 viewVectorFromLocal(float3 localPos)
{
    return viewVectorFromWorld(localToWorld(localPos));
}

// Projects the viewVector onto a plane 1 unit forward
float3 unwarpViewVector(float3 viewVector)
{
    return viewVector / dot(viewVector, unity_WorldToCamera._m20_m21_m22);
}

float3 viewVectorFromUV(float2 uv)
{
    float3 viewDir = mul(unity_CameraInvProjection, float4(uv * 2.0 - 1.0, 0, 1)).xyz;
    viewDir.z = -viewDir.z;
    return mul(unity_CameraToWorld, float4(viewDir, 0)).xyz;
}

// Returns the cameras worldspace forward direction
float3 getCameraForward()
{
    return mul((float3x3)unity_CameraToWorld, float3(0,0,1));
}

// Returns the direction towards the current light in context (typically the directional sun light)
float3 getLightDirection()
{
    return normalize(_WorldSpaceLightPos0.xyz);
}

// Samples the currently active reflection probe, in the direction of `viewVector`
float4 sampleReflectionProbe(float3 viewVector)
{
    return float4(DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, viewVector, 0), unity_SpecCube0_HDR), 0);
}

// Rotates point `p` counter-clockwise by `a` radians
// Positive rotations will rotate the X axis towards the Y axis
float2 rotate2D(float a, float2 p)
{
    float c = cos(a);
    float s = sin(a);
    return mul(float2x2(c, -s, s, c), p);
}

// Rotates point `p` around the X axis by `a` radians
float3 rotateX(float a, float3 p)
{
    return float3(
        p.x,
        rotate2D(a, p.yz)
    );
}

// Rotates point `p` around the Y axis by `a` radians
float3 rotateY(float a, float3 p)
{
    float2 xz = rotate2D(a, p.xz);

    return float3(
        xz.x,
        p.y,
        xz.y
    );
}

// Rotates point `p` around the Z axis by `a` radians
float3 rotateZ(float a, float3 p)
{
    return float3(
        rotate2D(a, p.xy),
        p.z
    );
}

// Rotates a point `p` by radian angles from `a`
float3 rotatePoint(float3 a, float3 p)
{
    float3 c = cos(a);
    float3 s = sin(a);
    float3x3 rotor = float3x3(
        c.y*c.x, s.z*s.y*c.x - c.z*s.x, c.z*s.y*c.x + s.z*s.x,
        c.y*s.x, s.z*s.y*s.x + c.z*c.x, c.z*s.y*s.x - s.z*c.x,
        -s.y, s.z*c.y, c.z*c.y
    );
    return mul(rotor, p);
}

// Projects the ray from `linePoint` towards `lineDir` onto the plane at with y=`planeY`
// linePoint: The starting point of the line, (for instance, your cameras world position)
// lineDir: The direction of the line, or the endpoint of the line (for instance, your i.vertex in worldspace)
// planeY: The y level of the XZ plane
// Note: `lineDir` does not need to be normalized, as the scale of the direction vector is accounted for in the division.
float3 lineXZPlaneIntersect(float3 linePoint, float3 lineDir, float planeY)
{
    float t = (planeY - linePoint.y) / lineDir.y;
    return linePoint + t * lineDir;
}

float3 linePlaneIntersect(float3 rayPos, float3 rayDir, float3 planePos, float3 planeNormal)
{
    float3 localCam = planePos - rayPos;
    float t = dot(localCam, planeNormal) / dot(rayDir, planeNormal);
    return rayPos + t * rayDir;
}

// Returns the cosine of the angle between two vectors.
// -1 if the vectors are facing the complete opposite way from each other
// 0 if the vectors are 90 degrees from each other (orthogonal)
// +1 if the vectors are facing the exact same direction
// Note: if you want the angle in radians, use acos(angleBetweenVectors(a, b))
// keep in mind acos is expensive though, but if you're doing cos(acos(angle)), they will cancel.
float angleBetweenVectors(float2 a, float2 b)
{
    return dot(a, b) * rsqrt(dot(a,a) * dot(b,b));
}

float angleBetweenVectors(float3 a, float3 b)
{
    return dot(a, b) * rsqrt(dot(a,a) * dot(b,b));
}

// https://www.youtube.com/watch?v=PMltMdi1Wzg
// Projects a point `p` onto a line from `linePoint1` to `linePoint2`
// feel free to saturate `t` if you dont want it to go past the endpoints of the line.
float3 closestPointOnLine(float3 linePoint1, float3 linePoint2, float3 p)
{
    float3 ba = linePoint2 - linePoint1;
    float3 pa = p - linePoint1;
    float t = dot(ba, pa) / dot(ba, ba);
    return lerp(linePoint1, linePoint2, t);
}

// Constructs a 3x3 matrix from column vectors `x`, `y`, `z`
float3x3 matrixFromBasis(float3 x, float3 y, float3 z)
{
    return float3x3(
        x.x, y.x, z.x,
        x.y, y.y, z.y,
        x.z, y.z, z.z
    );
}

// Returns the transform.position or "pivot" position of the mesh in worldspace
float3 getObjectTransformPos()
{
    return unity_ObjectToWorld._m03_m13_m23;
}

// Constructs a vector from the objects transform pivot position to the cameras worldspace position
float3 directionToCamera()
{
    return _WorldSpaceCameraPos - getObjectTransformPos();
}

// Creates a lookAt matrix based on a `forward` direction, and applies the rotation to a point `p`
float3 rotateLook(float3 forward, float3 p)
{
    forward = normalize(forward);

    float3 up = float3(0,1,0);

    float3 right = normalize(cross(forward, up));
    up = cross(right, forward);

    float3x3 m = matrixFromBasis(right, up, forward);

    return -mul(m, p);
}

// Applies rotateLook but local to an `up` vector, defining the normal of the rotation plane.
float3 rotateLookOnAxis(float3 forward, float3 up, float3 p)
{
    forward = normalize(forward);
    up = normalize(up);

    float3 right = normalize(cross(forward, up));
    forward = cross(up, right);

    float3x3 m = matrixFromBasis(right, up, forward);

    return -mul(m, p);
}

#endif //VIVIFY_MATH_FUNCTIONS_INCLUDED
