float4 getScreenCol(sampler2D s, float2 uv)
{
	return UNITY_SAMPLE_SCREENSPACE_TEXTURE(s, UnityStereoTransformScreenSpaceTex(uv));
}

float alphaInfluence(float a)
{
	return pow(a, 0.7);
}

float _Strength;
static int N = 25;

float4 blurPass(sampler2D s, float2 U, float2 D)
{
	float4 O = 0;
	float r = float(N-1)/2., g, t=0., x;
	for (int k = 0; k < N; k++)
	{
		x = float(k)/r-1.;
		t += g = exp(-2.*x*x );
		float4 c = getScreenCol(s, (U+_Strength*x*D));
		O += g * c * alphaInfluence(c.a);
	}

	return O/t;
}
