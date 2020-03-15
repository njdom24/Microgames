sampler s0;
texture lightMask;
sampler lightSampler = sampler_state { Texture = <lightMask>; };
float4 PixelShaderFunction(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	float4 mask = tex2D(lightSampler, float2(pos.x/400, pos.y/240));
	if (color.a)
		if (pos.x > 0)
		{
			color.rgb = 1 - color.rgb;
		}
	//color.a = mask.a;
	return color;
}

float4 TextboxChangeOld(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	if (!color.r)
	{
		/*
		color.r = 0;
		color.g = 0.5;
		color.b = 0.1;
		*/
		color.r = 0.5;
		color.g = 0;
		color.b = 0;
	}
	//color.a = mask.a;
	return color;
}

float4 TextboxChange(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	if(color.a)
	{
		if (color.r < 0.1)
		{
			/*
			color.r = 0;
			color.g = 0.5;
			color.b = 0.1;
			*/
			color.r = 0.5;
			color.g = 0;
			color.b = 0;
		}
	}
	//color.a = mask.a;
	return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_5_0 PixelShaderFunction();
    }
	pass Pass2
	{
		PixelShader = compile ps_5_0 TextboxChange();
	}
}