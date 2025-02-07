float3 localToWorld(float3 pos)
{
    return mul(unity_ObjectToWorld, float4(pos, 1));
}

float3 worldToLocal(float3 pos)
{
    return mul(unity_WorldToObject, float4(pos, 1));
}

float3 viewVectorFromWorld(float3 worldPos)
{
    return worldPos - _WorldSpaceCameraPos;
}

float3 viewVectorFromLocal(float3 localPos)
{
    return viewVectorFromWorld(localToWorld(localPos));
}

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

float3 getCameraForward()
{
    return mul((float3x3)unity_CameraToWorld, float3(0,0,1));
}

float3 getLightDirection()
{
    return normalize(_WorldSpaceLightPos0.xyz);
}

float4 sampleReflectionProbe(float3 viewVector) 
{
    return float4(DecodeHDR(UNITY_SAMPLE_TEXCUBE_LOD(unity_SpecCube0, viewVector, 0), unity_SpecCube0_HDR), 0);
}

float2 rotate2D(float a, float2 p)
{
    float c = cos(a);
    float s = sin(a);
    return mul(float2x2(c, -s, s, c), p);
}

float3 rotateX(float a, float3 p) 
{
    return float3(
        p.x,
        rotate2D(a, p.yz)
    );
}

float3 rotateY(float a, float3 p) 
{
    float2 xz = rotate2D(a, p.xz);

    return float3(
        xz.x,
        p.y,
        xz.y
    );
}

float3 rotateZ(float a, float3 p) 
{
    return float3(
        rotate2D(a, p.xy),
        p.z
    );
}

float3 rotatePoint(float3 a, float3 p) 
{
    float cx = cos(a.x);
    float sx = sin(a.x);
    float cy = cos(a.y);
    float sy = sin(a.y);
    float cz = cos(a.z);
    float sz = sin(a.z);
    
    return float3(
        p.x * (cy*cx) + p.y * (sz*sy*cx - cz*sx) + p.z * (cz*sy*cx + sz*sx),
        p.x * (cy*sx) + p.y * (sz*sy*sx + cz*cx) + p.z * (cz*sy*sx - sz*cx),
        p.x * (-sy) + p.y * (sz*cy) + p.z * (cz*cy)
    );
}

float3 lineXZPlaneIntersect(float3 linePoint, float3 lineDir, float planeY)
{
    lineDir = normalize(lineDir);
    float t = (planeY - linePoint.y) / lineDir.y;
    return linePoint + t * lineDir;
}

float angleBetweenVectors(float2 a, float2 b)
{
    return dot(a, b) / (length(a) * length(b));
}

float angleBetweenVectors(float3 a, float3 b)
{
    return dot(a, b) / (length(a) * length(b));
}

float3 closestPointOnLine(float3 linePoint1, float3 linePoint2, float3 p)
{
    float3 lineDirection = normalize(linePoint2 - linePoint1);
    float3 toPoint = p - linePoint1;

    float projection = dot(toPoint, lineDirection);
    return linePoint1 + projection * lineDirection;
}

float3x3 matrixFromBasis(float3 x, float3 y, float3 z)
{
    return float3x3(
        x.x, y.x, z.x,
        x.y, y.y, z.y,
        x.z, y.z, z.z
    );
}

float3 directionToCamera()
{
    return _WorldSpaceCameraPos - localToWorld(0);
}

float3 rotateLook(float3 forward, float3 p)
{
    forward = normalize(forward);

    float3 up = float3(0,1,0);

    float3 right = normalize(cross(forward, up));
    up = cross(right, forward);

    float3x3 m = matrixFromBasis(right, up, forward);

    return -mul(m, p);
}

float3 rotateLookOnAxis(float3 forward, float3 up, float3 p)
{
    forward = normalize(forward);
    up = normalize(up);

    float3 right = normalize(cross(forward, up));
    forward = cross(up, right);

    float3x3 m = matrixFromBasis(right, up, forward);

    return -mul(m, p);
}