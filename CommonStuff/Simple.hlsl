
struct ConstantData
{
	float4x4 WorldViewProj;
};

cbuffer ConstBuf : register(b0) {
	ConstantData ConstData;
}

struct VS_IN
{
	float4 col : COLOR;
	float4 pos : POSITION;
};

struct PS_IN
{
	float4 pos : SV_POSITION;
	//float4 pos2 : POSITION;
	float4 col : COLOR;
};

PS_IN VSMain( VS_IN input )
{
	PS_IN output = (PS_IN)0;
	
	output.pos = mul(float4(input.pos.xyz, 1.0f), ConstData.WorldViewProj);
	//output.pos2 = output.pos;
	output.col = input.col;
	
	return output;
}

float4 PSMain( PS_IN input ) : SV_Target
{
	float4 col = input.col;

	//if (input.pos2.x > 0) col = float4(1.0f, 1.0f, 1.0f, 1.0f);

	return col;
}
