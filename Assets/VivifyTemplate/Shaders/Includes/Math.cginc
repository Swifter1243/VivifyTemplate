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
        rotate2D(a, p.xz)
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
        p.x * (cy*cx) + p.x * (sz*sy*cx) + p.x * (cz*sy*cx),
        p.y * (cy*sx) + p.y * (sz*sy*sx + cz*cx) + p.y * (cz*sy*sx - sz*cx),
        p.z * (-sy) + p.z * (sz*cy) + p.z * (cz*cy)
    );
}

float3 lineXZPlaneIntersect(float3 linePoint, float3 lineDir, float planeY)
{
    lineDir = normalize(lineDir);
    float t = (planeY - linePoint.y) / lineDir.y;
    return linePoint + t * lineDir;
}

float3 closestPointOnLine(float3 linePoint1, float3 linePoint2, float3 targetPoint)
{
    float3 lineDirection = normalize(linePoint2 - linePoint1);
    float3 toTarget = targetPoint - linePoint1;

    float projection = dot(toTarget, lineDirection);
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

float3 rotatePointLookat(float3 forward, float3 up, float3 p)
{
    // Compute the right vector as the cross product of forward and up
    float3 right = cross(forward, up);
    forward = cross(up, right);

    // Create the transformation matrix
    float3x3 m = matrixFromBasis(right, up, forward);

    return mul(m, p);
}