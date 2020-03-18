sampler s0;
texture2D palette;
sampler paletteSampler = sampler_state { Texture = <palette>; };
float time;
float paletteWidth;
float2 VerticalStretch(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.y = texCoord.y + 0.4*sin(texCoord.y + time * 1);
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float2 HorizontalStretch(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.x = texCoord.x + 0.4*sin(texCoord.x + time * 2);
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float2 HorizontalScroll(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.x = texCoord.x + time*0.25;
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float2 VerticalScroll(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.y = texCoord.y + time * 0.25;
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float2 SineWave(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	if(texCoord.y % 0.1 != 0)
		texCoord.y = texCoord.y + 0.2*sin(10*texCoord.x + time*2)+0.1;
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float2 HorizontalSineWave(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.x = texCoord.x + 0.2*sin(10 * texCoord.y + time * 2) + 0.1;
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float4 InvertColors(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	color.rgb = 1-color.rgb;

	return color;
}

float4 HighContrast(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	float4 color = tex2D(s0, texCoord);
	float high = .5;
	float low = .4;

	if (color.r > high) color.r = 1;
	else if (color.r < low) color.r = 0;

	if (color.g > high) color.g = 1;
	else if (color.g < low) color.g = 0;

	if (color.b > high) color.b = 1;
	else if (color.b < low) color.b = 0;

	return color;
}

float2 InterlacedInvert(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.x = texCoord.x + 0.2*sin(10 * texCoord.y + time * 2) + 0.1;
	float4 color = tex2D(s0, texCoord);

	return texCoord;
}

float4 Layer1(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	//Vertical Scroll
	texCoord = VerticalScroll(pos, color1, texCoord);
	//Horizontal Stretch
	texCoord = HorizontalStretch(pos, color1, texCoord);

	float4 color = tex2D(s0, texCoord);

	return color;
}

float4 Layer2(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	//Horizontal Scroll
	texCoord.x = texCoord.x + time * 0.1;
	//Vertical Scroll
	texCoord.y = texCoord.y + time * 0.2;
	//Sine Wave
	texCoord.y = texCoord.y + 0.2*sin(10 * texCoord.x + time * 2) + 0.1;

	float4 color = tex2D(s0, texCoord);

	return color;
}

float4 Layer3(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	//Makes smaller images maintain their size and wrap if necessary
	texCoord.x = texCoord.x * (320.0 / 256.0) - 0.3;
	texCoord.y = texCoord.y * (180.0 / 256.0) - 0.095;
	//Distortions
	texCoord.y = texCoord.y + time * 0.2;
	texCoord.y = texCoord.y + 0.1*sin(texCoord.y*4 + time * 1);
	//Maps color indexes to respective indexes in a palette
	float4 color = 255.0*tex2D(s0, texCoord) + 3.0;
	float4 mask = tex2D(paletteSampler, (1.0/paletteWidth)*(color.r + floor(10*time)) + 0.25);

	return mask;
}

float4 Layer5(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.y = texCoord.y + time * 0.1;
	
	texCoord.x = texCoord.x * (320.0/256.0);
	texCoord.y = texCoord.y * (180.0/256.0);
	float2 texCoordLeft = texCoord;
	//texCoord.x = texCoord.x + time * 1;
	//float4 color = tex2D(s0, texCoord);
	

	//INTERLACED METHOD BELOW
	//if (texCoord.x*256.0 % 2.0 >= -1.0 && texCoord.x*256.0 % 2.0 <= 1.0)//Selects every other vertical line
	/*
	if (texCoord.y*256.0 % 2.0 >= -1.0 && texCoord.y*256.0 % 2.0 <= 1.0)
	{
		texCoord.x = texCoord.x + 0.2*sin(10 * texCoord.y + time * 2) + 0.1;
	}
	else
	{
		texCoord.x = texCoord.x - 0.2*sin(10 * texCoord.y + time * 2) - 0.1;
	}
	*/
	texCoordLeft.x = texCoord.x - 0.2*sin(10 * texCoord.y + time * 2);
	texCoord.x = texCoord.x + 0.2*sin(10 * texCoord.y + time * 2);
	
	float4 color = 0.5*tex2D(s0, texCoord) + 0.5*tex2D(s0, texCoordLeft);
	
	return color;
}

float4 Interlace(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	texCoord.y = texCoord.y + time * 0.1;
	
	texCoord.x = texCoord.x * (320.0/256.0);
	texCoord.y = texCoord.y * (180.0/256.0);
	//float2 texCoordLeft = texCoord;
	//texCoord.x = texCoord.x + time * 1;
	//float4 color = tex2D(s0, texCoord);
	

	//INTERLACED METHOD BELOW
	//if (texCoord.x*256.0 % 2.0 >= -1.0 && texCoord.x*256.0 % 2.0 <= 1.0)//Selects every other vertical line

	if (texCoord.y*256.0 % 2.0 >= -1.0 && texCoord.y*256.0 % 2.0 <= 1.0)
	{
		texCoord.x = texCoord.x + 0.2*sin(10 * texCoord.y + time * 2) + 0.1;
	}
	else
	{
		texCoord.x = texCoord.x - 0.2*sin(10 * texCoord.y + time * 2) - 0.1;
	}
	
	float4 color = tex2D(s0, texCoord);
	
	return color;
}

technique Technique1
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Interlace();
	}

	pass Pass2
	{
		PixelShader = compile ps_3_0 Interlace();
	}
}

technique Technique2
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Layer5();
	}
	pass Pass2
	{
		PixelShader = compile ps_3_0 Layer5();
	}
}

technique Technique3
{
	pass Pass1
	{
		PixelShader = compile ps_3_0 Layer3();
	}

	pass Pass2
	{
		PixelShader = compile ps_3_0 Layer3();
	}
};