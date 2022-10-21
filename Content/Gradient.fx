float4 start, end;

float4 MainPS(float2 pos : TEXCOORD0) : COLOR
{
    return lerp(start, end, pos.xxxx);
}

technique Main
{
	pass Main
	{
		PixelShader = compile ps_3_0 MainPS();
	}
};