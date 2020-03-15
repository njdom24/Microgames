sampler s0;
texture2D palette;
float time;

float4 FillColor(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	//float3 tint;
	float4 color = tex2D(s0, texCoord);
	if (color.a)
		color.rgb += time;
	color.rgb *= time * 6;
	
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_5_0 FillColor();
	}
}