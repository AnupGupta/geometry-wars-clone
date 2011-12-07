// Copyright 2010 Giovanni Botta

// This file is part of GeomClone.

// GeomClone is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.

// GeomClone is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.

// You should have received a copy of the GNU Lesser General Public License
// along with GeomClone.  If not, see <http://www.gnu.org/licenses/>.


uniform extern texture ColorTexture;
uniform extern float2 TargetSize;
uniform extern float GlowScalar;
uniform extern int numPixel;

sampler2D ColorSampler = sampler_state
{
	Texture = (ColorTexture);
	MinFilter = LINEAR;
	MagFilter = LINEAR;
	MipFilter = LINEAR;
};

float4 BlurVertical(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 texCoordSample = 0;
	float4 color = tex2D(ColorSampler, TexCoords);
	float4 cOut = color;
	
	int N=(numPixel%80)/2;
	float4 temp;

	for(int i=1; i<=N; i++)
	{
	
		texCoordSample.x = TexCoords.x;
		texCoordSample.y = TexCoords.y + i/TargetSize.y;
		temp = tex2D(ColorSampler, texCoordSample);
				
		cOut.rgb += N*temp.rgb/i;
		
		texCoordSample.x = TexCoords.x;
		texCoordSample.y = TexCoords.y -i/TargetSize.y;
		temp = tex2D(ColorSampler, texCoordSample);
				
		cOut.rgb += N*temp.rgb/i;
		
	}
	
	cOut.rgb = GlowScalar*cOut/(numPixel+1)+color.rgb;

	return cOut;
}

float4 BlurHorizontal(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 texCoordSample = 0;
	float4 color = tex2D(ColorSampler, TexCoords);
	float4 cOut = color;
	//float4 cOut = tex2D(ColorSampler, TexCoords);
	
	int N=(numPixel%80)/2;
	float4 temp;
	
	for(int i=1; i<=N; i++)
	{
		texCoordSample.x = TexCoords.x + i/TargetSize.x;
		texCoordSample.y = TexCoords.y;
		temp = tex2D(ColorSampler, texCoordSample);
		
		cOut.rgb += N/2*temp.rgb/i;
		
		texCoordSample.x = TexCoords.x - i/TargetSize.x;
		texCoordSample.y = TexCoords.y;
		temp = tex2D(ColorSampler, texCoordSample);
		
		cOut.rgb += N/2*temp.rgb/i;
	}
	
	cOut.rgb = GlowScalar*cOut/(numPixel+1)+color.rgb;

	return cOut;
}


float4 BlurVertical2(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 texCoordSample = 0;
	float4 color = tex2D(ColorSampler, TexCoords);
	float4 cOut = color;
	float4 temp;
	
	texCoordSample.x = TexCoords.x;
	
	for(int i=0; i<=20; i++)
	{	
		if(i!=10){
			texCoordSample.y = TexCoords.y + (i-10)/TargetSize.y;
			temp = tex2D(ColorSampler, texCoordSample);
			cOut.rgb += 10*temp.rgb/abs(i-10);
		}
	}
	
	cOut.rgb = GlowScalar*cOut/21+color.rgb;

	return cOut;
}

float4 BlurHorizontal2(float2 TexCoords : TEXCOORD0) : COLOR0
{
	float2 texCoordSample = 0;
	float4 color = tex2D(ColorSampler, TexCoords);
	float4 cOut = color;
	float4 temp;
	
	texCoordSample.y = TexCoords.y;
	
	for(int i=0; i<=20; i++)
	{
		if(i!=10){
			texCoordSample.x = TexCoords.x + (i-10)/TargetSize.x;		
			temp = tex2D(ColorSampler, texCoordSample);	
			cOut.rgb += 10*temp.rgb/abs(i-10);
		}
	}
	
	cOut.rgb = GlowScalar*cOut/21+color.rgb;

	return cOut;
}

technique Blur
{
    pass P0
    {
        PixelShader = compile ps_3_0 BlurVertical();
        PixelShader = compile ps_2_0 BlurVertical2();
    }
    pass P1
    {
        PixelShader = compile ps_3_0 BlurHorizontal();
        PixelShader = compile ps_2_0 BlurHorizontal2();
    }
}