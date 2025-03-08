static float weights[10] =
{
	0.227f,
	0.194f,
	0.162f,
	0.131f,
	0.103f,
	0.077f,
	0.055f,
	0.037f,
	0.023f,
	0.013f
};

float4 getScreenCol(sampler2D s, float2 uv)
{
	return UNITY_SAMPLE_SCREENSPACE_TEXTURE(s, UnityStereoTransformScreenSpaceTex(uv));
}

float4 blurPass(sampler2D s, float2 uv, float2 offset)
{
	float4 result = getScreenCol(s, uv);
	float total = result;

	for (int i = 1; i < 10; i++)
	{
		float2 newOffset = offset * i;

		float4 colLeft = getScreenCol(s, uv - newOffset);
		float4 colRight = getScreenCol(s, uv + newOffset);

		result += colLeft * colLeft.a;
		result += colRight * colRight.a;
		total += 2;
	}

	result /= total;

	return result;
}
