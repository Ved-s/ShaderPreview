sampler2D textureSampler : register(S0);
float blurDist;

float4 MainPS(float2 pos : TEXCOORD0) : COLOR
{
    float2 diff = float2(.5, .5) - pos;
    float len = length(diff) * 2;
    if (len < 1 - blurDist)
        return tex2D(textureSampler, pos);
    if (len < 1)
        return tex2D(textureSampler, pos) * 1-(len - (1-blurDist)) / blurDist;
    return 0;
}

technique Main
{
	pass P0
	{
		PixelShader = compile ps_3_0 MainPS();
	}
};