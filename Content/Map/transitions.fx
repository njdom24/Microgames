sampler s0;
float time;

float4 Interlace(float4 pos : SV_POSITION, float4 color1 : COLOR0, float2 texCoord : TEXCOORD0) : SV_TARGET0
{
	//texCoord.y = texCoord.y + time * 0.1;
	
	texCoord.x = texCoord.x;// * (320.0/256.0);
	texCoord.y = texCoord.y;// * (180.0/256.0);
	//float2 texCoordLeft = texCoord;
	//texCoord.x = texCoord.x + time * 1;
	//float4 color = tex2D(s0, texCoord);
	

	//INTERLACED METHOD BELOW
	//if (texCoord.x*256.0 % 2.0 >= -1.0 && texCoord.x*256.0 % 2.0 <= 1.0)//Selects every other vertical line

	if (texCoord.y*180 % 2.0 >= -1.0 && texCoord.y*180 % 2.0 <= 1.0)
	{
		texCoord.x = texCoord.x + 0.4*time;
	}
	else
	{
		texCoord.x = texCoord.x - 0.4*time;
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
};