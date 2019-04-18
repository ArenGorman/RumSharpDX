#include "./Include/Constants.hlsl"
#include "./Include/Structures.hlsl"
#include "./PBRLightSurface.hlsl"
#include "./Include/Layouts.hlsl"

cbuffer cbPerObjectBuffer : register(b0)
{
    CBufferPerObjectStruct cbPerObject;
}

cbuffer cbPerFrameBuffer : register(b1)
{
    CBufferPerFrameStruct cbPerFrame;
}

cbuffer Lights : register(b2)
{
    LightBuffer LightData[MaxLightsCount];
}

SamplerState Sampler : register(s0);
Texture2D AlbedoMap : register(t0);
Texture2D NormalMap : register(t1);
Texture2D RoughnessMap : register(t2);
Texture2D MetallicMap : register(t3);
Texture2D OcclusionMap : register(t4);
TextureCube RaddianceEnvMap : register(t5);
TextureCube IrradianceEnvMap : register(t6);

COMMON_PS_IN VSMain(COMMON_VS_IN Input)
{
    COMMON_PS_IN Output = (COMMON_PS_IN) 0;
    
    Input.pos.w = 1.0f;
    Output.pos = mul(Input.pos, cbPerObject.WorldViewProjMatrix);
    Output.posWS = mul(Input.pos, cbPerObject.WorldMatrix);
    
    Output.color = Input.color;

    Output.uv0 = float2(Input.uv0.x, 1.0 - Input.uv0.y);
    Output.uv0.x *= cbPerObject.textureTiling.x;
    Output.uv0.x += cbPerObject.textureShift.x;
    Output.uv0.y *= cbPerObject.textureTiling.y;
    Output.uv0.y += cbPerObject.textureShift.y;
    Output.uv1.xy = Input.uv1.xy;

    Output.normal = normalize(mul(float4(Input.normal.xyz, 0), cbPerObject.WorldMatrix));
    Output.tangent = normalize(mul(float4(Input.tangent.xyz, 0), cbPerObject.WorldMatrix));
 
    return Output;
}

float4 PSMain(COMMON_PS_IN Input) : SV_Target
{
    float unlit = cbPerObject.optionsMask1.g;
    float nonShadow = cbPerObject.optionsMask1.b;

    float3 AlbedoValue = cbPerObject.AlbedoColor.rgb;
    if (cbPerObject.optionsMask0.r > 0)
    {
        AlbedoValue = AlbedoMap.Sample(Sampler, Input.uv0.xy).rgb;
    }

    if (unlit > 0.0f)
    {
        return float4(AlbedoValue, 1.0f);
    }

    float3 NormalValue = normalize(Input.normal.xyz);
    if (cbPerObject.optionsMask0.g > 0)
    {
        NormalValue = NormalMap.Sample(Sampler, Input.uv0.xy).xyz * 2.0f - 1.0f;
        float3 binormal = CalcBinormal(Input.normal.xyz, Input.tangent.xyz);
        NormalValue = (NormalValue.x * Input.tangent.xyz) + (NormalValue.y * binormal) + (NormalValue.z * Input.normal.xyz);
        NormalValue = normalize(NormalValue);
    }
    //return float4((NormalValue + 1.0f) * 0.5f, 1.0f);

    float RoughnessValue = cbPerObject.Roughness;
    if (cbPerObject.optionsMask0.b > 0)
    {
        RoughnessValue = RoughnessMap.Sample(Sampler, Input.uv0.xy).r;
    }
    //return RoughnessValue;

    float MetallicValue = cbPerObject.Metallic;
    if (cbPerObject.optionsMask0.a > 0)
    {
        MetallicValue = MetallicMap.Sample(Sampler, Input.uv0.xy).r;
    }
    //return MetallicValue;

    float OcclusionValue = 1.0f;
    if (cbPerObject.optionsMask1.r > 0)
    {
        OcclusionValue = OcclusionMap.Sample(Sampler, Input.uv0.xy).r;
    }
    //return OcclusionValue;

    float shadowDepthValue = 1.0;
    /*if (nonShadow < 1.0f) {
        float4 lightViewPosition = mul(input.worldPos, LightData[0].lightViewProjMatrix);
        shadowDepthValue = GetShadowOneSample(GetShadowMapCoordinates(lightViewPosition), ShadowMap, ShadowsSampler);
    }*/
    
    float3 V = normalize(cbPerFrame.CameraPos.xyz - Input.posWS.xyz);

    float3 color = LightSurface(V, NormalValue, 1, LightData, AlbedoValue, RoughnessValue, MetallicValue, OcclusionValue, RaddianceEnvMap, IrradianceEnvMap, Sampler, Input.posWS.xyz);
    //float3 color = float3(Input.uv0.xy, 0);
    return float4(color, 1.0) * shadowDepthValue;
}