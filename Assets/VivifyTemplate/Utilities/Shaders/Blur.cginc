float4 getScreenCol(sampler2D s, float2 uv)
{
	return UNITY_SAMPLE_SCREENSPACE_TEXTURE(s, UnityStereoTransformScreenSpaceTex(uv));
}

float _Strength;
static int N = 25;

// adapted from https://www.shadertoy.com/view/WtKfD3
float4 blurPass(in sampler2D s, in float2 U, in float2 D)
{
	float4 O = 0;
	float r = float(N-1)/2., g, t=0., x;
	for (int k = 0; k < N; k++)
	{
		x = float(k)/r-1.;
		t += g = exp(-3*x*x );
		float4 c = getScreenCol(s, (U+_Strength*x*D));
		c = saturate(c); // TODO: Figure out how to set scene view to a normalized texture format?
		O += g * c * pow(c.a, 0.5);
	}

	return O/t;
}
