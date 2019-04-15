struct ConstantData
{
	float4x4 WorldViewProj;
};

cbuffer ConstBuf : register(b0) {
	ConstantData ConstData;
}

struct VS_IN
{
	float4 pos		: POSITION;
	float4 normal	: NORMAL;
	float4 tex		: TEXCOORD;
};

struct PS_IN
{
	float4 pos		: SV_POSITION;
	float4 normal	: NORMAL;
	float2 tex		: TEXCOORD;
};

Texture2D		DiffuseMap		: register(t0);
TextureCube     CubeMap         : register(t0);
SamplerState	Sampler			: register(s0);


PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(float4(input.pos.xyz, 1.0f), ConstData.WorldViewProj);
	output.normal = input.normal;

	output.tex = input.tex.xy;
	
	return output;
}

PS_IN VSCube(VS_IN input)
{
    PS_IN output = (PS_IN) 0;
	
    output.pos = mul(float4(input.pos.xyz, 1.0f), ConstData.WorldViewProj);
    output.normal = input.normal;
    output.tex = input.tex.xy;
	
    return output;
}


float4 PSMain(PS_IN input) : SV_Target
{
    float4 color = DiffuseMap.Sample(Sampler, float2(input.tex.x, 1.0f - input.tex.y));
    return color;
}

float4 PSCube(PS_IN input) : SV_Target
{
    float4 color = saturate(CubeMap.Sample(Sampler, -input.normal.xyz));
    return color;
}
