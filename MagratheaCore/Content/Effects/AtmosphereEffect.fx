#if OPENGL
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
float4x4 ViewProjection;

float CosPlanetAngularRadius;
float FalloffGradient;

float3 PlanetDirection;
float3 AtmosphereColour;


//////////////////
//I/O structures//
//////////////////
struct VertexShaderInput
{
	float4 Position : POSITION0;
};

struct VertexShaderOutput
{
	float4 Position          : POSITION0;
	float3 SamplingDirection : TEXCOORD0;
};


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, ViewProjection);
	output.SamplingDirection = input.Position.xyz;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float planetViewingAngleExponent = -saturate(CosPlanetAngularRadius - dot(PlanetDirection, normalize(input.SamplingDirection)));
	float3 scatteredLightColour = AtmosphereColour*exp(FalloffGradient*planetViewingAngleExponent);

	float4 finalColour = float4(scatteredLightColour, saturate(max(max(scatteredLightColour.r, scatteredLightColour.g), scatteredLightColour.b) + 0.5f));

	return finalColour;
}

technique AtmosphereTechnique
{
	pass Pass1
	{
		VertexShader = compile VS_SHADERMODEL VertexShaderFunction();
		PixelShader = compile PS_SHADERMODEL PixelShaderFunction();
	}
}