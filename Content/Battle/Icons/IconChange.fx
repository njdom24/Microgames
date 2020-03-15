sampler s0;

float4 TintIcon(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	color.rgb = color.rgb - 0.3;//darken
	float intensity = dot(color.rgb, float3(0.3, 0.59, 0.11));//grayscale intensity

	//0 = nothing, 1 = fully desaturated
	color.r = intensity * 0.7 + color.r*(1 - 0.7);
	color.g = intensity * 0.7 + color.g*(1 - 0.7);
	color.b = intensity * 0.7 + color.b*(1 - 0.7);
	
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_5_0 TintIcon();
	}
}