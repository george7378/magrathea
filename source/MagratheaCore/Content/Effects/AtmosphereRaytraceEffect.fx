#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_3
	#define PS_SHADERMODEL ps_4_0_level_9_3
#endif


/////////////
//Constants//
/////////////
static int NumAtmosphereSamples = 3;


////////////////////
//Global variables//
////////////////////
float4x4 ViewProjection;

float PlanetVectorLengthSquared;
float AtmosphereRadiusSquared;
float AtmosphereDepth;

float3 PlanetVector;
float3 CameraPosition;
float3 LightDirection;
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
	float4 Position        : POSITION0;
	float3 SampleDirection : TEXCOORD0;
};


/////////////
//Functions//
/////////////
float3 AtmosphereIntersect(float3 sampleDirection)
{
	float tca = dot(PlanetVector, sampleDirection);
	float d2 = PlanetVectorLengthSquared - tca*tca;
	if (d2 > AtmosphereRadiusSquared)
	{
		return float3(0.0f, 0.0f, 0.0f);
	}

	float thc = sqrt(AtmosphereRadiusSquared - d2);
	float t0 = tca - thc;
	float t1 = tca + thc;
	if (t0 < 0.0f && t1 < 0.0f)
	{
		return float3(0.0f, 0.0f, 0.0f);
	}

	return float3(1.0f, max(0.0f, t0), t1);
}


///////////
//Shaders//
///////////
VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;

	output.Position = mul(input.Position, ViewProjection);
	output.SampleDirection = input.Position.xyz;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 sampleDirectionNormalised = normalize(input.SampleDirection);

	float averageSampleDensity = 0.0f;

	float3 atmosphereIntersect = AtmosphereIntersect(sampleDirectionNormalised);
	if (atmosphereIntersect.x > 0.0f)
	{
		float samplePositionDelta = (atmosphereIntersect.z - atmosphereIntersect.y)/(NumAtmosphereSamples + 1);

		float totalSampleDensity = 0.0f;
		for (int i = 1; i <= NumAtmosphereSamples; i++)
		{
			float3 samplePosition = CameraPosition + (atmosphereIntersect.y + i*samplePositionDelta)*sampleDirectionNormalised;
			
			float sampleRadius = length(samplePosition);
			if (sampleRadius > 1.0f)
			{
				float sampleDepthNormalised = 1.0f - (sampleRadius - 1.0f)/AtmosphereDepth;
				float sampleDensity = exp(sampleDepthNormalised) - 1.0f;

				float lightAltitudeAngle = acos(dot(normalize(samplePosition), LightDirection)) - asin(1.0f/sampleRadius);
				float sampleWeight = min(1.0f, lightAltitudeAngle/0.35f + 1.0f);

				totalSampleDensity += sampleDensity*sampleWeight;
			}
		}

		averageSampleDensity = totalSampleDensity/NumAtmosphereSamples;
	}

	float3 scatteredLightColour = saturate(AtmosphereColour*averageSampleDensity);

	float4 finalColour = float4(scatteredLightColour, saturate(max(max(scatteredLightColour.r, scatteredLightColour.g), scatteredLightColour.b)/0.5f));

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