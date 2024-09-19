﻿#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif


////////////////////
//Global variables//
////////////////////
float4x4 World;
float4x4 WorldViewProjection;

float LightPower;
float AmbientLightPower;

float3 LightDirection;


//////////////////
//I/O structures//
//////////////////
struct VertexShaderInput
{
	float4 Position   : POSITION0;
	float3 Normal     : NORMAL0;
	float4 BaseColour : COLOR0;
};

struct VertexShaderOutput
{
	float4 Position   : POSITION0;
	float3 Normal     : TEXCOORD0;
	float4 BaseColour : COLOR0;
};


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, WorldViewProjection);
	output.Normal = normalize(mul(input.Normal, (float3x3)World));
	output.BaseColour = input.BaseColour;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float diffuseLightingFactor = saturate(dot(-LightDirection, input.Normal))*LightPower;

	float4 finalColour = float4(input.BaseColour.rgb*(AmbientLightPower + diffuseLightingFactor), input.BaseColour.a);

	return finalColour;
}

technique TerrainTechnique
{
	pass Pass1
	{
		//Fillmode = Wireframe;

		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}